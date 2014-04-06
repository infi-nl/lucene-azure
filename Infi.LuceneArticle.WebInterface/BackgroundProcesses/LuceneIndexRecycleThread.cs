using System;
using System.Threading;
using Infi.LuceneArticle.LuceneSupport.Helpers;
using NLog;

namespace Infi.LuceneArticle.WebInterface.BackgroundProcesses {
    public class LuceneIndexRecycleThread {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly PeriodicIndexRecycler _periodicIndexRecycler;
        private readonly TimeSpan _recycleInterval;
        private readonly Thread _myThread;

        public LuceneIndexRecycleThread(PeriodicIndexRecycler periodicIndexRecycler, TimeSpan recycleInterval) {
            _periodicIndexRecycler = periodicIndexRecycler;
            _recycleInterval = recycleInterval;
            _myThread = new Thread(ThreadRun) { IsBackground = true };
        }

        public void Start() {
            _myThread.Start();
        }

        public void Stop() {
            _myThread.Abort();
        }

        private void ThreadRun() {
            while (true) {
                try {
                    _periodicIndexRecycler.DoPeriodicRecycle(_recycleInterval);
                } catch (Exception e) {
                    Logger.LogException(LogLevel.Warn, "Error during PeriodicRecycle: ", e);
                    Thread.Sleep(TimeSpan.FromSeconds(5));
                }
            }
        }
    }
}