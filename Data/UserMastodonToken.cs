using MapleFedNet.Common;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ArtworkInbox.Data {
    public class UserMastodonToken : IMastodonCredentials {
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; }

        [Required]
        public string Host { get; set; }

        [ForeignKey(nameof(UserId))]
        public ApplicationUser User { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string AccessToken { get; set; }

        public DateTimeOffset? LastRead { get; set; }

        string IMastodonCredentials.Domain => Host;
        string IMastodonCredentials.Token => AccessToken;
    }
}
