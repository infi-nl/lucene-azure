using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Infi.LuceneArticle.Helpers.Queue;
using Infi.LuceneArticle.LuceneSupport.Updates.QueueMessages;

namespace Infi.LuceneArticle.LuceneSupport.Indexer {
    public class BackgroundWorkQueueReader {
        private readonly IAzureObjectQueue<AbstractDocumentMessage> _azureLuceneUpdateQueue;
        private readonly int _queueBatchLimit;
        private readonly BlockingCollection<IAzureObjectCloudQueueMessage<AbstractDocumentMessage>> _todoItems;
        private readonly Lazy<Thread> _readerThread;

        public BackgroundWorkQueueReader(IAzureObjectQueue<AbstractDocumentMessage> azureLuceneUpdateQueue, int queueBatchLimit) {
            _azureLuceneUpdateQueue = azureLuceneUpdateQueue;
            _queueBatchLimit = queueBatchLimit;
            _todoItems = new BlockingCollection<IAzureObjectCloudQueueMessage<AbstractDocumentMessage>>();
            _readerThread = new Lazy<Thread>(() => new Thread(BackgroundFiller));
        }

        private void EnsureReaderThreadIsRunning() {
            if (!_readerThread.IsValueCreated) {
                _readerThread.Value.Start();
            }
        }

        private void BackgroundFiller() {
            while (true) {
                try {
                    var item = _azureLuceneUpdateQueue.GetMessage();

                    if (item != null) {
                        _todoItems.Add(item);
                    } else {
                        Thread.Sleep(TimeSpan.FromSeconds(1));
                    }
                } catch (Exception e) {
                    Debug.WriteLine("Unhandled exception in FillTodoQueue(): {0}", e);
                }
            }
        }

        /// <summary>
        /// Blocks until at least one item of work is available, and take up to _queueBatchLimit for subsequent processing
        /// </summary>
        public List<IAzureObjectCloudQueueMessage<AbstractDocumentMessage>> ExtractBatchFromWorkQueue() {
            EnsureReaderThreadIsRunning();

            var batch = new List<IAzureObjectCloudQueueMessage<AbstractDocumentMessage>>();

            // First, a blocking call
            batch.Add(_todoItems.Take());

            // And perhaps more items are waiting.
            var batchSize = 1;
            IAzureObjectCloudQueueMessage<AbstractDocumentMessage> queueItem;
            while (_todoItems.TryTake(out queueItem) && batchSize < _queueBatchLimit) {
                batch.Add(queueItem);
                batchSize++;
            }

            return batch;
        }
    }
}