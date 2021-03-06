﻿using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using Infi.LuceneArticle.Helpers.Queue;
using Infi.LuceneArticle.LuceneSupport.Updates.QueueMessages;

namespace Infi.LuceneArticle.LuceneSupport.Indexer {
    public class BackgroundWorkQueueCompleter {
        private readonly IAzureObjectQueue<AbstractDocumentMessage> _azureLuceneUpdateQueue;
        private readonly BlockingCollection<IAzureObjectCloudQueueMessage<AbstractDocumentMessage>> _doneItems;
        private readonly Lazy<Thread> _completerThread;

        public BackgroundWorkQueueCompleter(IAzureObjectQueue<AbstractDocumentMessage> azureLuceneUpdateQueue) {
            _azureLuceneUpdateQueue = azureLuceneUpdateQueue;
            _doneItems = new BlockingCollection<IAzureObjectCloudQueueMessage<AbstractDocumentMessage>>();
            _completerThread = new Lazy<Thread>(() => new Thread(BackgroundCompleter));
        }

        private void EnsureCompleterThreadIsRunning() {
            if (!_completerThread.IsValueCreated) {
                _completerThread.Value.Start();
            }
        }

        private void BackgroundCompleter() {
            while (true) {
                try {
                    foreach (var doneItem in _doneItems.GetConsumingEnumerable()) {
                        Console.WriteLine(string.Format("Done queue, item {0} is done.", doneItem.Id));
                        _azureLuceneUpdateQueue.CompleteMessage(doneItem);
                    }
                } catch (Exception e) {
                    Debug.WriteLine("Unhandled exception in HandleDoneQueue(): {0}", e);
                    Thread.Sleep(TimeSpan.FromSeconds(0.1));
                }
            }
        }

        public void Complete(IAzureObjectCloudQueueMessage<AbstractDocumentMessage> obj) {
            EnsureCompleterThreadIsRunning();

            _azureLuceneUpdateQueue.CompleteMessage(obj);
        }
    }
}