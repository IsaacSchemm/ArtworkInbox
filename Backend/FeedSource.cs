using DANotify.Backend.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DANotify.Backend {
    public abstract class FeedSource {
        public abstract Task<Author> GetAuthenticatedUserAsync();
        public abstract Task<FeedBatch> GetBatchAsync(string cursor);
        public abstract Task<bool> HasNotificationsAsync();
        public abstract string GetNotificationsUrl();

        public async Task<FeedBatch> GetBatchesAsync(FeedParameters parameters) {
            DateTime start = DateTime.Now;

            string cursor = parameters.Cursor;
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

            return new FeedBatch {
                Cursor = cursor,
                HasMore = hasMore,
                FeedItems = items
            };
        }
    }
}
