using DANotify.Backend.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DANotify.Backend {
    public abstract class FeedSource<T> {
        public abstract Task<FeedResult<T>> GetBatchAsync(T cursor);

        public async Task<FeedResult<T>> GetBatchesAsync(FeedParameters<T> parameters) {
            DateTime start = DateTime.Now;

            T cursor = parameters.Cursor;
            bool hasMore = true;
            var items = new List<FeedItem>();

            while (hasMore) {
                if (items.Count >= parameters.StopAtCount) break;
                if (DateTime.Now - start >= parameters.StopAtTime) break;

                var result = await GetBatchAsync(cursor);
                cursor = result.Cursor;
                hasMore = result.HasMore;
                foreach (var newItem in result.FeedItems) {
                    if (newItem.Timestamp < parameters.StartAt) {
                        hasMore = false;
                        break;
                    } else {
                        items.Add(newItem);
                    }
                }
            }

            return new FeedResult<T> {
                Cursor = cursor,
                HasMore = hasMore,
                FeedItems = items
            };
        }
    }
}
