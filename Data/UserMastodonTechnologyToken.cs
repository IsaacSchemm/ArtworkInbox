using MapleFedNet.Common;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtworkInbox.Data {
    public class UserMastodonTechnologyToken : IMastodonCredentials {
        [Key]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string AccessToken { get; set; }

        string IMastodonCredentials.Domain => "mastodon.technology";
        string IMastodonCredentials.Token => AccessToken;
    }
}
