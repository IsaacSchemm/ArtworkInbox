using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DANotify.Data;
using DANotify.Models;
using DeviantArtFs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DANotify.Controllers {
    public class DeviantArtController : Controller {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IDeviantArtAuth _auth;

        public DeviantArtController(UserManager<IdentityUser> userManager, ApplicationDbContext context, ILogger<HomeController> logger, IDeviantArtAuth auth) {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _auth = auth;
        }

        public IActionResult Index() {
            return RedirectToAction(nameof(Feed));
        }

        public async Task<IActionResult> Feed(string cursor = null, DateTimeOffset? start = null) {
            if (start == null)
                start = DateTimeOffset.UtcNow;

            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserDeviantArtTokens
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                return View("NoAccount");
            var token = new DeviantArtTokenWrapper(_auth, _context, dbToken);

            var lastRead = await _context.UserReadMarkers
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (lastRead == null) {
                lastRead = new UserReadMarker {
                    UserId = userId
                };
                _context.UserReadMarkers.Add(lastRead);
            }

            DateTimeOffset cutoff = lastRead.DeviantArtLastRead ?? DateTimeOffset.MinValue;

            var items = new List<IBclDeviantArtFeedItem>();
            bool hasMore = true;
            for (int i = 0; i < 20 && hasMore; i++) {
                try {
                    var page = await DeviantArtFs.Requests.Feed.FeedHome.ExecuteAsync(token, cursor);
                    cursor = page.Cursor;

                    if (!page.HasMore)
                        hasMore = false;

                    foreach (var x in page.Items) {
                        if (x.Ts.ToUniversalTime() < cutoff) {
                            hasMore = false;
                            break;
                        } else {
                            items.Add(x);
                        }
                    }
                } catch (WebException ex) when (i != 0) {
                    _logger.LogWarning(ex, "Could not load DeviantArt feed");
                    break;
                }
            }

            return View(new DeviantArtFeedViewModel {
                Start = start.Value,
                Cursor = cursor,
                Items = items,
                More = hasMore
            });
        }

        public async Task<IActionResult> MarkAsRead(DateTimeOffset dt) {
            var userId = _userManager.GetUserId(User);
            var lastRead = await _context.UserReadMarkers
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (lastRead == null) {
                lastRead = new UserReadMarker {
                    UserId = userId
                };
                _context.UserReadMarkers.Add(lastRead);
            }

            lastRead.DeviantArtLastRead = dt;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Feed));
        }

        [HttpGet]
        public async Task<IActionResult> FeedSettings() {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserDeviantArtTokens
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                return View("NoAccount");
            var token = new DeviantArtTokenWrapper(_auth, _context, dbToken);
            var feedSettings = await DeviantArtFs.Requests.Feed.FeedSettings.ExecuteAsync(token);
            return View(new DeviantArtFeedSettingsViewModel {
                Statuses = feedSettings.Include.Statuses,
                Deviations = feedSettings.Include.Deviations,
                Journals = feedSettings.Include.Journals,
                GroupDeviations = feedSettings.Include.GroupDeviations,
                Collections = feedSettings.Include.Collections,
                Misc = feedSettings.Include.Misc,
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> FeedSettings(DeviantArtFeedSettingsViewModel model) {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserDeviantArtTokens
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                return View("NoAccount");
            var token = new DeviantArtTokenWrapper(_auth, _context, dbToken);
            await DeviantArtFs.Requests.Feed.FeedSettingsUpdate.ExecuteAsync(token, new DeviantArtFs.Requests.Feed.FeedSettingsUpdateRequest {
                Statuses = model.Statuses,
                Deviations = model.Deviations,
                Journals = model.Journals,
                GroupDeviations = model.GroupDeviations,
                Collections = model.Collections,
                Misc = model.Misc
            });
            return View(model);
        }
    }
}