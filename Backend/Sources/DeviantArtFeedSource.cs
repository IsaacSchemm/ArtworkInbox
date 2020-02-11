using ArtworkInbox.Backend.Types;
using DeviantArtFs;
using DeviantArtFs.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class DeviantArtFeedSource : IFeedSource, INotificationsSource {
        private readonly IDeviantArtAccessToken _token;

        public DeviantArtFeedSource(IDeviantArtAccessToken token) {
            _token = token;
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            try {
                var user = await DeviantArtFs.Requests.User.Whoami.ExecuteAsync(_token);
                return new Author {
                    Username = user.username,
                    AvatarUrl = user.usericon,
                    ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(user.username)}"
                };
            } catch (Exception ex) when (ex.Message == "Client is rate-limited (too many 429 responses)") {
                throw new TooManyRequestsException();
            }
        }

        private static IEnumerable<FeedItem> Wrangle(IEnumerable<DeviantArtFeedItem> feedItems) {
            foreach (var f in feedItems) {
                var author = new Author {
                    Username = f.by_user.username,
                    AvatarUrl = f.by_user.usericon,
                    ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(f.by_user.username)}"
                };
                switch (f.type) {
                    case "deviation_submitted":
                        foreach (var d in f.deviations.OrEmpty().Where(x => !x.is_deleted))
                            yield return new Artwork {
                                Author = author,
                                Timestamp = f.ts,
                                Title = d.title.OrNull() ?? "",
                                Thumbnails = d.thumbs.OrEmpty().Select(x => new Thumbnail {
                                    Url = x.src,
                                    Width = x.width,
                                    Height = x.height
                                }),
                                LinkUrl = d.url.OrNull(),
                                MatureContent = d.is_mature.OrNull() == true
                            };
                        break;
                    case "journal_submitted":
                        foreach (var d in f.deviations.OrEmpty().Where(x => !x.is_deleted))
                            yield return new JournalEntry {
                                Author = author,
                                Timestamp = f.ts,
                                Html = d.excerpt.OrNull() ?? "",
                                LinkUrl = d.url.OrNull() ?? ""
                            };
                        break;
                    case "status":
                        var status = f.status.OrNull();
                        if (status != null && !status.is_deleted)
                            yield return new StatusUpdate {
                                Author = author,
                                Timestamp = f.ts,
                                Html = status.body.OrNull() ?? "",
                                LinkUrl = status.url.OrNull() ?? ""
                            };
                        break;
                    case "username_change":
                        yield return new CustomFeedItem {
                            Author = author,
                            Timestamp = f.ts,
                            Description = $"{f.formerly.OrNull()} has changed their username to {f.by_user.username}"
                        };
                        break;
                    case "collection_update":
                        yield return new CustomFeedItem {
                            Author = author,
                            Timestamp = f.ts,
                            Description = $"{f.by_user.username} has added {f.added_count.OrNull()} deviations to the collection {f.collection.OrNull().name}"
                        };
                        break;
                    default:
                        yield return new CustomFeedItem {
                            Author = author,
                            Timestamp = f.ts,
                            Description = $"Unknown feed item of type {f.type}"
                        };
                        break;
                }
            }
        }

        public async Task<FeedBatch> GetBatchAsync(string cursor) {
            try {
                var page = await DeviantArtFs.Requests.Feed.FeedHome.ExecuteAsync(_token, cursor);
                return new FeedBatch {
                    Cursor = page.cursor,
                    HasMore = page.has_more,
                    FeedItems = Wrangle(page.items)
                };
            } catch (Exception ex) when (ex.Message == "Client is rate-limited (too many 429 responses)") {
                throw new TooManyRequestsException();
            }
        }

        public async Task<int> GetNotificationsCountAsync() {
            try {
                var ns = await DeviantArtFs.Requests.Feed.FeedNotifications.ToArrayAsync(_token, null, 99);
                return ns.Length;
            } catch (Exception ex) when (ex.Message == "Client is rate-limited (too many 429 responses)") {
                throw new TooManyRequestsException();
            }
        }

        public string GetNotificationsUrl() => "https://www.deviantart.com/notifications/feedback";
        public string GetSubmitUrl() => "https://www.deviantart.com/submit";
    }
}
