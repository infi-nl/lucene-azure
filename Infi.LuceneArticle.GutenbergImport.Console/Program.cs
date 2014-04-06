using System.Threading;

namespace Infi.LuceneArticle.GutenbergImport.Console
{
    class Program
    {
        static  void Main(string[] args)
        {
            Run();
        }

        private static void Run() {
            var updater = GutenbergImporterFactory.Create();
            updater.ImportAll();

            System.Console.Out.WriteLine("Finished reading");

            while (updater.QueueSize > 0)
            {
               
                System.Console.Out.WriteLine("QueueSize:" + updater.QueueSize);
                Thread.Sleep(2000);
            }
            System.Console.ReadKey();
        }
    }
}
