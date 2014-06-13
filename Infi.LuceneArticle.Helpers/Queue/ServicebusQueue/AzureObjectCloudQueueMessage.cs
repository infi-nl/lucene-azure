using System;
using Microsoft.ServiceBus.Messaging;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Infi.LuceneArticle.Helpers.Queue.ServicebusQueue {
    /// <summary>
    /// Represents a object in the queue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AzureObjectServiceBusQueueMessage<T> : IAzureObjectCloudQueueMessage<T> {
        private readonly string _blobStorageId;
        private BrokeredMessage ServiceBusQueueMessage { get; set; }

        /// <summary>
        /// The queued object
        /// </summary>
        public T Value { get; private set; }

        public AzureObjectServiceBusQueueMessage(BrokeredMessage serviceBusQueueMessage, string blobStorageId, T obj) {
            _blobStorageId = blobStorageId;
            ServiceBusQueueMessage = serviceBusQueueMessage;
            Value = obj;
        }

        /// <summary>
        /// An unique message ID
        /// </summary>
        public String Id {
            get { return _blobStorageId; }
        }

        /// <summary>
        /// Number of times this object has been dequeued
        /// </summary>
        public int DequeueCount {
            get { return ServiceBusQueueMessage.DeliveryCount; }
        }

        internal void Complete() {
            ServiceBusQueueMessage.Complete();
        }
    }
}