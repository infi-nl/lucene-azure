using Infi.LuceneArticle.LuceneSupport.Indexer;
using Infi.LuceneArticle.LuceneSupport.Updates;
using Infi.LuceneArticle.Models;
using Lucene.Net.Documents;

namespace Infi.LuceneArticle.Lucene {
    public class BookLuceneAdapter : LuceneIndexable {
        private  Book _book { get; set; }
    
        public BookLuceneAdapter(Book book) {
            _book = book;
        }

        public string IdFieldName {
            get { return "GutenBergId"; }
        }

        public string IdFieldValue {
            get { return _book.GutenBergId; }
        }

        public Document ToDocument() {
            var document = new Document();
            document.Add(new Field("GutenBergId", _book.GutenBergId, Field.Store.YES, Field.Index.NO));
            document.Add(new Field("Title", _book.Title, Field.Store.YES, Field.Index.NO));
            document.Add(new Field("ReleaseDate", _book.ReleaseDate.HasValue ? _book.ReleaseDate.Value.ToShortDateString() : string.Empty, Field.Store.YES, Field.Index.NO));
            document.Add(new Field("Language", _book.Language, Field.Store.YES, Field.Index.NO));
            document.Add(new Field("Contents", _book.Contents, Field.Store.YES /* todo: willen we dit echt? */, Field.Index.ANALYZED));
            document.Add(new Field("Author", _book.Author, Field.Store.YES, Field.Index.ANALYZED));

            return document;
        }
    }
}
