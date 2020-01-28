using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtworkInbox.Data {
    public class UserMastodonToken {
        public int Id { get; set; }

        [Required]
        public string LoginProvider { get; set; }

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string AccessToken { get; set; }
    }
}
