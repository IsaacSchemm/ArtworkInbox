using System;
using System.Collections.Generic;
using System.Linq;

namespace ArtworkInbox.Backend.Types {
    public abstract class FeedItem {
        public DateTimeOffset Timestamp { get; set; }
        public Author Author { get; set; }
        public string RepostedFrom { get; set; }

        public abstract string GetDescription();

        public override string ToString() {
            return $"{GetType().Name} - {GetDescription()}";
        }
    }

    public class Thumbnail {
        public string Url { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
    }

    public class Artwork : FeedItem {
        public string Title { get; set; }
        public IEnumerable<Thumbnail> Thumbnails { get; set; }
        public string LinkUrl { get; set; }

        public Thumbnail DefaultThumbnail =>
            Thumbnails
            .OrderBy(t => t.Height)
            .FirstOrDefault();

        public string ThumbnailUrl => DefaultThumbnail?.Url;

        public string Srcset => Thumbnails.Skip(1).Any()
            ? string.Join(", ", Thumbnails.Select(x => $"{x.Url} {1.0 * x.Height / DefaultThumbnail.Height}x"))
            : null;

        public override string GetDescription() => $"{Author.Username}: {Title ?? "[untitled]"}";
    }

    public abstract class TextFeedItem : FeedItem {
        public string Html { get; set; }
        public string LinkUrl { get; set; }

        public string Excerpt => /*Html.Length > 50
            ? Html.Substring(0, 49) + "..."
            :*/ Html;

        public override string GetDescription() => $"{Author.Username}: {Excerpt}";
    }

    public class JournalEntry : TextFeedItem { }
    public class StatusUpdate : TextFeedItem { }

    public class CustomFeedItem : FeedItem {
        public string Description { get; set; }

        public override string GetDescription() => Description;
    }
}
