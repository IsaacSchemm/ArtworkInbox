using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading.Tasks;
using ArtworkInbox.Backend;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Backend.Types;
using ArtworkInbox.Data;
using ArtworkInbox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace ArtworkInbox.Controllers {
    [Authorize]
    public abstract class SourceController : Controller {
        public record ControllerCacheItem {
            public Guid Id { get; init; }
            public string LocalUserId { get; init; }
            public Author RemoteUser { get; init; }
            public AsyncEnumerableCache<FeedItem> FeedItems { get; init; }
            public string NotificationsCount { get; init; }
            public string NotificationsUrl { get; init; }
            public string SubmitUrl { get; init; }
            public DateTimeOffset Earliest { get; init; }
        }

        protected UserManager<ApplicationUser> _userManager;
        protected IMemoryCache _cache;

        protected SourceController(UserManager<ApplicationUser> userManager, IMemoryCache cache) {
            _cache = cache;
            _userManager = userManager;
        }

        protected abstract Task<ISource> GetSourceAsync();
        protected abstract Task<DateTimeOffset> GetLastReadAsync();
        protected abstract Task SetLastReadAsync(DateTimeOffset lastRead);

        protected abstract string SiteName { get; }

        public IActionResult Index() {
            return RedirectToAction(nameof(Feed));
        }

        public async Task<IActionResult> Feed(int offset = 0, int limit = 20, DateTimeOffset? latest = null, Guid? key = null) {
            try {
                var user = await _userManager.GetUserAsync(User);

                ControllerCacheItem cacheItem;
                if (key != null && _cache.TryGetValue(key, out object o) && o is ControllerCacheItem c && c.LocalUserId == user.Id) {
                    cacheItem = c;
                } else {
                    var source = await GetSourceAsync();
                    int ct = await source.GetNotificationsAsync().Take(20).CountAsync();
                    cacheItem = new ControllerCacheItem {
                        Id = Guid.NewGuid(),
                        LocalUserId = user.Id,
                        RemoteUser = await source.GetAuthenticatedUserAsync(),
                        FeedItems = new AsyncEnumerableCache<FeedItem>(source.GetFeedItemsAsync()),
                        NotificationsCount = ct <= 0 ? null
                            : ct >= 20 ? "20+"
                            : $"{ct}",
                        NotificationsUrl = source.GetNotificationsUrl(),
                        SubmitUrl = source.GetSubmitUrl(),
                        Earliest = await GetLastReadAsync()
                    };
                    _cache.Set(cacheItem.Id, cacheItem, DateTimeOffset.UtcNow.AddMinutes(15));
                }

                IAsyncEnumerable<FeedItem> feedItems = cacheItem.FeedItems.TakeWhile(x => x.Timestamp > cacheItem.Earliest);

                if (user.HideReposts)
                    feedItems = feedItems.Where(x => x.RepostedFrom == null);
                if (user.HideMature)
                    feedItems = feedItems.Where(x => x.MatureContent == false);
                if (user.HideMatureThumbnails)
                    feedItems = feedItems.Select(i => {
                        if (i.MatureContent && i is Artwork a) {
                            return new Artwork {
                                Author = a.Author,
                                LinkUrl = a.LinkUrl,
                                MatureContent = a.MatureContent,
                                RepostedFrom = a.RepostedFrom,
                                Thumbnails = Enumerable.Empty<Thumbnail>(),
                                Timestamp = a.Timestamp,
                                Title = a.Title
                            };
                        } else {
                            return i;
                        }
                    });

                var page = await feedItems.Skip(offset).Take(limit).ToListAsync();
                bool hasMore = await feedItems.Skip(offset + limit).AnyAsync();

                return View("NewFeed", new SourceViewModel {
                    Key = cacheItem.Id,
                    Latest = latest ?? page.Select(x => x.Timestamp).DefaultIfEmpty(DateTimeOffset.MinValue).First(),
                    AuthenticatedUser = cacheItem.RemoteUser,
                    FeedItems = page,
                    NextOffset = offset + limit,
                    HasMore = hasMore,
                    NotificationsCount = cacheItem.NotificationsCount,
                    NotificationsUrl = cacheItem.NotificationsUrl,
                    SubmitUrl = cacheItem.SubmitUrl
                });
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
            ViewBag.SiteName = SiteName;
            return View("JsonError");
        }

        public IActionResult TooManyRequests() {
            ViewBag.SiteName = SiteName;
            return View("TooManyRequests");
        }

        public IActionResult NoToken() {
            ViewBag.SiteName = SiteName;
            return View("NoToken");
        }

        public async Task<IActionResult> MarkAsRead(DateTimeOffset latest) {
            await SetLastReadAsync(latest);
            return RedirectToAction(nameof(Feed));
        }
    }
}