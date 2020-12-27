using DeviantArtFs;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class DeviantArtFeedSource : CompositeFeedSource, INotificationsSource {
        private readonly IDeviantArtAccessToken _token;

        public DeviantArtFeedSource(IDeviantArtAccessToken token) : base(new IFeedSource[] {
            new DeviantArtDeviationFeedSource(token),
            new DeviantArtPostFeedSource(token)
        }) {
            _token = token;
        }

        public async Task<int> GetNotificationsCountAsync() {
            var messages = await DeviantArtFs.Api.Messages.MessagesFeed.ToArrayAsync(
                _token,
                new DeviantArtFs.Api.Messages.MessagesFeedRequest { Stack = false },
                null,
                99);
            return messages.Length;
        }
    }
}
