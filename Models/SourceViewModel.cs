using ArtworkInbox.Backend.Types;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ArtworkInbox.Models {
    public class SourceViewModel {
        public Guid Key { get; set; }
        public DateTimeOffset Latest { get; set; }
        public Author AuthenticatedUser { get; set; }
        public IEnumerable<FeedItem> FeedItems { get; set; }
        public int LastOffset { get; set; }
        public bool HasLess { get; set; }
        public int NextOffset { get; set; }
        public bool HasMore { get; set; }
        public string NotificationsCount { get; set; }
        public string NotificationsUrl { get; set; }
        public string SubmitUrl { get; set; }

        public IEnumerable<IGrouping<string, Artwork>> ArtworkByUser =>
            FeedItems.OfType<Artwork>()
            .GroupBy(d => d.Author.Username)
            .OrderByDescending(g => g.Select(x => x.Timestamp).Max());

        public IEnumerable<IGrouping<string, JournalEntry>> JournalsByUser =>
            FeedItems.OfType<JournalEntry>()
            .GroupBy(s => s.Author.Username)
            .OrderByDescending(g => g.Select(x => x.Timestamp).Max());

        public IEnumerable<IGrouping<string, BlogPost>> BlogPostsByUser =>
            FeedItems.OfType<BlogPost>()
            .GroupBy(s => s.Author.Username)
            .OrderByDescending(g => g.Select(x => x.Timestamp).Max());

        public IEnumerable<IGrouping<string, StatusUpdate>> StatusesByUser =>
            FeedItems.OfType<StatusUpdate>()
            .GroupBy(s => s.Author.Username)
            .OrderByDescending(g => g.Select(x => x.Timestamp).Max());

        public IEnumerable<FeedItem> Misc =>
            FeedItems
            .Except(FeedItems.OfType<Artwork>())
            .Except(FeedItems.OfType<JournalEntry>())
            .Except(FeedItems.OfType<BlogPost>())
            .Except(FeedItems.OfType<FeedItem>());
    }
}
