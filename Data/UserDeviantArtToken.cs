using DeviantArtFs;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DANotify.Data {
    public class UserDeviantArtToken {
        [Key]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }

        [Column(TypeName = "char(50)")]
        public string AccessToken { get; set; }

        [Column(TypeName = "char(40)")]
        public string RefreshToken { get; set; }
    }
}
