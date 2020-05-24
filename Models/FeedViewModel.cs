using ArtworkInbox.Backend;
using ArtworkInbox.Backend.Filters;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Backend.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Models {
    public class FeedViewModel {
        public string Host { get; set; }
        public DateTimeOffset Latest { get; set; }
        public FeedBatch FeedBatch { get; set; }
        public Author AuthenticatedUser { get; set; }
        public int? NotificationsCount { get; set; }
        public string NotificationsUrl { get; set; }
        public string SubmitUrl { get; set; }

        public static async Task<FeedViewModel> BuildAsync(string host, IFeedSource feedSource, IEnumerable<IFeedFilter> filters, string cursor = null, DateTimeOffset? earliest = null, DateTimeOffset? latest = null) {
            var batch = await feedSource.GetBatchesAsync(new FeedParameters {
                Filters = filters,
                Cursor = cursor,
                StartAt = earliest ?? DateTimeOffset.MinValue
            });
            return new FeedViewModel {
                Host = host,
                Latest = latest ?? batch.FeedItems.Select(x => x.Timestamp).DefaultIfEmpty(DateTimeOffset.MinValue).First(),
                FeedBatch = batch,
                AuthenticatedUser = await feedSource.GetAuthenticatedUserAsync(),
                NotificationsCount = feedSource is INotificationsSource ns
                    ? await ns.GetNotificationsCountAsync()
                    : (int?)null,
                NotificationsUrl = feedSource.GetNotificationsUrl(),
                SubmitUrl = feedSource.GetSubmitUrl()
            };
        }

        public IEnumerable<IGrouping<string, Artwork>> ArtworkByUser =>
            FeedBatch.Artworks
            .GroupBy(d => d.Author.Username)
            .OrderByDescending(g => g.Select(x => x.Timestamp).Max());

        public IEnumerable<IGrouping<string, JournalEntry>> JournalsByUser =>
            FeedBatch.JournalEntries
            .GroupBy(s => s.Author.Username)
            .OrderByDescending(g => g.Select(x => x.Timestamp).Max());

        public IEnumerable<IGrouping<string, BlogPost>> BlogPostsByUser =>
            FeedBatch.BlogPosts
            .GroupBy(s => s.Author.Username)
            .OrderByDescending(g => g.Select(x => x.Timestamp).Max());

        public IEnumerable<IGrouping<string, StatusUpdate>> StatusesByUser =>
            FeedBatch.StatusUpdates
            .GroupBy(s => s.Author.Username)
            .OrderByDescending(g => g.Select(x => x.Timestamp).Max());

        public IEnumerable<FeedItem> Misc =>
            FeedBatch.FeedItems
            .Except(FeedBatch.Artworks)
            .Except(FeedBatch.JournalEntries)
            .Except(FeedBatch.BlogPosts)
            .Except(FeedBatch.StatusUpdates);
    }
}
