using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtworkInbox.Data {
    public class UserYouTubeToken {
        [Key]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string AccessToken { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string RefreshToken { get; set; }

        public DateTimeOffset? LastRead { get; set; }
    }
}
