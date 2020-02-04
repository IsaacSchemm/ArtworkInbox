using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtworkInbox.Data {
    public class UserInkbunnyToken {
        [Key]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [Required, Column(TypeName = "varchar(max)")]
        public string Sid { get; set; }

        [Required]
        public string Username { get; set; }

        public DateTimeOffset? LastRead { get; set; }
    }
}
