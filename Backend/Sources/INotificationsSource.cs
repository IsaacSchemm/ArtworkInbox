using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public interface INotificationsSource : IFeedSource {
        Task<int> GetNotificationsCountAsync();
    }
}
