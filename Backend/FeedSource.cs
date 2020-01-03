using ArtworkInbox.Backend.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend {
    public abstract class FeedSource {
        public abstract Task<Author> GetAuthenticatedUserAsync();
        public abstract Task<FeedBatch> GetBatchAsync(string cursor);

        public abstract string GetNotificationsUrl();
        public abstract string GetSubmitUrl();

        public async Task<FeedBatch> GetBatchesAsync(FeedParameters parameters) {
            DateTime start = DateTime.Now;

            string cursor = parameters.Cursor;
            bool hasMore = true;
            var items = new List<FeedItem>();

            while (hasMore) {
                if (items.Count >= parameters.StopAtCount) break;
                if (DateTime.Now - start >= parameters.StopAtTime) break;

                try {
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

    public class EmptyFeedSource : FeedSource {
        public override Task<Author> GetAuthenticatedUserAsync() {
            return Task.FromResult(new Author {
                Username = "Not logged in",
                AvatarUrl = "https://upload.wikimedia.org/wikipedia/commons/c/ce/Transparent.gif",
                ProfileUrl = "/Identity/Account/Manage/ExternalLogins"
            });
        }

        public override Task<FeedBatch> GetBatchAsync(string cursor) {
            return Task.FromResult(new FeedBatch {
                Cursor = null,
                HasMore = false,
                FeedItems = Enumerable.Empty<FeedItem>()
            });
        }

        public override string GetNotificationsUrl() => "https://www.example.com";
        public override string GetSubmitUrl() => "https://www.example.org";
    }
}
