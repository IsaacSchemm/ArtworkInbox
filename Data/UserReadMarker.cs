﻿using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace DANotify.Data {
    public class UserReadMarker {
        [Key]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public IdentityUser User { get; set; }

        public DateTimeOffset? DeviantArtLastRead { get; set; }

        public DateTimeOffset? TwitterLastRead { get; set; }

        public DateTimeOffset? TumblrLastRead { get; set; }
    }
}