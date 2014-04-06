using System;
using System.Diagnostics;
using Infi.LuceneArticle.LuceneSupport.Indexer;
using Lucene.Net.Index;

namespace Infi.LuceneArticle.LuceneSupport.Updates.QueueMessages {
    public class UpdateDocumentMessage<T> : AbstractDocumentMessage where T : LuceneIndexable {
        public T Document { get; private set; }

        public UpdateDocumentMessage(T document) : base(document) {
            Document = document;
        }

        public override void Handle(IndexWriter indexWriter) {
            Debug.WriteLine(String.Format("Updating {0}.", IdFieldValue));
            indexWriter.UpdateDocument(MyIdTerm, Document.ToDocument());
        }
    }
}