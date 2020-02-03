using ArtworkInbox.Backend.Types;
using System.Collections.Generic;

namespace ArtworkInbox.Backend.Filters {
    public interface IFeedFilter {
        IEnumerable<FeedItem> Apply(IEnumerable<FeedItem> feedItems);
    }
}
