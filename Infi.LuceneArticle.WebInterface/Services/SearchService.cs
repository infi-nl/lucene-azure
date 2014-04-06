using System.Collections.Generic;
using System.Linq;
using Infi.LuceneArticle.LuceneSupport.Helpers;
using Infi.LuceneArticle.LuceneSupport.Managers;
using Infi.LuceneArticle.Models;
using Lucene.Net.Analysis.Standard;
using Lucene.Net.QueryParsers;
using Lucene.Net.Util;

namespace Infi.LuceneArticle.WebInterface.Services
{
    public class SearchService
    {
        private readonly IndexSearcherManager _indexSearchManager;

        public SearchService(IndexSearcherManager indexSearchManager) {
            _indexSearchManager = indexSearchManager;
        }

        public IEnumerable<Book> Search(string searchTerm) {
            
            var searchHelper = new LuceneSearchHelper(_indexSearchManager);
            //Todo: nette plaats geven en minder hard deze velden doorgeven
            var queryParser = new MultiFieldQueryParser(Version.LUCENE_30, new[] {"Contents", "Title"}, new StandardAnalyzer(Version.LUCENE_30));
            var query = queryParser.Parse(searchTerm);

            return searchHelper.Query(query, 16).Select(c => new Book {Title = c.Get("Title"), GutenBergId = c.Get("GutenBergId")});
        }
    }
}
