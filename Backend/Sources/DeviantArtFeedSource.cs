using ArtworkInbox.Backend.Types;
using DeviantArtFs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class DeviantArtFeedSource : FeedSource {
        private readonly IDeviantArtAccessToken _token;

        public DeviantArtFeedSource(IDeviantArtAccessToken token) {
            _token = token;
        }

        public override async Task<Author> GetAuthenticatedUserAsync() {
            try {
                var user = await DeviantArtFs.Requests.User.Whoami.ExecuteAsync(_token);
                return new Author {
                    Username = user.Username,
                    AvatarUrl = user.Usericon,
                    ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(user.Username)}"
                };
            } catch (Exception ex) when (ex.Message == "Client is rate-limited (too many 429 responses)") {
                throw new TooManyRequestsException();
            }
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
                                Thumbnails = d.Thumbs.Select(x => new Thumbnail {
                                    Url = x.Src,
                                    Width = x.Width,
                                    Height = x.Height
                                }),
                                LinkUrl = d.Url,
                                MatureContent = d.IsMature
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
                        yield return new StatusUpdate {
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
            try {
                var page = await DeviantArtFs.Requests.Feed.FeedHome.ExecuteAsync(_token, cursor);
                return new FeedBatch {
                    Cursor = page.Cursor,
                    HasMore = page.HasMore,
                    FeedItems = Wrangle(page.Items)
                };
            } catch (Exception ex) when (ex.Message == "Client is rate-limited (too many 429 responses)") {
                throw new TooManyRequestsException();
            }
        }

        public override string GetNotificationsUrl() => "https://www.deviantart.com/notifications/feedback";
        public override string GetSubmitUrl() => "https://www.deviantart.com/submit";
    }
}
