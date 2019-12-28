using System;
using System.Threading.Tasks;
using DANotify.Backend;
using DANotify.Models;
using Microsoft.AspNetCore.Mvc;

namespace DANotify.Controllers {
    public abstract class FeedController : Controller {
        protected abstract Task<FeedSource> GetFeedSourceAsync();
        protected abstract Task<DateTimeOffset> GetLastRead();
        protected abstract Task SetLastRead(DateTimeOffset lastRead);

        public async Task<IActionResult> Feed(string cursor = null, DateTimeOffset? latest = null) {
            DateTimeOffset earliest = await GetLastRead();
            var feedSource = await GetFeedSourceAsync();
            return View(await FeedViewModel.BuildAsync(feedSource, cursor, earliest, latest));
        }

        public async Task<IActionResult> MarkAsRead(DateTimeOffset latest) {
            await SetLastRead(latest);
            return RedirectToAction(nameof(Feed));
        }
    }
}