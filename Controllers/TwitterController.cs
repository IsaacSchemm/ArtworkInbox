using System;
using System.Linq;
using System.Threading.Tasks;
using ArtworkInbox.Backend;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Tweetinvi.Models;

namespace ArtworkInbox.Controllers {
    public class TwitterController : SourceController {
        private readonly ApplicationDbContext _context;
        private readonly IReadOnlyConsumerCredentials _consumerCredentials;

        public TwitterController(UserManager<ApplicationUser> userManager, IMemoryCache cache, ApplicationDbContext context, IReadOnlyConsumerCredentials consumerCredentials) : base(userManager, cache) {
            _context = context;
            _consumerCredentials = consumerCredentials;
        }

        protected override string SiteName => "Twitter";

        protected override async Task<ISource> GetArtworkSource() {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserTwitterTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                throw new NoTokenException();
            var credentials = new TwitterCredentials(
                _consumerCredentials.ConsumerKey,
                _consumerCredentials.ConsumerSecret,
                dbToken.AccessToken,
                dbToken.AccessTokenSecret);
            return new TwitterFeedSource(credentials);
        }

        protected override async Task<DateTimeOffset> GetLastRead() {
            var userId = _userManager.GetUserId(User);
            var dt = await _context.UserTwitterTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .Select(t => t.LastRead)
                .SingleOrDefaultAsync();
            return dt ?? DateTimeOffset.MinValue;
        }

        protected override async Task SetLastRead(DateTimeOffset lastRead) {
            var userId = _userManager.GetUserId(User);
            var o = await _context.UserTwitterTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .SingleAsync();

            o.LastRead = lastRead;
            await _context.SaveChangesAsync();
        }
    }
}