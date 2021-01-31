using ArtworkInbox.Backend.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public interface ISource {
        string Name { get; }

        Task<Author> GetAuthenticatedUserAsync();
        IAsyncEnumerable<FeedItem> GetFeedItemsAsync();
        IAsyncEnumerable<string> GetNotificationsAsync();
        string GetNotificationsUrl();
        string GetSubmitUrl();
    }
}
