using Infi.LuceneArticle.GutenbergImport.Console.Properties;
using Infi.LuceneArticle.Lucene;
using Infi.LuceneArticle.LuceneSupport.Updates;

namespace Infi.LuceneArticle.GutenbergImport.Console {
    public static class GutenbergImporterFactory {
        public static GutenBergImporter Create() {
            var directoryReader = new GutenBergDirectoryReader(Settings.Default.GutenbergLibraryDirectory);
            var azureUpdateQueue = LuceneUpdateQueue.Factory.Create();
            var updater = new Updater<BookLuceneAdapter>(azureUpdateQueue);

            return new GutenBergImporter(directoryReader, updater);
        }
    }
}
