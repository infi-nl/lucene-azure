using System;
using System.Diagnostics;
using Infi.LuceneArticle.Helpers;
using Infi.LuceneArticle.LuceneSupport.Managers;

namespace Infi.LuceneArticle.LuceneSupport.Indexer {
    public class IndexerJob {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IndexWriterManager _indexWriterManager;

        private readonly BackgroundWorkQueueReader _workQueueReader;
        private readonly BackgroundWorkQueueCompleter _workQueueCompleter;

        public IndexerJob(IndexWriterManager indexWriterManager, BackgroundWorkQueueReader workQueueReader, BackgroundWorkQueueCompleter workQueueCompleter) {
            _indexWriterManager = indexWriterManager;
            _workQueueReader = workQueueReader;
            _workQueueCompleter = workQueueCompleter;
        }

        /// <summary>
        /// Blocking active loop for processing received .
        /// </summary>
        public void Run() {
            Logger.Info("Lucene Indexer background thread is running");

            while (true) {
                try {
                    var batch = _workQueueReader.ExtractBatchFromWorkQueue();

                    // We use a retry mechanism to recover from out-of-memory exceptions
                    Retry.Do(() => {
                        try {
                            foreach (var queueItem in batch) {
                                Logger.Trace("Indexing {0}", queueItem.Id);
                                queueItem.Value.Handle(_indexWriterManager.IndexWriter);
                            }

                            // Commit the newly indexed items to disk
                            _indexWriterManager.IndexWriter.Commit();
                        } catch (OutOfMemoryException) {
                            // when Lucene throws this exception, we have to recycle the writer
                            _indexWriterManager.RecycleWriter();
                            // and let Retry.Do do the rest.
                            throw;
                        }
                    }, TimeSpan.FromMilliseconds(500), 10);

                    batch.ForEach(_workQueueCompleter.Complete);

                    Logger.Info("Committing {0} items to Lucene.", batch.Count);
                } catch (Exception e) {
                    Logger.ErrorException("Unhandled exception in HandleWorkQueue(): {0}", e);
                }
            }
        }
    }
}
