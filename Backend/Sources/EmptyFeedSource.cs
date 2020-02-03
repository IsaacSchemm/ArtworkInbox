using ArtworkInbox.Backend.Types;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class EmptyFeedSource : IFeedSource {
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

        public string GetNotificationsUrl() => "https://www.example.com";
        public string GetSubmitUrl() => "https://www.example.org";
    }
}
