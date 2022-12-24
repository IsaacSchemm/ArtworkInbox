using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ArtworkInbox.Data {
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser> {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options) {
        }

        public DbSet<UserDeviantArtToken> UserDeviantArtTokens { get; set; }

        public DbSet<UserTwitterToken> UserTwitterTokens { get; set; }

        public DbSet<UserTumblrToken> UserTumblrTokens { get; set; }

        public DbSet<UserMastodonToken> UserMastodonTokens { get; set; }

        public DbSet<UserWeasylToken> UserWeasylTokens { get; set; }

        public DbSet<UserRedditToken> UserRedditTokens { get; set; }

        public DbSet<UserFurAffinityToken> UserFurAffinityTokens { get; set; }

        public DbSet<UserExternalFeed> UserExternalFeeds { get; set; }
    }
}
