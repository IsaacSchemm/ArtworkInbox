using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtworkInbox.Backend;
using ArtworkInbox.Backend.Types;
using ArtworkInbox.Data;
using ArtworkInbox.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ArtworkInbox.Controllers {
    [Authorize]
    public abstract class SourceController : Controller {
        protected abstract Task<ApplicationUser> GetUserAsync();
        protected abstract string SiteName { get; }

        protected abstract Task<Author> GetAuthorAsync();

        protected abstract IAsyncEnumerable<FeedItem> GetFeedItemsAsync();
        protected abstract Task<DateTimeOffset> GetLastRead();
        protected abstract Task SetLastRead(DateTimeOffset lastRead);

        protected abstract string NotificationsUrl { get; }
        protected abstract Task<int?> GetNotificationsCountAsync();

        protected abstract string SubmitUrl { get; }

        public async Task<IActionResult> Page(int offset = 0, int limit = 200, DateTimeOffset? latest = null) {
            try {
                var user = await GetUserAsync();
                var earliest = await GetLastRead();

                var source = GetFeedItemsAsync();
                if (user.HideReposts)
                    source = source.Where(x => x.RepostedFrom == null);
                if (user.HideMature)
                    source = source.Where(x => x.MatureContent == false);
                if (user.HideMatureThumbnails)
                    source = source.Select(i => {
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

                var feedItems = await source.Skip(offset).Take(limit).ToListAsync();
                bool hasMore = await source.Skip(offset + limit).AnyAsync();

                return View(new SourceViewModel {
                    Latest = latest ?? feedItems.Select(x => x.Timestamp).DefaultIfEmpty(DateTimeOffset.MinValue).First(),
                    AuthenticatedUser = await GetAuthorAsync(),
                    FeedItems = feedItems,
                    NextOffset = offset + limit,
                    HasMore = hasMore,
                    NotificationsCount = await GetNotificationsCountAsync(),
                    NotificationsUrl = NotificationsUrl,
                    SubmitUrl = SubmitUrl
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
            await SetLastRead(latest);
            return RedirectToAction(nameof(Page));
        }
    }
}