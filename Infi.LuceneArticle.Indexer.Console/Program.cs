namespace Infi.LuceneArticle.Indexer.Console {
    class Program {
        static void Main(string[] args) {
            var indexerJob = IndexerJobSetup.Create();
            indexerJob.Run();
        }
    }
}
