using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Backend.Types;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Filters {
    public abstract class FeedFilter : FeedSource {
        private readonly FeedSource _source;

        protected FeedFilter(FeedSource source) {
            _source = source;
        }

        protected abstract IEnumerable<FeedItem> Apply(IEnumerable<FeedItem> feedItems);

        public override async Task<FeedBatch> GetBatchAsync(string cursor) {
            var batch = await _source.GetBatchAsync(cursor);
            return new FeedBatch {
                Cursor = batch.Cursor,
                HasMore = batch.HasMore,
                FeedItems = Apply(batch.FeedItems)
            };
        }

        public override Task<Author> GetAuthenticatedUserAsync() =>
            _source.GetAuthenticatedUserAsync();

        public override string GetNotificationsUrl() =>
            _source.GetNotificationsUrl();

        public override string GetSubmitUrl() =>
            _source.GetSubmitUrl();
    }
}
