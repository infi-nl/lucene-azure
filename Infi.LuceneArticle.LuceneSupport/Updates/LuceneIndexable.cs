using Lucene.Net.Documents;

namespace Infi.LuceneArticle.LuceneSupport.Updates
{
    public interface LuceneIndexable {
        string IdFieldName { get; }
        string IdFieldValue { get; }

        Document ToDocument();
    }
}
