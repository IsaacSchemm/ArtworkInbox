﻿using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ArtworkInbox.Data {
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {
        }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);
            builder.Entity<ApplicationUser>()
                .Property(u => u.HideMatureThumbnails)
                .HasDefaultValue(true);
        }

        public DbSet<UserDeviantArtToken> UserDeviantArtTokens { get; set; }

        public DbSet<UserTwitterToken> UserTwitterTokens { get; set; }

        public DbSet<UserReadMarker> UserReadMarkers { get; set; }
    }
}
