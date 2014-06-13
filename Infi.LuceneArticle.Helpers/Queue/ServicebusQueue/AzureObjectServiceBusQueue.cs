using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.ServiceBus;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Infi.LuceneArticle.Helpers.Queue.ServicebusQueue
{
    public class AzureObjectServiceBusQueue<T> : IAzureObjectQueue<T> {
        #region Privates

        private readonly int _maxDequeueCount;
        private readonly CloudBlobContainer _store;
        private readonly Collection<string> _previouslyDequeuedMessages;
        private readonly QueueClient _queue;

        private static string NewMessageId() {
            return Guid.NewGuid().ToString();
        }

        #endregion

        public AzureObjectServiceBusQueue(string blobStorageConnectionString, string blobStorageContainerName, 
                                          string serviceBusConnectionString, string serviceBusQueueName, 
                                          int maxDequeueCount = 16) {
            _maxDequeueCount = maxDequeueCount;

            _queue = CreateQueue(serviceBusConnectionString, serviceBusQueueName);
            _store = CreateBlobStorage(blobStorageConnectionString, blobStorageContainerName);
            _previouslyDequeuedMessages = new Collection<string>();
        }

        public int ApproximateQueueSize {
            get { return _queue.Peek() != null ? 1 : 0; }
        }

        public void AddMessage(T message) {
            var messageId = NewMessageId();

            UploadToBlobStorage(message, messageId);
            AddToQueue(messageId);
        }

        public IAzureObjectCloudQueueMessage<T> GetMessage() {
            AzureObjectServiceBusQueueMessage<T> message;

            // find an acceptable message: not a previously dequeued message, and not a faulty message with high dequeue count (e.g.
            // a message without an associated blob)
            while (true) {
                var serviceBusMessage = _queue.Receive();

                if (serviceBusMessage == null) {
                    // No message available
                    return null;
                }

                // You can only read the body once, a bit ugly but necessary.
                var blobStorageId = serviceBusMessage.GetBody<string>();

                if (_previouslyDequeuedMessages.Contains(blobStorageId)) {
                    // Already dequeued, skip
                    continue;
                }

                message = CreateMessage(serviceBusMessage, blobStorageId);
                if (message == null) {
                    // Could not deserialize message, skip
                    continue;
                }

                // prevent this message from turning up, even if _visibilityTimeout has been reached, 
                // thus relaxing requirements for timely processing
                _previouslyDequeuedMessages.Add(message.Id);

                if (message.DequeueCount > _maxDequeueCount) {
                    // Note: DeleteMessage removes itself from _previouslyDequeuedMessages.
                    CompleteMessage(message);
                    continue;
                }

                return message;
            }
        }

        private AzureObjectServiceBusQueueMessage<T> CreateMessage(BrokeredMessage cloudQueueMessage, string blobStorageId) {
            T associatedBlob;
            try {
                associatedBlob = DownloadFromBlobStorage(blobStorageId);
            } catch {
                // We ignore queue messages without associated blob rather ungracefully.
                cloudQueueMessage.Complete();
                return null;
            }

            return new AzureObjectServiceBusQueueMessage<T>(cloudQueueMessage, blobStorageId, associatedBlob);
        }

        public void CompleteMessage(IAzureObjectCloudQueueMessage<T> doneItem) {
            if (!(doneItem is AzureObjectServiceBusQueueMessage<T>)) {
                throw new Exception("Trying to complete a message from a different IAzureObjectQueue implementation.");
            }

            if (!_previouslyDequeuedMessages.Contains(doneItem.Id)) {
                throw new Exception("Attempting to dequeue {0}, but this item has already been deleted, or was never in the queue.");
            }

            var doneItemTyped = doneItem as AzureObjectServiceBusQueueMessage<T>;

            Debug.WriteLine(String.Format("Deleting queue message {0}.", doneItemTyped.Id));

            doneItemTyped.Complete();
            DeleteFromBlobStorage(doneItemTyped.Id);

            _previouslyDequeuedMessages.Remove(doneItemTyped.Id);
        }

        #region Blobs

        private void DeleteFromBlobStorage(string messageId) {
             _store.GetBlockBlobReference(messageId).Delete();
        }

        private T DownloadFromBlobStorage(string messageId) {
            using (var memoryStream = new MemoryStream()) {
                _store.GetBlockBlobReference(messageId).DownloadToStream(memoryStream);
                string text = Encoding.UTF8.GetString(memoryStream.ToArray());

                return JsonConvert.DeserializeObject<T>(text, MyJsonSerializerSettings);
            }
        }

        private void UploadToBlobStorage(T message, string messageId) {
            var contents = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(message, MyJsonSerializerSettings));

            using (var fileStream = new MemoryStream(contents)) {
                _store.GetBlockBlobReference(messageId).UploadFromStream(fileStream);
            }
        }

        private static CloudBlobContainer CreateBlobStorage(string blobStorageConnectionString, string blobStorageContainerName) {
            var storageAccount = CloudStorageAccount.Parse(blobStorageConnectionString);

            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(blobStorageContainerName);

            container.CreateIfNotExists();

            return container;
        }

        #endregion

        #region Azure Initialization

        private static QueueClient CreateQueue(string serviceBusConnectionString, string serviceBusQueueName) {
            var queueDescription = new QueueDescription(serviceBusQueueName) {
                DefaultMessageTimeToLive = new TimeSpan(0, 1 /* minute */, 0)
            };

            var namespaceManager = NamespaceManager.CreateFromConnectionString(serviceBusConnectionString);

            if (!namespaceManager.QueueExists(serviceBusQueueName)) {
                namespaceManager.CreateQueue(queueDescription);
            }

            return QueueClient.CreateFromConnectionString(serviceBusConnectionString, serviceBusQueueName);
        }

        private void AddToQueue(string messageId) {
            _queue.Send(new BrokeredMessage(messageId));
        }

        #endregion

        #region JSON Settings

        private static JsonSerializerSettings MyJsonSerializerSettings {
            get {
                return new JsonSerializerSettings {
                    TypeNameHandling = TypeNameHandling.All,
                    ContractResolver = new DefaultContractResolver {
                        DefaultMembersSearchFlags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                    },
                };
            }
        }

        #endregion

    }
}
