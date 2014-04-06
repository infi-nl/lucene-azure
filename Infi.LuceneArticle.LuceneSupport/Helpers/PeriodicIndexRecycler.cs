using System;
using System.Diagnostics;
using System.Threading;
using Infi.LuceneArticle.LuceneSupport.Managers;

namespace Infi.LuceneArticle.LuceneSupport.Helpers {
    public class PeriodicIndexRecycler {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly IndexReaderManager _indexReaderManagerToRecycle;

        public PeriodicIndexRecycler(IndexReaderManager indexReaderManagerToRecycle) {
            _indexReaderManagerToRecycle = indexReaderManagerToRecycle;
        }

        public void DoPeriodicRecycle(TimeSpan recycleInterval) {
            Logger.Trace("Starting periodic recycling, interval {0}.", recycleInterval);
            var myStopwatch = new Stopwatch();
     
            while (true) {
                myStopwatch.Restart();
                _indexReaderManagerToRecycle.RecycleIndexReader();
                myStopwatch.Stop();

                if (myStopwatch.Elapsed >= recycleInterval) {
                    Logger.Warn("Lucene Index recycling takes longer ({0}) than the requested recycle interval ({1})",
                        myStopwatch.Elapsed, recycleInterval);
                } else {
                    Thread.Sleep(recycleInterval - myStopwatch.Elapsed);
                }
            }
        }
    }
}
