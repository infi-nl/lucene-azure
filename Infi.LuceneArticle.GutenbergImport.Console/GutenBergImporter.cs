using System;
using Infi.LuceneArticle.Helpers.Queue;
using Infi.LuceneArticle.Lucene;
using Infi.LuceneArticle.LuceneSupport.Updates;
using Infi.LuceneArticle.LuceneSupport.Updates.QueueMessages;

namespace Infi.LuceneArticle.GutenbergImport.Console {
    public class GutenBergImporter {
        private readonly GutenBergDirectoryReader _directoryReader;
        private readonly Updater<BookLuceneAdapter> _updater;

        public GutenBergImporter(GutenBergDirectoryReader directoryReader, Updater<BookLuceneAdapter> updater) {
            _directoryReader = directoryReader;
            _updater = updater;
        }

        public void ImportAll() {
            foreach (var book in _directoryReader.ReadAll()) {
                _updater.QueueAdd(new BookLuceneAdapter(book));
            }
        }

        public int QueueSize {
            get { return _updater.ApproximateQueueSize; }
        }
    }
}
