using System;
using System.Linq;
using System.Threading.Tasks;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace ArtworkInbox.Controllers {
    public class ExternalFeedController : SourceController {
        private readonly ApplicationDbContext _context;

        public ExternalFeedController(UserManager<ApplicationUser> userManager, IMemoryCache cache, ApplicationDbContext context) : base(userManager, cache) {
            _context = context;
        }

        protected override string SiteName => "RSS / Atom";

        protected override async Task<ISource> GetSourceAsync() {
            string userId = _userManager.GetUserId(User);
            var feeds = await _context.UserExternalFeeds
                .AsNoTracking()
                .Where(f => f.UserId == userId)
                .ToListAsync();
            return feeds.Count == 1
                ? new ExternalFeedSource(feeds.Single())
                : new CompositeSource(feeds.Select(x => new ExceptionEater(new ExternalFeedSource(x))));
        }

        // For external feeds, the hiding of already-read items is handled inside ExternalFeedSource.
        protected override Task<DateTimeOffset> GetLastReadAsync() => Task.FromResult(DateTimeOffset.MinValue);

        protected override async Task SetLastReadAsync(DateTimeOffset lastRead) {
            var userId = _userManager.GetUserId(User);
            var feeds = await _context.UserExternalFeeds
                .AsQueryable()
                .Where(f => f.UserId == userId)
                .ToListAsync();

            foreach (var f in feeds)
                f.LastRead = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();
        }
    }
}