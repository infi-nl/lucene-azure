using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infi.LuceneArticle.Helpers.Queue;
using Infi.LuceneArticle.LuceneSupport.Updates.QueueMessages;

namespace Infi.LuceneArticle.LuceneUpdateQueue
{
    public static class Factory
    {
        public static AzureObjectQueue<AbstractDocumentMessage> Create() {
            return
                new AzureObjectQueue<AbstractDocumentMessage>(
                    Properties.Settings.Default.AzureStorageQueueConnectionString,
                    Properties.Settings.Default.AzureStorageQueueName, Properties.Settings.Default.AzureStorageQueueName);
        }
    }
}
