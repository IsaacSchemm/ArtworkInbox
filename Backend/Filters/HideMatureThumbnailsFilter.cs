using ArtworkInbox.Backend.Types;
using System.Collections.Generic;

namespace ArtworkInbox.Backend.Filters {
    public class HideMatureThumbnailsFilter : IFeedFilter {
        public IEnumerable<FeedItem> Apply(IEnumerable<FeedItem> feedItems) {
            foreach (var i in feedItems) {
                if (i is Artwork a) {
                    yield return a.WithMatureThumbnailsHidden();
                } else {
                    yield return i;
                }
            }
        }
    }
}
