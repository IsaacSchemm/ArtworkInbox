using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Backend.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Filters {
    public abstract class FeedFilter : IFeedSource {
        private readonly IFeedSource _source;

        protected FeedFilter(IFeedSource source) {
            _source = source;
        }

        protected abstract IEnumerable<FeedItem> Apply(IEnumerable<FeedItem> feedItems);

        public async Task<FeedBatch> GetBatchAsync(string cursor) {
            var batch = await _source.GetBatchAsync(cursor);
            return new FeedBatch {
                Cursor = batch.Cursor,
                HasMore = batch.HasMore,
                FeedItems = Apply(batch.FeedItems)
            };
        }

        public Task<Author> GetAuthenticatedUserAsync() =>
            _source.GetAuthenticatedUserAsync();

        public string GetNotificationsUrl() =>
            _source.GetNotificationsUrl();

        public string GetSubmitUrl() =>
            _source.GetSubmitUrl();
    }
}
