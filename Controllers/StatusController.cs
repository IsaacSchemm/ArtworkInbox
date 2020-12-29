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
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Tweetinvi;
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

        protected override string GetSiteName() => "Status Updates";

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

        private class Site {
            public string Host { get; set; }
            public Func<string, Task> PostAsync { get; set; }
        }

        private async IAsyncEnumerable<Site> GetHostsAsync() {
            var userId = _userManager.GetUserId(User);

            var twitter_rows = await _context.UserTwitterTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
            foreach (var dbToken in twitter_rows) {
                yield return new Site {
                    Host = "twitter.com",
                    PostAsync = async text => {
                        TwitterClient client = new TwitterClient(
                            _consumerCredentials.ConsumerKey,
                            _consumerCredentials.ConsumerSecret,
                            dbToken.AccessToken,
                            dbToken.AccessTokenSecret);
                        await client.Tweets.PublishTweetAsync(text);
                    }
                };
            }

            var deviantArt_rows = await _context.UserDeviantArtTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
            foreach (var dbToken in deviantArt_rows) {
                yield return new Site {
                    Host = "deviantart.com",
                    PostAsync = async text => {
                        var token = new DeviantArtTokenWrapper(_app, _context, dbToken);
                        await DeviantArtFs.Api.User.StatusPost.ExecuteAsync(token, new DeviantArtFs.Api.User.StatusPostRequest(text));
                    }
                };
            }

            var mastodon_rows = await _context.UserMastodonTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
            foreach (var dbToken in mastodon_rows) {
                yield return new Site {
                    Host = dbToken.Host,
                    PostAsync = async text => {
                        await MapleFedNet.Api.Statuses.Posting(dbToken, text);
                    }
                };
            }
        }

        [HttpGet]
        public async Task<IActionResult> Post() {
            var sites = new List<Site>();
            await foreach (var s in GetHostsAsync()) {
                sites.Add(s);
            }
            return View(new PostStatusModel {
                Sites = sites.Select(s => new PostStatusModel.Site {
                    Host = s.Host,
                    Checked = false
                }).ToList(),
                Text = ""
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Post(PostStatusModel model) {
            var sites = new List<Site>();
            var tasks = new List<Task>();
            await foreach (var s in GetHostsAsync()) {
                sites.Add(s);
                if (model.Sites.Any(x => x.Host == s.Host && x.Checked)) {
                    tasks.Add(s.PostAsync(model.Text));
                }
            }
            await Task.WhenAll(tasks);
            return RedirectToAction(nameof(Feed));
        }
    }
}