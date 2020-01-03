using DeviantArtFs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Data {
    public class UserDeviantArtToken {
        [Key]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string AccessToken { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string RefreshToken { get; set; }
    }
}
