using System;

namespace Infi.LuceneArticle.Helpers {
    /// <summary>
    /// Manages reference-counter based access to an object. After the last Release(), a custom cleanup action is executed.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ReferenceCounter<T> {
        private bool _lastReleaseDone;
        private readonly T _value;
        private readonly Action<T> _cleanUpAction;
        private int _counter;
        private readonly object _syncRoot;

        public ReferenceCounter(T value, Action<T> cleanUpAction) {
            _counter = 0;
            _syncRoot = new object();
            _value = value;
            _cleanUpAction = cleanUpAction;
            _lastReleaseDone = false;
        }

        /// <summary>
        /// Gets a reference, and increases the reference counter.
        /// </summary>
        /// <returns></returns>
        public Reference GetReference() {
            lock (_syncRoot) {
                if (_lastReleaseDone) {
                    throw new Exception("Cannot get new reference after last release has been signaled.");
                }
                _counter++;
                return new Reference(_value, this);
            }
        }

        /// <summary>
        /// Holds a reference to an object. 
        /// </summary>
        public class Reference : IDisposable {
            private readonly T _value;
            private readonly ReferenceCounter<T> _referenceCounter;
            private bool _isReleased;

            public Reference(T value, ReferenceCounter<T> referenceCounter) {
                _value = value;
                _referenceCounter = referenceCounter;
                _isReleased = false;
            }

            /// <summary>
            /// The referenced object
            /// </summary>
            public T Value {
                get {
                    return _value;
                }
            }

            /// <summary>
            /// Releases the object. After calling this method, the object referenced by .Value
            /// should not be used anymore.
            /// </summary>
            public void Release() {
                if (!_isReleased) {
                    _isReleased = true;
                    _referenceCounter.ReleaseOne();
                }
            }

            /// <summary>
            /// Calls Release()
            /// </summary>
            public void Dispose() {
                Release();
            }
        }

        private void ReleaseOne() {
            lock (_syncRoot) {
                _counter--;

                if (_counter == 0) {
                    _lastReleaseDone = true;
                    _cleanUpAction(_value);
                }
            }
        }
    }
}
