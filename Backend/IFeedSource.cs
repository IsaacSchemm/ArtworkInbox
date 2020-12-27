using ArtworkInbox.Backend.Types;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend {
    public interface IFeedSource {
        Task<Author> GetAuthenticatedUserAsync();
        Task<FeedBatch> GetBatchAsync(string cursor);

        string GetNotificationsUrl();
        string GetSubmitUrl();
    }
}
