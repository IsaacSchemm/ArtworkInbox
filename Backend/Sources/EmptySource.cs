using ArtworkInbox.Backend.Types;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class EmptySource : ISource {
        public Task<Author> GetAuthenticatedUserAsync() {
            return Task.FromResult(new Author());
        }

        public string Name => "Empty";

        public IAsyncEnumerable<FeedItem> GetFeedItemsAsync() => AsyncEnumerable.Empty<FeedItem>();

        public Task<int?> GetNotificationCountAsync() => Task.FromResult<int?>(null);

        public string GetNotificationsUrl() => null;
        public string GetSubmitUrl() => null;
    }
}
