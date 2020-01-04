using System;
using System.Threading.Tasks;
using ArtworkInbox.Backend;
using ArtworkInbox.Models;
using Microsoft.AspNetCore.Mvc;

namespace ArtworkInbox.Controllers {
    public abstract class FeedController : Controller {
        protected abstract string GetSiteName();
        protected abstract Task<FeedSource> GetFeedSourceAsync();
        protected abstract Task<DateTimeOffset> GetLastRead();
        protected abstract Task SetLastRead(DateTimeOffset lastRead);

        public async Task<IActionResult> Feed(string cursor = null, DateTimeOffset? latest = null) {
            try {
                DateTimeOffset earliest = await GetLastRead();
                var feedSource = await GetFeedSourceAsync();
                return View(await FeedViewModel.BuildAsync(feedSource, cursor, earliest, latest));
            } catch (TooManyRequestsException) {
                return View("TooManyRequests");
            } catch (NoTokenException) {
                ViewBag.SiteName = GetSiteName();
                return View("NoToken");
            }
        }

        public async Task<IActionResult> MarkAsRead(DateTimeOffset latest) {
            await SetLastRead(latest);
            return RedirectToAction(nameof(Feed));
        }
    }
}