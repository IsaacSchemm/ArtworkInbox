using ArtworkInbox.Backend.Types;
using System.Collections.Generic;

namespace ArtworkInbox.Backend.Filters {
    public class HideMatureFilter : IFeedFilter {
        public IEnumerable<FeedItem> Apply(IEnumerable<FeedItem> feedItems) {
            foreach (var i in feedItems) {
                if (!i.MatureContent)
                    yield return i;
            }
        }
    }
}
