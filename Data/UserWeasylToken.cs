using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtworkInbox.Data {
    public class UserWeasylToken : WeasylFs.IWeasylCredentials {
        [Key]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string ApiKey { get; set; }

        public DateTimeOffset? LastRead { get; set; }
    }
}
