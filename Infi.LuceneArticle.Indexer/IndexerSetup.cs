using Infi.LuceneArticle.Helpers.Queue;
using Infi.LuceneArticle.Indexer.Properties;
using Infi.LuceneArticle.LuceneSupport.Helpers;
using Infi.LuceneArticle.LuceneSupport.Indexer;
using Infi.LuceneArticle.LuceneSupport.Managers;
using Infi.LuceneArticle.LuceneSupport.Updates.QueueMessages;

namespace Infi.LuceneArticle.Indexer {
    public static class IndexerJobSetup {
        public static IndexerJob Create() {
            var indexWriterManager = CreateIndexWriteManager();
            var updateQueue = LuceneUpdateQueue.Factory.Create();

            return IndexerFactory.Create(indexWriterManager, updateQueue, Settings.Default.LuceneUpdateQueueBatchLimit);
        }

        private static IndexWriterManager CreateIndexWriteManager() {
            var azureDirectory = MyAzureDirectory.Factory.Create();
            
            return new IndexWriterManager(azureDirectory);
        }
    }
}
