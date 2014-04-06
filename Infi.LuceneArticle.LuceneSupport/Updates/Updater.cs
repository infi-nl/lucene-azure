using Infi.LuceneArticle.Helpers.Queue;
using Infi.LuceneArticle.LuceneSupport.Indexer;
using Infi.LuceneArticle.LuceneSupport.Updates.QueueMessages;

namespace Infi.LuceneArticle.LuceneSupport.Updates {
    public class Updater<T> where T : LuceneIndexable {
        private readonly AzureObjectQueue<AbstractDocumentMessage> _azureLuceneUpdateQueue;

        public Updater(AzureObjectQueue<AbstractDocumentMessage> azureLuceneUpdateQueue) {
            _azureLuceneUpdateQueue = azureLuceneUpdateQueue;
        }

        public int ApproximateQueueSize {
            get { return _azureLuceneUpdateQueue.ApproximateQueueSize; }
        }

        public void QueueAdd(T document) {
            _azureLuceneUpdateQueue.AddMessage(new AddDocumentMessage<T>(document));
        }

        public void QueueUpdate(T document) {
            _azureLuceneUpdateQueue.AddMessage(new UpdateDocumentMessage<T>(document));
        }

        public void QueueDelete(T document) {
            _azureLuceneUpdateQueue.AddMessage(new DeleteMessage(document));
        }
    }
}
