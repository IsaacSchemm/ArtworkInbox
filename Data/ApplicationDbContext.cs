using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DANotify.Data {
    public class ApplicationDbContext : IdentityDbContext {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {
        }

        public DbSet<UserDeviantArtToken> UserDeviantArtTokens { get; set; }

        public DbSet<UserTwitterToken> UserTwitterTokens { get; set; }

        public DbSet<UserReadMarker> UserReadMarkers { get; set; }
    }
}
