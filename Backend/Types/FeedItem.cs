using System;

namespace DANotify.Backend.Types {
    public abstract class FeedItem {
        public DateTimeOffset Timestamp { get; set; }
        public Author Author { get; set; }

        public abstract string GetDescription();

        public override string ToString() {
            return $"{GetType().Name} - {GetDescription()}";
        }
    }

    public class Artwork : FeedItem {
        public string Title { get; set; }
        public string ThumbnailUrl { get; set; }
        public string LinkUrl { get; set; }

        public override string GetDescription() => $"{Author.Username}: {Title ?? "[untitled]"}";
    }

    public abstract class TextFeedItem : FeedItem {
        public string Html { get; set; }
        public string LinkUrl { get; set; }

        public string Excerpt => Html.Length > 50
            ? Html.Substring(0, 49) + "..."
            : Html;

        public override string GetDescription() => $"{Author.Username}: {Excerpt}";
    }

    public class JournalEntry : TextFeedItem { }
    public class StatusUpdate : TextFeedItem { }

    public class CustomFeedItem : FeedItem {
        public string Description { get; set; }

        public override string GetDescription() => Description;
    }
}
