using ArtworkInbox.Backend.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public interface ISource {
        string Name { get; }

        Task<Author> GetAuthenticatedUserAsync();
        IAsyncEnumerable<FeedItem> GetFeedItemsAsync();
        Task<int?> GetNotificationCountAsync();
        string GetNotificationsUrl();
        string GetSubmitUrl();
    }
}
