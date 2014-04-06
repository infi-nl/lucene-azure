using System;
using System.Linq;
using System.Web.Mvc;
using Infi.LuceneArticle.WebInterface.Models;
using Infi.LuceneArticle.WebInterface.Services;

namespace Infi.LuceneArticle.WebInterface.Controllers
{
    public class SearchController : Controller
    {
        private readonly SearchService _searchService;

        public SearchController() {
            _searchService = WebApiApplication.SearchService;
        }

        public ActionResult Index()
        {
            return View();
        }

        public ActionResult Search(string query)
        {
            var model = new QueryResultModel();
            try {
                model.Results = _searchService.Search(query).Select(c=>new ResultEntryModel{Title = c.Title}).ToList();
            } catch (Exception e) {
                model.Error = String.Format("Error in query: {0}", e.ToString());
            }
            return View(model);
        }
    }
}
