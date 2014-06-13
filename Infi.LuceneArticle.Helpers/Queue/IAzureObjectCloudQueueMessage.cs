using System;

namespace Infi.LuceneArticle.Helpers.Queue {
    public interface IAzureObjectCloudQueueMessage<T> {
        /// <summary>
        /// The queued object
        /// </summary>
        T Value { get; }

        /// <summary>
        /// An unique message ID
        /// </summary>
        String Id { get; }

        /// <summary>
        /// Number of times this object has been dequeued
        /// </summary>
        int DequeueCount { get; }
    }
}