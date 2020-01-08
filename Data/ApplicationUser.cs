using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Data {
    public class ApplicationUser : IdentityUser {
        public bool HideMature { get; set; }

        public bool HideMatureThumbnails { get; set; }
    }
}
