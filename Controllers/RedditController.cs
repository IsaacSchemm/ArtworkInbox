using System;
using System.Linq;
using System.Threading.Tasks;
using ArtworkInbox.Backend;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ArtworkInbox.Controllers {
    public class RedditController : SourceController {
        private readonly ApplicationDbContext _context;
        private readonly ArtworkInboxRedditCredentials _credentials;

        public RedditController(UserManager<ApplicationUser> userManager, IMemoryCache cache, ApplicationDbContext context, ArtworkInboxRedditCredentials credentials) : base(userManager, cache) {
            _context = context;
            _credentials = credentials;
        }

        protected override string SiteName => "Reddit";

        protected override async Task<ISource> GetSourceAsync() {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserRedditTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                throw new NoTokenException();
            var client = new Reddit.RedditClient(_credentials.AppId, dbToken.RefreshToken, _credentials.AppSecret, dbToken.AccessToken, "ArtowkrInbox/0.0 (https://artworkinbox.azurewebsites.net)");
            client.Models.OAuthCredentials.TokenUpdated += (o, e) => {
                dbToken.AccessToken = e.AccessToken;
                _context.SaveChanges();
            };
            return new RedditFeedSource(client);
        }

        protected override async Task<DateTimeOffset> GetLastReadAsync() {
            var userId = _userManager.GetUserId(User);
            var dt = await _context.UserRedditTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .Select(t => t.LastRead)
                .SingleOrDefaultAsync();
            return dt ?? DateTimeOffset.MinValue;
        }

        protected override async Task SetLastReadAsync(DateTimeOffset lastRead) {
            var userId = _userManager.GetUserId(User);
            var o = await _context.UserRedditTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .SingleAsync();

            o.LastRead = lastRead;
            await _context.SaveChangesAsync();
        }
    }
}