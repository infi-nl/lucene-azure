using System;
using System.Diagnostics;
using Infi.LuceneArticle.LuceneSupport.Indexer;
using Lucene.Net.Index;

namespace Infi.LuceneArticle.LuceneSupport.Updates.QueueMessages {
    public class AddDocumentMessage<T> : AbstractDocumentMessage where T : LuceneIndexable {
        public T Document { get; private set; }

        public AddDocumentMessage(T document) : base(document) {
            Document = document;
        }

        public override void Handle(IndexWriter indexWriter) {
            Debug.WriteLine(String.Format("Adding {0}.", IdFieldValue));
            indexWriter.AddDocument(Document.ToDocument());
        }
    }
}