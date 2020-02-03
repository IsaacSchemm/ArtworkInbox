using ArtworkInbox.Backend.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public static class FeedSourceExtensions {
        public static async Task<FeedBatch> GetBatchesAsync(this IFeedSource source, FeedParameters parameters) {
            DateTime start = DateTime.Now;

            string cursor = parameters.Cursor;
            bool hasMore = true;
            var items = new List<FeedItem>();

            while (hasMore) {
                if (items.Count >= parameters.StopAtCount) break;
                if (DateTime.Now - start >= parameters.StopAtTime) break;

                try {
                    var result = await source.GetBatchAsync(cursor);
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
                } catch (TooManyRequestsException) when (items.Count > 0) {
                    break;
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
