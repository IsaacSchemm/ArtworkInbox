using DANotify.Backend.Types;
using System.Collections.Generic;
using System.Linq;

namespace DANotify.Backend {
    public class FeedBatch {
        public string Cursor { get; set; }
        public bool HasMore { get; set; }
        public IEnumerable<FeedItem> FeedItems { get; set; }

        public IEnumerable<Artwork> Artworks {
            get {
                foreach (var item in FeedItems)
                    if (item is Artwork o)
                        yield return o;
            }
        }

        public IEnumerable<JournalEntry> JournalEntries {
            get {
                foreach (var item in FeedItems)
                    if (item is JournalEntry o)
                        yield return o;
            }
        }

        public IEnumerable<StatusUpdate> StatusUpdates {
            get {
                foreach (var item in FeedItems)
                    if (item is StatusUpdate o)
                        yield return o;
            }
        }
    }
}
