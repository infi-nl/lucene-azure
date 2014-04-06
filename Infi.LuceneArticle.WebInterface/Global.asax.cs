using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;
using Infi.LuceneArticle.LuceneSupport.Helpers;
using Infi.LuceneArticle.LuceneSupport.Managers;
using Infi.LuceneArticle.WebInterface.BackgroundProcesses;
using Infi.LuceneArticle.WebInterface.Services;

namespace Infi.LuceneArticle.WebInterface
{
    // Note: For instructions on enabling IIS6 or IIS7 classic mode, 
    // visit http://go.microsoft.com/?LinkId=9394801

    public class WebApiApplication : System.Web.HttpApplication
    {
        private LuceneIndexRecycleThread _luceneRecycleThread;
        public static SearchService SearchService;
        public static IndexReaderManager IndexReaderManager;

        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();

            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleConfig.RegisterBundles(BundleTable.Bundles);

            _luceneRecycleThread = CreateLuceneIndexRecycleThread();
            _luceneRecycleThread.Start();
        }

        private static LuceneIndexRecycleThread CreateLuceneIndexRecycleThread() {
            return new LuceneIndexRecycleThread(new PeriodicIndexRecycler(IndexReaderManager), Properties.Settings.Default.LuceneIndexRecycleInterval);
        }

        protected void Application_End() {
            _luceneRecycleThread.Stop();
        }

        static WebApiApplication() {
            CreateIndexReaderManager();
            CreateSearchService();
        }

        private static void CreateIndexReaderManager() {
            IndexReaderManager = new IndexReaderManager(MyAzureDirectory.Factory.Create());
        }

        private static void CreateSearchService() {
            var indexSearchManager = new IndexSearcherManager(IndexReaderManager);
            SearchService = new SearchService(indexSearchManager);
        }
    }
}