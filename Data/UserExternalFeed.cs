using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtworkInbox.Data {
    public class UserExternalFeed {
        public Guid Id { get; set; }

        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        public enum FeedType {
            Atom = 1, RSS = 2
        }

        public FeedType Type { get; set; }

        public string Url { get; set; }

        public DateTimeOffset? LastRead { get; set; }
    }
}
