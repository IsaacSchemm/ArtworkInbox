using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ArtworkInbox.Backend;
using ArtworkInbox.Backend.Filters;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Data;
using ArtworkInbox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtworkInbox.Controllers {
    [Authorize]
    public abstract class MultiFeedController : Controller {
        protected abstract Task<ApplicationUser> GetUserAsync();
        protected abstract string GetSiteName();
        protected abstract Task<IFeedSource> GetFeedSourceAsync(string host);
        protected abstract Task<DateTimeOffset> GetLastRead(string host);
        protected abstract Task SetLastRead(string host, DateTimeOffset lastRead);

        public async Task<IActionResult> Feed(string host = null, string cursor = null, DateTimeOffset? latest = null) {
            try {
                var user = await GetUserAsync();
                var earliest = await GetLastRead(host);
                var feedSource = await GetFeedSourceAsync(host);

                var filters = new List<IFeedFilter>();
                if (user.HideReposts)
                    filters.Add(new HideRepostsFilter());
                if (user.HideMature)
                    filters.Add(new HideMatureFilter());
                if (user.HideMatureThumbnails)
                    filters.Add(new HideMatureThumbnailsFilter());

                return View(await FeedViewModel.BuildAsync(host, feedSource, filters, cursor, earliest, latest, StopAtCount));
            } catch (System.Text.Json.JsonException) {
                return JsonError();
            } catch (Newtonsoft.Json.JsonException) {
                return JsonError();
            } catch (FSharp.Json.JsonDeserializationError) {
                return JsonError();
            } catch (TooManyRequestsException) {
                return TooManyRequests();
            } catch (NoTokenException) {
                return NoToken();
            }
        }

        public IActionResult JsonError() {
            ViewBag.SiteName = GetSiteName();
            return View("JsonError");
        }

        public IActionResult TooManyRequests() {
            ViewBag.SiteName = GetSiteName();
            return View("TooManyRequests");
        }

        public IActionResult NoToken() {
            ViewBag.SiteName = GetSiteName();
            return View("NoToken");
        }

        public async Task<IActionResult> MarkAsRead(DateTimeOffset latest, string host = null) {
            await SetLastRead(host, latest);
            return RedirectToAction(nameof(Feed), new { host });
        }
    }

    public abstract class FeedController : MultiFeedController {
        protected abstract Task<IFeedSource> GetFeedSourceAsync();
        protected override Task<IFeedSource> GetFeedSourceAsync(string _) => GetFeedSourceAsync();

        protected abstract Task<DateTimeOffset> GetLastRead();
        protected override Task<DateTimeOffset> GetLastRead(string _) => GetLastRead();

        protected abstract Task SetLastRead(DateTimeOffset lastRead);
        protected override Task SetLastRead(string _, DateTimeOffset lastRead) => SetLastRead(lastRead);
    }
}