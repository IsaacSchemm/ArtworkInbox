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
    public class FurAffinityController : SourceController {
        private readonly ApplicationDbContext _context;

        public FurAffinityController(UserManager<ApplicationUser> userManager, IMemoryCache cache, ApplicationDbContext context) : base(userManager, cache) {
            _context = context;
        }

        protected override string SiteName => "FurAffinity";

        protected override async Task<ISource> GetSourceAsync() {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserFurAffinityTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                throw new NoTokenException();
            return new FurAffinityFeedSource(dbToken.FA_COOKIE);
        }

        protected override async Task<DateTimeOffset> GetLastReadAsync() {
            var userId = _userManager.GetUserId(User);
            var dt = await _context.UserFurAffinityTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .Select(t => t.LastRead)
                .SingleOrDefaultAsync();
            return dt ?? DateTimeOffset.MinValue;
        }

        protected override async Task SetLastReadAsync(DateTimeOffset lastRead) {
            var userId = _userManager.GetUserId(User);
            var o = await _context.UserFurAffinityTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .SingleAsync();

            o.LastRead = lastRead;
            await _context.SaveChangesAsync();
        }
    }
}