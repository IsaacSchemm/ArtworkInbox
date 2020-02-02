using ArtworkInbox.Backend.Types;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class EmptyFeedSource : FeedSource {
        public override Task<Author> GetAuthenticatedUserAsync() {
            return Task.FromResult(new Author());
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
