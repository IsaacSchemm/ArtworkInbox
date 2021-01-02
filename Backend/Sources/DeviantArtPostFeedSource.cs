using ArtworkInbox.Backend.Types;
using DeviantArtFs;
using DeviantArtFs.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class DeviantArtPostFeedSource : IFeedSource, ISource {
        private readonly IDeviantArtAccessToken _token;

        public bool IncludeJournals { get; set; } = true;
        public bool IncludeStatuses { get; set; } = true;

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

        private IEnumerable<FeedItem> Wrangle(IEnumerable<DeviantArtPost> posts) {
            foreach (var p in posts) {
                if (p.journal.OrNull() is Deviation d && IncludeJournals) {
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
                if (p.status.OrNull() is DeviantArtStatus s && IncludeStatuses) {
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

        public async IAsyncEnumerable<FeedItem> GetFeedItemsAsync() {
            var asyncSeq = DeviantArtFs.Api.Browse.PostsByDeviantsYouWatch.ToAsyncSeq(_token, 0);
            var asyncEnum = FSharp.Control.AsyncSeq.toAsyncEnum(asyncSeq);
            await foreach (var p in asyncEnum) {
                if (p.journal.OrNull() is Deviation d && IncludeJournals) {
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
                if (p.status.OrNull() is DeviantArtStatus s && IncludeStatuses) {
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

        public async IAsyncEnumerable<string> GetNotificationsAsync() {
            var asyncSeq = DeviantArtFs.Api.Messages.MessagesFeed.ToAsyncSeq(
                _token,
                new DeviantArtFs.Api.Messages.MessagesFeedRequest { Stack = false },
                Microsoft.FSharp.Core.FSharpOption<string>.None);
            var asyncEnum = FSharp.Control.AsyncSeq.toAsyncEnum(asyncSeq);
            await foreach (var n in asyncEnum) {
                yield return $"{n}";
            }
        }
    }
}
