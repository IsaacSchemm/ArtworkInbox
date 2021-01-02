using ArtworkInbox.Backend.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tweetinvi;
using Tweetinvi.Exceptions;
using Tweetinvi.Models;

namespace ArtworkInbox.Backend.Sources {
    public class TwitterFeedSource : IFeedSource, ISource {
        private readonly TwitterClient _client;

        public bool IgnoreMedia { get; set; } = false;

        public TwitterFeedSource(IReadOnlyTwitterCredentials token) {
            _client = new TwitterClient(token);
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            var user = await _client.Users.GetAuthenticatedUserAsync();
            return new Author {
                Username = $"@{user.ScreenName}",
                AvatarUrl = user.ProfileImageUrl,
                ProfileUrl = $"https://twitter.com/{Uri.EscapeDataString(user.ScreenName)}"
            };
        }

        private IEnumerable<FeedItem> Wrangle(IEnumerable<ITweet> tweets) {
            foreach (var t in tweets) {
                var author = new Author {
                    Username = $"@{t.CreatedBy.ScreenName}",
                    AvatarUrl = t.CreatedBy.ProfileImageUrl,
                    ProfileUrl = $"https://twitter.com/{Uri.EscapeDataString(t.CreatedBy.ScreenName)}"
                };
                var photos = IgnoreMedia
                    ? Enumerable.Empty<Tweetinvi.Models.Entities.IMediaEntity>()
                    : t.Media.Where(m => m.MediaType == "photo");
                foreach (var media in photos) {
                    yield return new Artwork {
                        Author = author,
                        Timestamp = t.CreatedAt,
                        LinkUrl = t.RetweetedTweet?.Url ?? t.Url,
                        Thumbnails = media.Sizes
                            .Where(x => x.Value.Resize == "fit")
                            .Select(x => new Thumbnail {
                                Url = $"{media.MediaURLHttps}?format=jpg&name={x.Key}",
                                Width = x.Value.Width ?? 0,
                                Height = x.Value.Height ?? 0
                            }),
                        RepostedFrom = t.RetweetedTweet?.CreatedBy?.ScreenName,
                        MatureContent = t.PossiblySensitive
                    };
                }
                if (!photos.Any()) {
                    yield return new StatusUpdate {
                        Author = author,
                        Timestamp = t.CreatedAt,
                        LinkUrl = t.RetweetedTweet?.Url ?? t.Url,
                        Html = WebUtility.HtmlEncode(WebUtility.HtmlDecode(t.FullText ?? t.Text)),
                        RepostedFrom = t.RetweetedTweet?.CreatedBy?.ScreenName
                    };
                }
            }
        }

        public async Task<FeedBatch> GetBatchAsync(string cursor) {
            var parameters = new Tweetinvi.Parameters.GetHomeTimelineParameters {
                PageSize = 100
            };
            if (cursor != null && long.TryParse(cursor, out long l))
                parameters.MaxId = l - 1;

            try {
                var page = await _client.Timelines.GetHomeTimelineAsync(parameters);

                return new FeedBatch {
                    Cursor = page.Any()
                        ? $"{page.Select(x => x.Id).Min()}"
                        : cursor,
                    HasMore = page.Length > 0,
                    FeedItems = Wrangle(page)
                };
            } catch (TwitterException ex) when (ex.StatusCode == 429) {
                throw new TooManyRequestsException();
            }
        }

        public string GetNotificationsUrl() => "https://twitter.com/notifications";
        public string GetSubmitUrl() => "https://twitter.com/compose/tweet";

        public async IAsyncEnumerable<FeedItem> GetFeedItemsAsync() {
            var parameters = new Tweetinvi.Parameters.GetHomeTimelineParameters {
                PageSize = 100
            };
            while (true) {
                ITweet[] page;
                try {
                    page = await _client.Timelines.GetHomeTimelineAsync(parameters);
                } catch (TwitterException ex) when (ex.StatusCode == 429) {
                    throw new TooManyRequestsException();
                }
                foreach (var t in page) {
                    var author = new Author {
                        Username = $"@{t.CreatedBy.ScreenName}",
                        AvatarUrl = t.CreatedBy.ProfileImageUrl,
                        ProfileUrl = $"https://twitter.com/{Uri.EscapeDataString(t.CreatedBy.ScreenName)}"
                    };
                    var photos = IgnoreMedia
                        ? Enumerable.Empty<Tweetinvi.Models.Entities.IMediaEntity>()
                        : t.Media.Where(m => m.MediaType == "photo");
                    foreach (var media in photos) {
                        yield return new Artwork {
                            Author = author,
                            Timestamp = t.CreatedAt,
                            LinkUrl = t.RetweetedTweet?.Url ?? t.Url,
                            Thumbnails = media.Sizes
                                .Where(x => x.Value.Resize == "fit")
                                .Select(x => new Thumbnail {
                                    Url = $"{media.MediaURLHttps}?format=jpg&name={x.Key}",
                                    Width = x.Value.Width ?? 0,
                                    Height = x.Value.Height ?? 0
                                }),
                            RepostedFrom = t.RetweetedTweet?.CreatedBy?.ScreenName,
                            MatureContent = t.PossiblySensitive
                        };
                    }
                    if (!photos.Any()) {
                        yield return new StatusUpdate {
                            Author = author,
                            Timestamp = t.CreatedAt,
                            LinkUrl = t.RetweetedTweet?.Url ?? t.Url,
                            Html = WebUtility.HtmlEncode(WebUtility.HtmlDecode(t.FullText ?? t.Text)),
                            RepostedFrom = t.RetweetedTweet?.CreatedBy?.ScreenName
                        };
                    }
                }
                parameters.MaxId = page.Select(x => x.Id).Min() - 1;
            }
        }

        public IAsyncEnumerable<string> GetNotificationsAsync() => AsyncEnumerable.Empty<string>();
    }
}
