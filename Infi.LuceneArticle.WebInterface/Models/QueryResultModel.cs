using System.Collections.Generic;

namespace Infi.LuceneArticle.WebInterface.Models
{
    public class QueryResultModel {
        public string Error { get; set; }
        public List<ResultEntryModel> Results { get; set; }
    }
}