using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Infi.LuceneArticle.Helpers {
    // http://stackoverflow.com/questions/1563191/c-sharp-cleanest-way-to-write-retry-logic
    public static class Retry {
        public static void Do(Action action, TimeSpan retryInterval, int retryCount = 3) {
            Do<object>(() => {
                action();
                return null;
            }, retryInterval, retryCount);
        }

        public static T Do<T>(Func<T> action, TimeSpan retryInterval, int retryCount = 3) {
            var exceptions = new List<Exception>();

            for (int retry = 0; retry < retryCount; retry++) {
                try {
                    return action();
                } catch (Exception ex) {
                    exceptions.Add(ex);
                    Thread.Sleep(retryInterval);
                }
            }

            throw new AggregateException(exceptions);
        }
    }
}
