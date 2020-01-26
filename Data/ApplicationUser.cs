using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;
using WeasylFs;

namespace ArtworkInbox.Data {
    public class ApplicationUser : IdentityUser, IWeasylCredentials {
        public bool HideReposts { get; set; }
        public bool HideMature { get; set; }
        public bool HideMatureThumbnails { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string WeasylApiKey { get; set; }

        [Column(TypeName = "varchar(max)")]
        public string InkbunnySessionId { get; set; }

        string IWeasylCredentials.ApiKey => WeasylApiKey;
    }
}
