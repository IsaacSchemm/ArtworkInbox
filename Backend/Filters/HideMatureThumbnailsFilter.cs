using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Backend.Types;
using System.Collections.Generic;
using System.Linq;

namespace ArtworkInbox.Backend.Filters {
    public class HideMatureThumbnailsFilter : FeedFilter {
        public HideMatureThumbnailsFilter(IFeedSource source) : base(source) { }

        protected override IEnumerable<FeedItem> Apply(IEnumerable<FeedItem> feedItems) {
            foreach (var i in feedItems) {
                if (i.MatureContent && i is Artwork a) {
                    yield return new Artwork {
                        Author = a.Author,
                        LinkUrl = a.LinkUrl,
                        MatureContent = a.MatureContent,
                        RepostedFrom = a.RepostedFrom,
                        Thumbnails = Enumerable.Empty<Thumbnail>(),
                        Timestamp = a.Timestamp,
                        Title = a.Title
                    };
                } else {
                    yield return i;
                }
            }
        }
    }
}
