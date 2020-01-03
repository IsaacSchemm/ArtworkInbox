using DANotify.Backend.Types;
using DeviantArtFs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DANotify.Backend {
    public class DeviantArtFeedSource : FeedSource {
        private readonly IDeviantArtAccessToken _token;

        public DeviantArtFeedSource(IDeviantArtAccessToken token) {
            _token = token;
        }

        public override async Task<Author> GetAuthenticatedUserAsync() {
            var user = await DeviantArtFs.Requests.User.Whoami.ExecuteAsync(_token);
            return new Author {
                Username = user.Username,
                AvatarUrl = user.Usericon,
                ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(user.Username)}"
            };
        }

        private static IEnumerable<FeedItem> Wrangle(IEnumerable<IBclDeviantArtFeedItem> feedItems) {
            foreach (var f in feedItems) {
                var author = new Author {
                    Username = f.ByUser.Username,
                    AvatarUrl = f.ByUser.Usericon,
                    ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(f.ByUser.Username)}"
                };
                switch (f.Type) {
                    case "deviation_submitted":
                        foreach (var d in f.Deviations)
                            yield return new Artwork {
                                Author = author,
                                Timestamp = f.Ts,
                                Title = d.Title,
                                ThumbnailUrl = d.Thumbs
                                    .OrderByDescending(x => x.Width * x.Height)
                                    .Select(x => x.Src)
                                    .FirstOrDefault(),
                                LinkUrl = d.Url
                            };
                        break;
                    case "journal_submitted":
                        foreach (var d in f.Deviations)
                            yield return new JournalEntry {
                                Author = author,
                                Timestamp = f.Ts,
                                Html = d.Excerpt,
                                LinkUrl = d.Url
                            };
                        break;
                    case "status":
                        yield return new JournalEntry {
                            Author = author,
                            Timestamp = f.Ts,
                            Html = f.Status.Body,
                            LinkUrl = f.Status.Url
                        };
                        break;
                    case "username_change":
                        yield return new CustomFeedItem {
                            Author = author,
                            Timestamp = f.Ts,
                            Description = $"{f.Formerly} has changed their username to {f.ByUser.Username}"
                        };
                        break;
                    case "collection_update":
                        yield return new CustomFeedItem {
                            Author = author,
                            Timestamp = f.Ts,
                            Description = $"{f.Formerly} has added {f.AddedCount} deviations to the collection {f.Collection.Name}"
                        };
                        break;
                }
            }
        }

        public override async Task<FeedBatch> GetBatchAsync(string cursor) {
            var page = await DeviantArtFs.Requests.Feed.FeedHome.ExecuteAsync(_token, cursor);
            return new FeedBatch {
                Cursor = page.Cursor,
                HasMore = page.HasMore,
                FeedItems = Wrangle(page.Items)
            };
        }

        public override string GetNotificationsUrl() => "https://www.deviantart.com/notifications/feedback";
    }
}
