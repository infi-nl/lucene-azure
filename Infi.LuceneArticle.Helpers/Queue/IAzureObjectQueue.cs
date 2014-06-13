namespace Infi.LuceneArticle.Helpers.Queue {
    public interface IAzureObjectQueue<T> {
        int ApproximateQueueSize { get; }
        void AddMessage(T message);
        IAzureObjectCloudQueueMessage<T> GetMessage();
        void CompleteMessage(IAzureObjectCloudQueueMessage<T> doneItem);
    }
}