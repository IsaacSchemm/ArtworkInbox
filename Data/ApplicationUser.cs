using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Data {
    public class ApplicationUser : IdentityUser {
        public bool HideReposts { get; set; }
        public bool HideMature { get; set; }
        public bool HideMatureThumbnails { get; set; }
    }
}
