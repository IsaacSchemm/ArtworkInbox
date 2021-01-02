using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Data;
using DeviantArtFs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Tweetinvi.Models;

namespace ArtworkInbox.Controllers {
    [Authorize]
    public class StatusController : SourceController {
        private readonly ApplicationDbContext _context;
        private readonly IReadOnlyConsumerCredentials _consumerCredentials;
        private readonly DeviantArtApp _app;

        public StatusController(UserManager<ApplicationUser> userManager, IMemoryCache cache, ApplicationDbContext context, IReadOnlyConsumerCredentials consumerCredentials, DeviantArtApp app) : base(userManager, cache) {
            _context = context;
            _consumerCredentials = consumerCredentials;
            _app = app;
        }

        protected override string SiteName => "Status Feed (Twitter / Mastodon / DeviantArt)";

        protected override async Task<ISource> GetSourceAsync() {
            var userId = _userManager.GetUserId(User);

            var feedSources = new List<ISource> {
                new EmptySource()
            };

            var twitter_rows = await _context.UserTwitterTokens
                .AsQueryable()
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
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .ToListAsync();
            foreach (var dbToken in deviantArt_rows) {
                var t = new DeviantArtTokenWrapper(_app, _context, dbToken);
                feedSources.Add(new DeviantArtPostFeedSource(t) { IncludeJournals = false });
            }

            var mastodon_rows = await _context.UserMastodonTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .ToListAsync();
            foreach (var dbToken in mastodon_rows) {
                feedSources.Add(new MastodonFeedSource(dbToken.Host, dbToken.AccessToken));
            }

            return new CompositeSource(feedSources);
        }

        protected override Task<DateTimeOffset> GetLastReadAsync() => Task.FromResult(DateTimeOffset.MinValue);

        protected override Task SetLastReadAsync(DateTimeOffset lastRead) => throw new NotImplementedException();
    }
}