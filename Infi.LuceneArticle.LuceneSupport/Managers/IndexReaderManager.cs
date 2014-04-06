using System;
using Lucene.Net.Index;
using Lucene.Net.Store;

namespace Infi.LuceneArticle.LuceneSupport.Managers {
    public class IndexReaderManager {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Directory _directory;
        private readonly object _indexReaderSyncRoot = new object();
        private IndexReader _indexReader;
        public event Action NewIndexReaderAvailable;

        public IndexReaderManager(Directory directory) {
            _directory = directory;
        }

        private IndexReader CreateIndexReader() {
            return IndexReader.Open(_directory, true);
        }

        public IndexReader IndexReader {
            get {
                if (_indexReader == null) {
                    lock (_indexReaderSyncRoot) {
                        if (_indexReader == null) {
                            Logger.Debug("Creating new IndexReader.");
                            _indexReader = CreateIndexReader();
                        }
                    }
                }
                return _indexReader;
            }
        }

        public void RecycleIndexReader() {
            // Prevent the application from reopening the reader multiple times at once
            lock (_indexReaderSyncRoot) {
                if (_indexReader == null) {
                    Logger.Debug("RecycleReader(): No IndexReader available, creating.");
                    _indexReader = CreateIndexReader();
                } else {
                    Logger.Debug("RecycleReader(): Reopening IndexReader.");
                    var newIndexReader = IndexReader.Reopen();

                    if (newIndexReader == _indexReader) {
                        Logger.Debug("RecycleReader(): IndexReader unchanged.");
                        // Index is unchanged on disk, Lucene reported this by returning 'this', so abort recycling
                        return;
                    }

                    _indexReader = newIndexReader;
                }

                Logger.Debug("RecycleReader(): done.");

                if (NewIndexReaderAvailable != null) {
                    NewIndexReaderAvailable();
                }
            }
        }
    }
}
