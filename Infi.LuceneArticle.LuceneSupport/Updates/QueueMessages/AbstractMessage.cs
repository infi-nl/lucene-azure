using Infi.LuceneArticle.LuceneSupport.Indexer;
using Lucene.Net.Index;
using Newtonsoft.Json;

namespace Infi.LuceneArticle.LuceneSupport.Updates.QueueMessages {
    public abstract class AbstractDocumentMessage {
        protected string IdFieldName { get; private set; }
        protected string IdFieldValue { get; private set; }

        protected AbstractDocumentMessage(LuceneIndexable document) {
            if (document != null) {
                IdFieldName = document.IdFieldName;
                IdFieldValue = document.IdFieldValue;
            }
        }

        [JsonIgnore]
        protected Term MyIdTerm {
            get { return new Term(IdFieldName, IdFieldValue); }
        }

        public abstract void Handle(IndexWriter indexWriter);
    }
}