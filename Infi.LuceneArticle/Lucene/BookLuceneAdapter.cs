using Infi.LuceneArticle.LuceneSupport.Indexer;
using Infi.LuceneArticle.LuceneSupport.Updates;
using Infi.LuceneArticle.Models;
using Lucene.Net.Documents;

namespace Infi.LuceneArticle.Lucene {
    public class BookLuceneAdapter : LuceneIndexable {
        // Because this object will round-trip via json, this must be a property - not a field! 
        private Book Book { get; set; }
    
        public BookLuceneAdapter(Book book) {
            Book = book;
        }

        public string IdFieldName {
            get { return "GutenBergId"; }
        }

        public string IdFieldValue {
            get { return Book.GutenBergId; }
        }

        public Document ToDocument() {
            var document = new Document();
            document.Add(new Field("GutenBergId", Book.GutenBergId, Field.Store.YES, Field.Index.NO));
            document.Add(new Field("Title", Book.Title, Field.Store.YES, Field.Index.NO));
            document.Add(new Field("ReleaseDate", Book.ReleaseDate.HasValue ? Book.ReleaseDate.Value.ToShortDateString() : string.Empty, Field.Store.YES, Field.Index.NO));
            document.Add(new Field("Language", Book.Language, Field.Store.YES, Field.Index.NO));
            document.Add(new Field("Contents", Book.Contents, Field.Store.YES /* todo: willen we dit echt? */, Field.Index.ANALYZED));
            document.Add(new Field("Author", Book.Author, Field.Store.YES, Field.Index.ANALYZED));

            return document;
        }
    }
}
