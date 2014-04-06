using System;
using System.Diagnostics;
using Infi.LuceneArticle.LuceneSupport.Indexer;
using Lucene.Net.Index;

namespace Infi.LuceneArticle.LuceneSupport.Updates.QueueMessages {
    public class DeleteMessage : AbstractDocumentMessage {
        public DeleteMessage(LuceneIndexable document) : base(document) {}

        public override void Handle(IndexWriter indexWriter) {
            Debug.WriteLine(String.Format("Deleting {0}", IdFieldValue));
            indexWriter.DeleteDocuments(MyIdTerm);
        }
    }
}