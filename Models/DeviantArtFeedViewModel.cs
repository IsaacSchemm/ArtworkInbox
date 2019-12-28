using DANotify.Backend;
using DANotify.Backend.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace DANotify.Models {
    public class DeviantArtFeedViewModel {
        public DateTimeOffset Start { get; set; }
        public FeedResult<string> FeedResult { get; set; }
        public bool AnyNotifications { get; set; }

        public IEnumerable<IGrouping<string, Artwork>> ArtworkByUser =>
            FeedResult.Artworks
            .GroupBy(d => d.Author.Username)
            .OrderByDescending(g => g.Select(x => x.Timestamp).Max());

        public IEnumerable<IGrouping<string, StatusUpdate>> StatusesByUser =>
            FeedResult.StatusUpdates
            .GroupBy(s => s.Author.Username)
            .OrderByDescending(g => g.Select(x => x.Timestamp).Max());

        public IEnumerable<FeedItem> Misc =>
            FeedResult.FeedItems
            .Except(FeedResult.Artworks)
            .Except(FeedResult.JournalEntries)
            .Except(FeedResult.StatusUpdates);
    }
}
