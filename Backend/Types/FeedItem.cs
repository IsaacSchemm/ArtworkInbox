using System;
using System.Collections.Generic;
using System.Linq;

namespace ArtworkInbox.Backend.Types {
    public abstract record FeedItem {
        public DateTimeOffset Timestamp { get; init; }
        public Author Author { get; init; }
        public string RepostedFrom { get; init; }
        public bool MatureContent { get; init; }

        public abstract string GetDescription();

        public override string ToString() {
            return $"{GetType().Name} - {GetDescription()}";
        }
    }

    public record Thumbnail {
        public string Url { get; init; }
        public int Width { get; init; }
        public int Height { get; init; }
    }

    public record Artwork : FeedItem {
        public string Title { get; init; }
        public IEnumerable<Thumbnail> Thumbnails { get; init; }
        public string LinkUrl { get; init; }

        public Thumbnail DefaultThumbnail =>
            Thumbnails
            .OrderBy(t => t.Height >= 150 ? 1 : 2)
            .ThenBy(t => t.Height)
            .FirstOrDefault();

        public string ThumbnailUrl => DefaultThumbnail?.Url;

        public string Srcset => Thumbnails.Skip(1).Any()
            ? string.Join(", ", Thumbnails.Select(x => $"{x.Url} {1.0 * x.Height / 150}x"))
            : null;

        public Artwork WithMatureThumbnailsHidden() => this with {
            Thumbnails = MatureContent
                ? Enumerable.Empty<Thumbnail>()
                : Thumbnails
        };

        public override string GetDescription() => $"{Author.Username}: {Title ?? "[untitled]"}";
    }

    public abstract record TextFeedItem : FeedItem {
        public string Html { get; init; }
        public string LinkUrl { get; init; }

        public string Excerpt => Html;

        public override string GetDescription() => $"{Author.Username}: {Excerpt}";
    }

    public record JournalEntry : TextFeedItem { }
    public record StatusUpdate : TextFeedItem { }
    public record BlogPost : TextFeedItem { }

    public record CustomFeedItem : FeedItem {
        public string Description { get; init; }

        public override string GetDescription() => Description;
    }
}
