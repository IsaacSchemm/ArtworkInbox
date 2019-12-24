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
                return Forbid();

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

            var token = new DeviantArtTokenWrapper(_auth, _context, dbToken);
            var items = new List<IBclDeviantArtFeedItem>();
            bool hasMore = true;
            for (int i = 0; i < 20; i++) {
                try {
                    var page = await DeviantArtFs.Requests.Feed.FeedHome.ExecuteAsync(token, cursor);
                    cursor = page.Cursor;
                    foreach (var x in page.Items) {
                        if (x.Ts < cutoff) {
                            hasMore = false;
                            break;
                        }
                        items.Add(x);
                    }
                    if (!page.HasMore) {
                        hasMore = false;
                        break;
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
    }
}