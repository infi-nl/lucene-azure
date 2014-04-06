using System.Collections.Generic;
using System.Linq;
using Infi.LuceneArticle.LuceneSupport.Managers;
using Lucene.Net.Documents;
using Lucene.Net.Search;

namespace Infi.LuceneArticle.LuceneSupport.Helpers
{
    public class LuceneSearchHelper {
        private readonly IndexSearcherManager _luceneSearchIndexManager;

        public LuceneSearchHelper(IndexSearcherManager luceneSearchIndexManager) {
            _luceneSearchIndexManager = luceneSearchIndexManager;
        }

        public IEnumerable<Document> Query(Query query, int numberOfResults) {
            // claim access to SearchIndex
            using (var indexSearcherReference = _luceneSearchIndexManager.GetIndexSearcherReference()) {
                // and obtain the actual reference
                var indexSearcher = indexSearcherReference.Value;   

                // perform search
                var topDocs = indexSearcher.Search(query, numberOfResults);

                // and resolve the topDoc to the referenced Lucene Documents.
                return topDocs.ScoreDocs.Select(scoreDoc => indexSearcher.Doc(scoreDoc.Doc)).ToList();
            }
        }
    }
}
