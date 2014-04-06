using Infi.LuceneArticle.MyAzureDirectory.Properties;
using Lucene.Net.Store;
using Lucene.Net.Store.Azure;
using Microsoft.WindowsAzure.Storage;

namespace Infi.LuceneArticle.MyAzureDirectory
{
    public static class Factory
    {
        public static Directory Create() {
            return GetAzureDirectory(
                Settings.Default.AzureDirectoryConnectionString,
                Settings.Default.AzureDirectoryCatalog);
        }

        private static Directory GetAzureDirectory(string storageConnectionString, string catalog)
        {
            var storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // The default constructor of AzureDirectory will use a FSDirectory in:
            // Path.Combine(Environment.ExpandEnvironmentVariables("%temp%"), "AzureDirectory");
            return new AzureDirectory(storageAccount, catalog);
        }
    }
}
