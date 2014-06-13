using System;
using Microsoft.WindowsAzure.Storage.Queue;

namespace Infi.LuceneArticle.Helpers.Queue.CloudQueue {
    /// <summary>
    /// Represents a object in the queue
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class AzureObjectCloudQueueMessage<T> : IAzureObjectCloudQueueMessage<T> {
        internal CloudQueueMessage AzureCloudQueueMessage { get; private set; }

        /// <summary>
        /// The queued object
        /// </summary>
        public T Value { get; private set; }

        public AzureObjectCloudQueueMessage(CloudQueueMessage azureCloudQueueMessage, T obj) {
            AzureCloudQueueMessage = azureCloudQueueMessage;
            Value = obj;
        }

        /// <summary>
        /// An unique message ID
        /// </summary>
        public String Id {
            get { return AzureCloudQueueMessage.AsString; }
        }

        /// <summary>
        /// Number of times this object has been dequeued
        /// </summary>
        public int DequeueCount {
            get { return AzureCloudQueueMessage.DequeueCount; }
        }
    }
}