using ArtworkInbox.Backend.Types;
using DeviantArtFs;
using DeviantArtFs.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class DeviantArtPostFeedSource : IFeedSource {
        private readonly IDeviantArtAccessToken _token;

        public DeviantArtPostFeedSource(IDeviantArtAccessToken token) {
            _token = token;
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            try {
                var user = await DeviantArtFs.Api.User.Whoami.ExecuteAsync(_token, DeviantArtObjectExpansion.None);
                return new Author {
                    Username = user.username,
                    AvatarUrl = user.usericon,
                    ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(user.username)}"
                };
            } catch (Exception ex) when (ex.Message == "Client is rate-limited (too many 429 responses)") {
                throw new TooManyRequestsException();
            }
        }

        private static IEnumerable<FeedItem> Wrangle(IEnumerable<DeviantArtPost> posts) {
            foreach (var p in posts) {
                if (p.journal.OrNull() is Deviation d) {
                    yield return new JournalEntry {
                        Author = d.author.OrNull() is DeviantArtUser a
                            ? new Author {
                                Username = a.username,
                                AvatarUrl = a.usericon,
                                ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(a.username)}"
                            } : new Author {
                                Username = "???",
                                AvatarUrl = null,
                                ProfileUrl = null
                            },
                        Timestamp = d.published_time.OrNull() ?? DateTimeOffset.UtcNow,
                        Html = d.excerpt.OrNull() ?? "",
                        LinkUrl = d.url.OrNull() ?? ""
                    };
                }
                if (p.status.OrNull() is DeviantArtStatus s) {
                    yield return new StatusUpdate {
                        Author = s.author.OrNull() is DeviantArtUser a
                            ? new Author {
                                Username = a.username,
                                AvatarUrl = a.usericon,
                                ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(a.username)}"
                            } : new Author {
                                Username = "???",
                                AvatarUrl = null,
                                ProfileUrl = null
                            },
                        Timestamp = s.ts.OrNull() ?? DateTimeOffset.UtcNow,
                        Html = s.body.OrNull() ?? "",
                        LinkUrl = s.url.OrNull() ?? ""
                    };
                }
            }
        }

        public async Task<FeedBatch> GetBatchAsync(string cursor) {
            int offset = int.TryParse(cursor, out int i)
                ? i
                : 0;
            try {
                var page = await DeviantArtFs.Api.Browse.PostsByDeviantsYouWatch.ExecuteAsync(_token, DeviantArtPagingParams.MaxFrom(offset));
                return new FeedBatch {
                    Cursor = $"{page.next_offset.OrNull()}",
                    HasMore = page.has_more,
                    FeedItems = Wrangle(page.results)
                };
            } catch (Exception ex) when (ex.Message == "Client is rate-limited (too many 429 responses)") {
                throw new TooManyRequestsException();
            }
        }

        public string GetNotificationsUrl() => "https://www.deviantart.com/notifications/feedback";
        public string GetSubmitUrl() => "https://www.deviantart.com/submit";
    }
}
