using System;
using System.Linq;
using System.Threading.Tasks;
using ArtworkInbox.Backend;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArtworkInbox.Controllers {
    public class RedditController : FeedController {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly ArtworkInboxRedditCredentials _credentials;

        public RedditController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger<HomeController> logger, ArtworkInboxRedditCredentials credentials) {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _credentials = credentials;
        }

        public IActionResult Index() {
            return RedirectToAction(nameof(Feed));
        }

        protected override Task<ApplicationUser> GetUserAsync() =>
            _userManager.GetUserAsync(User);

        protected override string GetSiteName() => "Reddit";

        protected override async Task<IFeedSource> GetFeedSourceAsync() {
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

        protected override async Task<DateTimeOffset> GetLastRead() {
            var userId = _userManager.GetUserId(User);
            var dt = await _context.UserRedditTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .Select(t => t.LastRead)
                .SingleOrDefaultAsync();
            return dt ?? DateTimeOffset.MinValue;
        }

        protected override async Task SetLastRead(DateTimeOffset lastRead) {
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