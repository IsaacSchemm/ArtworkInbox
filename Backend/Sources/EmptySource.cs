using ArtworkInbox.Backend.Types;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class EmptySource : IFeedSource, ISource {
        public Task<Author> GetAuthenticatedUserAsync() {
            return Task.FromResult(new Author());
        }

        public Task<FeedBatch> GetBatchAsync(string cursor) {
            return Task.FromResult(new FeedBatch {
                Cursor = null,
                HasMore = false,
                FeedItems = Enumerable.Empty<FeedItem>()
            });
        }

        public IAsyncEnumerable<FeedItem> GetFeedItemsAsync() => AsyncEnumerable.Empty<FeedItem>();

        public IAsyncEnumerable<string> GetNotificationsAsync() => AsyncEnumerable.Empty<string>();

        public string GetNotificationsUrl() => null;
        public string GetSubmitUrl() => null;
    }
}
