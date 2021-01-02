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
    public abstract class MastodonController : SourceController {
        private readonly ApplicationDbContext _context;

        public MastodonController(UserManager<ApplicationUser> userManager, IMemoryCache cache, ApplicationDbContext context) : base(userManager, cache) {
            _userManager = userManager;
            _context = context;
        }

        protected abstract string Host { get; }

        protected override string SiteName => Host;

        protected override async Task<ISource> GetArtworkSource() {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserMastodonTokens
                .AsQueryable()
                .Where(t => t.Host == Host)
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                throw new NoTokenException();
            return new MastodonFeedSource(dbToken);
        }

        protected override async Task<DateTimeOffset> GetLastRead() {
            var userId = _userManager.GetUserId(User);
            var dt = await _context.UserMastodonTokens
                .AsQueryable()
                .Where(t => t.Host == Host)
                .Where(t => t.UserId == userId)
                .Select(t => t.LastRead)
                .SingleOrDefaultAsync();
            return dt ?? DateTimeOffset.MinValue;
        }

        protected override async Task SetLastRead(DateTimeOffset lastRead) {
            var userId = _userManager.GetUserId(User);
            var o = await _context.UserMastodonTokens
                .AsQueryable()
                .Where(t => t.Host == Host)
                .Where(t => t.UserId == userId)
                .SingleAsync();

            o.LastRead = lastRead;
            await _context.SaveChangesAsync();
        }
    }
}