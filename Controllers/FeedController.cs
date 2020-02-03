﻿using System;
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
    public abstract class FeedController : Controller {
        protected abstract Task<ApplicationUser> GetUserAsync();
        protected abstract string GetSiteName();
        protected abstract Task<IFeedSource> GetFeedSourceAsync();
        protected abstract Task<DateTimeOffset> GetLastRead();
        protected abstract Task SetLastRead(DateTimeOffset lastRead);

        public async Task<IActionResult> Feed(string cursor = null, DateTimeOffset? latest = null) {
            try {
                var user = await GetUserAsync();
                var earliest = await GetLastRead();
                var feedSource = await GetFeedSourceAsync();

                var filters = new List<IFeedFilter>();
                if (user.HideReposts)
                    filters.Add(new HideRepostsFilter());
                if (user.HideMature)
                    filters.Add(new HideMatureFilter());
                if (user.HideMatureThumbnails)
                    filters.Add(new HideMatureThumbnailsFilter());

                return View(await FeedViewModel.BuildAsync(feedSource, filters, cursor, earliest, latest));
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

        public async Task<IActionResult> MarkAsRead(DateTimeOffset latest) {
            await SetLastRead(latest);
            return RedirectToAction(nameof(Feed));
        }
    }
}