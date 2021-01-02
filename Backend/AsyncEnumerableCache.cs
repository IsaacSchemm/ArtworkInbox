using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend {
    public class AsyncEnumerableCache<T> : IAsyncEnumerable<T> {
        private readonly List<T> _cache;
        private readonly IAsyncEnumerator<T> _enumerator;
        private readonly SemaphoreSlim _sem;

        public AsyncEnumerableCache(IAsyncEnumerable<T> source) {
            _cache = new List<T>();
            _enumerator = source.GetAsyncEnumerator();
            _sem = new SemaphoreSlim(1, 1);
        }

        public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) {
            return EnumerateAsync().GetAsyncEnumerator(cancellationToken);
        }

        private async IAsyncEnumerable<T> EnumerateAsync() {
            for (int i = 0; ; i++) {
                await _sem.WaitAsync();
                try {
                    if (_cache.Count <= i) {
                        if (!await _enumerator.MoveNextAsync()) {
                            yield break;
                        }
                        _cache.Add(_enumerator.Current);
                    }
                    yield return _cache[i];
                } finally {
                    _sem.Release();
                }
            }
        }
    }
}
