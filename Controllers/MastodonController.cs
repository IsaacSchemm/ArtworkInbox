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
    public class MastodonController : SourceController {
        private readonly ApplicationDbContext _context;

        public MastodonController(UserManager<ApplicationUser> userManager, IMemoryCache cache, ApplicationDbContext context) : base(userManager, cache) {
            _userManager = userManager;
            _context = context;
        }

        protected override string SiteName => "Mastodon";

        protected override async Task<ISource> GetSourceAsync() {
            var userId = _userManager.GetUserId(User);
            var dbTokens = await _context.UserMastodonTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .ToListAsync();
            if (!dbTokens.Any())
                throw new NoTokenException();
            return new CompositeSource(dbTokens.Select(t => new MastodonFeedSource(t.Host, t.AccessToken)));
        }

        protected override async Task<DateTimeOffset> GetLastReadAsync() {
            var userId = _userManager.GetUserId(User);
            var timestamps = await _context.UserMastodonTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .Select(t => t.LastRead)
                .ToListAsync();
            return timestamps
                .Select(x => x ?? DateTimeOffset.MinValue)
                .Min();
        }

        protected override async Task SetLastReadAsync(DateTimeOffset lastRead) {
            var userId = _userManager.GetUserId(User);
            var dbTokens = await _context.UserMastodonTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .ToListAsync();

            foreach (var o in dbTokens)
                o.LastRead = lastRead;
            await _context.SaveChangesAsync();
        }
    }
}