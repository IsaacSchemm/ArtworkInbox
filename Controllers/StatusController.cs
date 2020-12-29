using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtworkInbox.Backend;
using ArtworkInbox.Backend.Filters;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Data;
using ArtworkInbox.Models;
using DeviantArtFs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Tweetinvi.Models;

namespace ArtworkInbox.Controllers {
    [Authorize]
    public class StatusController : FeedController {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        private readonly IReadOnlyConsumerCredentials _consumerCredentials;
        private readonly DeviantArtApp _app;

        public StatusController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, IReadOnlyConsumerCredentials consumerCredentials, DeviantArtApp app) {
            _userManager = userManager;
            _context = context;
            _consumerCredentials = consumerCredentials;
            _app = app;
        }

        protected override Task<ApplicationUser> GetUserAsync() => _userManager.GetUserAsync(User);

        protected override string GetSiteName() => "Status Feed (Twitter / Mastodon / DeviantArt)";

        protected override async Task<IFeedSource> GetFeedSourceAsync() {
            var userId = _userManager.GetUserId(User);

            var feedSources = new List<IFeedSource> {
                new EmptyFeedSource()
            };

            var twitter_rows = await _context.UserTwitterTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
            foreach (var dbToken in twitter_rows) {
                var t = new TwitterCredentials(
                    _consumerCredentials.ConsumerKey,
                    _consumerCredentials.ConsumerSecret,
                    dbToken.AccessToken,
                    dbToken.AccessTokenSecret);
                feedSources.Add(new TwitterFeedSource(t) { IgnoreMedia = true });
            }

            var deviantArt_rows = await _context.UserDeviantArtTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
            foreach (var dbToken in deviantArt_rows) {
                var t = new DeviantArtTokenWrapper(_app, _context, dbToken);
                feedSources.Add(new DeviantArtPostFeedSource(t) { IncludeJournals = false });
            }

            var mastodon_rows = await _context.UserMastodonTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
            foreach (var dbToken in mastodon_rows) {
                feedSources.Add(new MastodonFeedSource(dbToken) { IgnoreMedia = true });
            }

            return new CompositeFeedSource(feedSources);
        }

        protected override Task<DateTimeOffset> GetLastRead() => Task.FromResult(DateTimeOffset.MinValue);

        protected override Task SetLastRead(DateTimeOffset lastRead) => throw new NotImplementedException();
    }
}