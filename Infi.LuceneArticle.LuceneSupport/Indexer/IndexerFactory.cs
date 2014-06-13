using Infi.LuceneArticle.Helpers.Queue;
using Infi.LuceneArticle.LuceneSupport.Managers;
using Infi.LuceneArticle.LuceneSupport.Updates.QueueMessages;

namespace Infi.LuceneArticle.LuceneSupport.Indexer {
    public static class IndexerFactory {
        public static IndexerJob Create(IndexWriterManager indexWriterManager, IAzureObjectQueue<AbstractDocumentMessage> azureLuceneUpdateQueue, int queueBatchLimit) {
            var workQueueReader = new BackgroundWorkQueueReader(azureLuceneUpdateQueue, queueBatchLimit);
            var workQueueCompleter = new BackgroundWorkQueueCompleter(azureLuceneUpdateQueue);

            return new IndexerJob(indexWriterManager, workQueueReader, workQueueCompleter);
        }
    }
}
