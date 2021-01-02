using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtworkInbox.Backend;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Backend.Types;
using ArtworkInbox.Data;
using DeviantArtFs;
using DeviantArtFs.Extensions;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ArtworkInbox.Controllers {
    public class DeviantArtController : SourceController {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly DeviantArtApp _app;

        public DeviantArtController(UserManager<ApplicationUser> userManager, IMemoryCache cache, ApplicationDbContext context, ILogger<HomeController> logger, DeviantArtApp app) : base(userManager, cache) {
            _context = context;
            _logger = logger;
            _app = app;
        }

        protected override string SiteName => "DeviantArt";

        protected override async Task<ISource> GetArtworkSource() {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserDeviantArtTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                throw new NoTokenException();
            var token = new DeviantArtTokenWrapper(_app, _context, dbToken);
            return new CompositeArtworkSource(new ISource[] {
                new DeviantArtDeviationFeedSource(token),
                new DeviantArtPostFeedSource(token)
            });
        }

        protected override async Task<DateTimeOffset> GetLastRead() {
            var userId = _userManager.GetUserId(User);
            var dt = await _context.UserDeviantArtTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .Select(t => t.LastRead)
                .SingleOrDefaultAsync();
            return dt ?? DateTimeOffset.MinValue;
        }

        protected override async Task SetLastRead(DateTimeOffset lastRead) {
            var userId = _userManager.GetUserId(User);
            var o = await _context.UserDeviantArtTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .SingleAsync();

            o.LastRead = lastRead;
            await _context.SaveChangesAsync();
        }
    }
}