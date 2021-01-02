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
using Microsoft.Extensions.Logging;

namespace ArtworkInbox.Controllers {
    public class DeviantArt2Controller : SourceController {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly DeviantArtApp _app;

        public DeviantArt2Controller(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger<HomeController> logger, DeviantArtApp app) {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _app = app;
        }

        public IActionResult Index() {
            return RedirectToAction(nameof(Page ));
        }

        protected override Task<ApplicationUser> GetUserAsync() =>
            _userManager.GetUserAsync(User);

        protected override string SiteName => "DeviantArt";

        protected override string NotificationsUrl => "https://www.deviantart.com/notifications/feedback";
        protected override string SubmitUrl => "https://www.deviantart.com/submit";

        private DeviantArtTokenWrapper _token = null;

        private async Task<DeviantArtTokenWrapper> GetTokenAsync() {
            if (_token == null) {
                var userId = _userManager.GetUserId(User);
                var dbToken = await _context.UserDeviantArtTokens
                    .AsQueryable()
                    .Where(t => t.UserId == userId)
                    .SingleOrDefaultAsync();
                if (dbToken == null)
                    throw new NoTokenException();
                _token = new DeviantArtTokenWrapper(_app, _context, dbToken);
            }
            return _token;
        }

        protected override async Task<Author> GetAuthorAsync() {
            var token = await GetTokenAsync();
            var user = await DeviantArtFs.Api.User.Whoami.ExecuteAsync(token, DeviantArtObjectExpansion.None);
            return new Author {
                Username = user.username,
                ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(user.username)}",
                AvatarUrl = user.usericon
            };
        }

        protected override async IAsyncEnumerable<FeedItem> GetFeedItemsAsync() {
            var token = await GetTokenAsync();
            var asyncSeq = DeviantArtFs.Api.Browse.PostsByDeviantsYouWatch.ToAsyncSeq(token, 0);
            var asyncEnum = FSharp.Control.AsyncSeq.toAsyncEnum(asyncSeq);
            await foreach (var p in asyncEnum) {
                if (p.journal.OrNull() is Deviation d) {
                    yield return new JournalEntry {
                        Author = d.author.OrNull() is DeviantArtUser a
                            ? new Author {
                                Username = a.username,
                                AvatarUrl = a.usericon,
                                ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(a.username)}"
                            } : new Author {
                                Username = "???",
                                AvatarUrl = null,
                                ProfileUrl = null
                            },
                        Timestamp = d.published_time.OrNull() ?? DateTimeOffset.UtcNow,
                        Html = d.excerpt.OrNull() ?? "",
                        LinkUrl = d.url.OrNull() ?? ""
                    };
                }
                if (p.status.OrNull() is DeviantArtStatus s) {
                    yield return new StatusUpdate {
                        Author = s.author.OrNull() is DeviantArtUser a
                            ? new Author {
                                Username = a.username,
                                AvatarUrl = a.usericon,
                                ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(a.username)}"
                            } : new Author {
                                Username = "???",
                                AvatarUrl = null,
                                ProfileUrl = null
                            },
                        Timestamp = s.ts.OrNull() ?? DateTimeOffset.UtcNow,
                        Html = s.body.OrNull() ?? "",
                        LinkUrl = s.url.OrNull() ?? ""
                    };
                }
            }
        }

        protected override async Task<string> GetNotificationsCountAsync() {
            var messages = await DeviantArtFs.Api.Messages.MessagesFeed.ToArrayAsync(
                _token,
                new DeviantArtFs.Api.Messages.MessagesFeedRequest { Stack = false },
                null,
                99);
            return messages.Length == 99
                ? "99+"
                : $"{messages.Length}";
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