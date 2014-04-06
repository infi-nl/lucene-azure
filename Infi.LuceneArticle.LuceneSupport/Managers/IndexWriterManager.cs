using Lucene.Net.Analysis.Standard;
using Lucene.Net.Index;
using Lucene.Net.Store;
using Lucene.Net.Util;

namespace Infi.LuceneArticle.LuceneSupport.Managers {
    public class IndexWriterManager {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly Directory _directory;
        private readonly object _indexWriterSyncRoot;
        private IndexWriter _indexWriter;

        public IndexWriterManager(Directory directory) {
            _indexWriterSyncRoot = new object();
            _directory = directory;
        }

        private IndexWriter CreateIndexWriter() {
            return new IndexWriter(_directory, 
                new StandardAnalyzer(Version.LUCENE_30), IndexWriter.MaxFieldLength.UNLIMITED);
        }

        public IndexWriter IndexWriter {
            get {
                if (_indexWriter == null) {
                    lock (_indexWriterSyncRoot) {
                        if (_indexWriter == null) {
                            Logger.Debug("Creating new IndexReader.");
                            _indexWriter = CreateIndexWriter();
                        }
                    }
                } 
                return _indexWriter;
            }
        }

        public void RecycleWriter() {
            lock (_indexWriterSyncRoot) {
                Logger.Debug("Recycling writer.");

                _indexWriter = CreateIndexWriter();
            }
        }
    }
}
