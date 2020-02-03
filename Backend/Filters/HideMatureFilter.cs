using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Backend.Types;
using System.Collections.Generic;

namespace ArtworkInbox.Backend.Filters {
    public class HideMatureFilter : FeedFilter {
        public HideMatureFilter(IFeedSource source) : base(source) { }

        protected override IEnumerable<FeedItem> Apply(IEnumerable<FeedItem> feedItems) {
            foreach (var i in feedItems) {
                if (!i.MatureContent)
                    yield return i;
            }
        }
    }
}
