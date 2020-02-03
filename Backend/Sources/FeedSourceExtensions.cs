using ArtworkInbox.Backend.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public static class FeedSourceExtensions {
        public static async Task<FeedBatch> GetBatchesAsync(this IFeedSource source, FeedParameters parameters) {
            DateTime start = DateTime.Now;

            var filters = parameters.Filters.ToList();
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

                    var feedItems = result.FeedItems;
                    foreach (var filter in filters)
                        feedItems = filter.Apply(feedItems);

                    foreach (var newItem in feedItems) {
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
