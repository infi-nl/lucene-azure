using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.WindowsAzure.Storage.Queue;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Infi.LuceneArticle.Helpers.Queue.CloudQueue
{
    public class AzureObjectCloudQueue<T> : IAzureObjectQueue<T> {
        #region Privates

        private readonly int _maxDequeueCount;
        private readonly Microsoft.WindowsAzure.Storage.Queue.CloudQueue _queue;
        private readonly CloudBlobContainer _store;
        private readonly Collection<string> _previouslyDequeuedMessages;
        private readonly TimeSpan _visibilityTimeout;

        private static string NewMessageId() {
            return Guid.NewGuid().ToString();
        }

        #endregion

        public AzureObjectCloudQueue(string connectionString, string queueName, string containerName, int maxDequeueCount = 16, TimeSpan? visibilityTimeout = null) {
            _maxDequeueCount = maxDequeueCount;
            _visibilityTimeout = visibilityTimeout ?? TimeSpan.FromMinutes(15);
            var storageAccount = CloudStorageAccount.Parse(connectionString);

            _queue = CreateQueue(storageAccount, queueName);
            _store = CreateBlobStorage(storageAccount, containerName);
            _previouslyDequeuedMessages = new Collection<string>();
        }

        public int ApproximateQueueSize {
            get { return _queue.ApproximateMessageCount.HasValue ? _queue.ApproximateMessageCount.Value : -1; }
        }

        public void AddMessage(T message) {
            var messageId = NewMessageId();

            UploadToBlobStorage(message, messageId);
            AddToQueue(messageId);
        }

        public IAzureObjectCloudQueueMessage<T> GetMessage() {
            AzureObjectCloudQueueMessage<T> message;

            // find an acceptable message: not a previously dequeued message, and not a faulty message with high dequeue count (e.g.
            // a message without an associated blob)
            while (true) {
                var cloudQueueMessage = _queue.GetMessage(_visibilityTimeout);

                if (cloudQueueMessage == null) {
                    // No message available
                    return null;
                }

                if (_previouslyDequeuedMessages.Contains(cloudQueueMessage.AsString)) {
                    // Already dequeued, skip
                    continue;
                }

                message = CreateMessage(cloudQueueMessage);
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

        private AzureObjectCloudQueueMessage<T> CreateMessage(CloudQueueMessage cloudQueueMessage) {
            T associatedBlob;
            try {
                associatedBlob = DownloadFromBlobStorage(cloudQueueMessage.AsString);
            } catch {
                // We ignore queue messages without associated blob rather ungracefully.
                DeleteFromQueue(cloudQueueMessage);
                return null;
            }

            return new AzureObjectCloudQueueMessage<T>(cloudQueueMessage, associatedBlob);
        }

        public void CompleteMessage(IAzureObjectCloudQueueMessage<T> doneItem) {
            if (!(doneItem is AzureObjectCloudQueueMessage<T>)) {
                throw new Exception("Trying to complete a message from a different IAzureObjectQueue implementation.");
            }

            if (!_previouslyDequeuedMessages.Contains(doneItem.Id)) {
                throw new Exception("Attempting to dequeue {0}, but this item has already been deleted, or was never in the queue.");
            }

            var doneItemTyped = doneItem as AzureObjectCloudQueueMessage<T>;

            Debug.WriteLine(String.Format("Deleting queue message {0}.", doneItemTyped.Id));

            DeleteFromQueue(doneItemTyped.AzureCloudQueueMessage);
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

        private static CloudBlobContainer CreateBlobStorage(CloudStorageAccount storageAccount, string containerName) {
            var blobClient = storageAccount.CreateCloudBlobClient();
            var container = blobClient.GetContainerReference(containerName);

            container.CreateIfNotExists();

            return container;
        }

        #endregion

        #region Azure Initialization

        private static Microsoft.WindowsAzure.Storage.Queue.CloudQueue CreateQueue(CloudStorageAccount storageAccount, string queueName) {
            var queueClient = storageAccount.CreateCloudQueueClient();
            var queue = queueClient.GetQueueReference(queueName);

            queue.CreateIfNotExists();

            return queue;
        }

        private void DeleteFromQueue(CloudQueueMessage toDelete) {
            _queue.DeleteMessage(toDelete);
        }

        private void AddToQueue(string messageId) {
            _queue.AddMessage(new CloudQueueMessage(messageId));
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
