using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Util.Store;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Data {
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser>, IDataStore {
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

        public DbSet<UserYouTubeToken> UserYouTubeTokens { get; set; }

        public DbSet<UserExternalFeed> UserExternalFeeds { get; set; }

        protected override void OnModelCreating(ModelBuilder builder) {
            base.OnModelCreating(builder);
            builder.Entity<IdentityRole>()
                .Property(b => b.ConcurrencyStamp)
                .IsETagConcurrency();
            builder.Entity<ApplicationUser>()
                .Property(b => b.ConcurrencyStamp)
                .IsETagConcurrency();
        }

        Task IDataStore.ClearAsync() {
            throw new NotImplementedException();
        }

        Task IDataStore.DeleteAsync<T>(string key) {
            throw new NotImplementedException();
        }

        async Task<T> IDataStore.GetAsync<T>(string key) {
            throw new NotImplementedException();
        }

        async Task IDataStore.StoreAsync<T>(string key, T value) {
            if (value is TokenResponse t) {
                var dbToken = await UserYouTubeTokens.Where(t => t.UserId == key).SingleAsync();
                if (dbToken.AccessToken != t.AccessToken || dbToken.RefreshToken != t.RefreshToken) {
                    dbToken.AccessToken = t.AccessToken;
                    dbToken.RefreshToken = t.RefreshToken;
                    await SaveChangesAsync();
                }
            } else {
                throw new NotImplementedException();
            }
        }
    }
}
