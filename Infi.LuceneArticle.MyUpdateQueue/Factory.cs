using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Infi.LuceneArticle.Helpers.Queue;
using Infi.LuceneArticle.Helpers.Queue.CloudQueue;
using Infi.LuceneArticle.Helpers.Queue.ServicebusQueue;
using Infi.LuceneArticle.LuceneSupport.Updates.QueueMessages;

namespace Infi.LuceneArticle.LuceneUpdateQueue
{
    public static class Factory
    {
        public static IAzureObjectQueue<AbstractDocumentMessage> Create() {
            // Use the code below to use Azure Cloud Queues (aka Storage Queues)
            //return
            //    new AzureObjectCloudQueue<AbstractDocumentMessage>(
            //        Properties.Settings.Default.AzureStorageQueueConnectionString,
            //        Properties.Settings.Default.AzureStorageQueueName, Properties.Settings.Default.AzureStorageQueueBlobContainer);

            return
                new AzureObjectServiceBusQueue<AbstractDocumentMessage>(
                    Properties.Settings.Default.AzureObjectStoreBlobConnectionString,
                    Properties.Settings.Default.AzureObjectStoreBlobContainer,
                    Properties.Settings.Default.AzureServiceBusQueueConnectionString,
                    Properties.Settings.Default.AzureServiceBusQueueName);
        }
    }
}
