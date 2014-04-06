using System.Threading;
using Infi.LuceneArticle.Helpers;
using Lucene.Net.Index;
using Lucene.Net.Search;

namespace Infi.LuceneArticle.LuceneSupport.Managers {
    public class IndexSearcherManager {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IndexReaderManager _indexReaderManager;
        private ReferenceCounter<IndexSearcher> _indexSearcherReferenceCounter;

        /// <summary>
        /// This is lock governing the Lucene IndexSearcher. It's a ReaderWriterLock, e.g.
        /// multiple threads can simultaniously obtain a read lock (and thus aqcuire the
        /// IndexSearcher), while only one thread can obtain a write lock (and thus swap
        /// the Lucene Writer).
        /// </summary>
        private readonly ReaderWriterLock _indexSearcherSyncLock = new ReaderWriterLock();

        /// <summary>
        /// This object always holds a Reference to the Lucene Index in order to prevent
        /// the ReferenceCounter from performing the clean up action, which would happen when
        /// the IndexSearcher is idle after usage.
        /// </summary>
        private ReferenceCounter<IndexSearcher>.Reference _indexSearcherRootReference;

        public IndexSearcherManager(IndexReaderManager indexReaderManager) {
            _indexReaderManager = indexReaderManager;

            // And register our interest for IndexReader updates
            indexReaderManager.NewIndexReaderAvailable += SwapInNewIndexSearcher;

            // And create a IndexSearcher based on the managed IndexReader
            SwapInNewIndexSearcher();
        }

        private void SwapInNewIndexSearcher() {
            // Prepare a new IndexSearcher
            Logger.Info("SwapInNewIndexSearcher(): Creating new IndexSearcher.");
            var newIndexSearcherReferenceCounter = CreateIndexSearcherReferenceCounter(_indexReaderManager.IndexReader);
            ReferenceCounter<IndexSearcher>.Reference oldSearcherRootReference;

            Logger.Info("SwapInNewIndexSearcher(): Waiting for writerlock.");
            try {
                // Before swapping our newIndexSearcher in, we need to be sure no new references
                // to our _indexSearcher are being handed out. Would we neglect this, there
                // is a chance the ReferenceCounter is Disposing the IndexSearcher before
                // obtaining a new Reference to it.
                // While we hold the writer lock, no readers are active. New requests for read
                // access are queued.
                _indexSearcherSyncLock.AcquireWriterLock(Timeout.Infinite);

                Logger.Info("SwapInNewIndexSearcher(): Swapping in new IndexSearcher.");

                // Save the reference to the old IndexSearcher for later disposal.
                oldSearcherRootReference = _indexSearcherRootReference;

                // Now we swap the IndexSearcher
                _indexSearcherReferenceCounter = newIndexSearcherReferenceCounter;

                // And obtain a reference in order to prevent premature Dispose()
                _indexSearcherRootReference = newIndexSearcherReferenceCounter.GetReference();
            } finally {
                _indexSearcherSyncLock.ReleaseWriterLock();
            }

            Logger.Debug("New IndexSearcher is active.");

            // If we currently own a index searcher, one of two things might occur:
            // 1) the IndexSearcher is not referenced by other object, on Release() the 
            //    clean up action (Dispose) is performed right now
            // 2) the IndexSearcher is referenced by at least one more object, this Release()
            //    merely signals the ReferenceCounter that we're done with it, and the 
            //    IndexSearcher is Dispose()d only after the last object Release()s it 
            //    (e.g. after the last search query is completed).
            if (oldSearcherRootReference != null) {
                Logger.Debug("New IndexSearcher is active.");
                oldSearcherRootReference.Release();
            }
        }

        private static ReferenceCounter<IndexSearcher> CreateIndexSearcherReferenceCounter(IndexReader newIndexReader) {
            Logger.Debug("Creating new IndexSearcher.");
            return new ReferenceCounter<IndexSearcher>(new IndexSearcher(newIndexReader), DisposeSearcherAndReader);
        }

        private static void DisposeSearcherAndReader(IndexSearcher searcher) {
            Logger.Debug("Disposing old IndexSearcher and IndexReader.");
            searcher.Dispose();
            // IndexSearcher.Dispose does not Dispose the reader, if initialized with an IndexReader.
            searcher.IndexReader.Dispose();
        }

        public ReferenceCounter<IndexSearcher>.Reference GetIndexSearcherReference() {
            try {
                // Lock in order to prevent returning a soon-to-be-disposed IndexSearcher reference.
                _indexSearcherSyncLock.AcquireReaderLock(Timeout.Infinite);
                return _indexSearcherReferenceCounter.GetReference();
            } finally {
                _indexSearcherSyncLock.ReleaseReaderLock();
            }
        }
    }
}