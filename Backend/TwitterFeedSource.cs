using ArtworkInbox.Backend.Types;
using DeviantArtFs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Tweetinvi.Models;

namespace ArtworkInbox.Backend {
    public class TwitterFeedSource : FeedSource {
        private readonly ITwitterCredentials _token;

        public TwitterFeedSource(ITwitterCredentials token) {
            _token = token;
        }

        public override async Task<Author> GetAuthenticatedUserAsync() {
            var user = await Tweetinvi.Auth.ExecuteOperationWithCredentials(_token, () => {
                return Tweetinvi.UserAsync.GetAuthenticatedUser();
            });
            return new Author {
                Username = $"@{user.ScreenName}",
                AvatarUrl = user.ProfileImageUrl,
                ProfileUrl = $"https://twitter.com/{Uri.EscapeDataString(user.ScreenName)}"
            };
        }

        private static IEnumerable<FeedItem> Wrangle(IEnumerable<ITweet> tweets) {
            foreach (var t in tweets) {
                var author = new Author {
                    Username = $"@{t.CreatedBy.ScreenName}",
                    AvatarUrl = t.CreatedBy.ProfileImageUrl,
                    ProfileUrl = $"https://twitter.com/{Uri.EscapeDataString(t.CreatedBy.ScreenName)}"
                };
                var photos = t.Media.Where(m => m.MediaType == "photo");
                foreach (var media in photos) {
                    yield return new Artwork {
                        Author = author,
                        Timestamp = t.CreatedAt,
                        LinkUrl = t.Url,
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
                        LinkUrl = t.Url,
                        Html = WebUtility.HtmlEncode(WebUtility.HtmlDecode(t.FullText ?? t.Text)),
                        RepostedFrom = t.RetweetedTweet?.CreatedBy?.ScreenName
                    };
                }
            }
        }

        public override async Task<FeedBatch> GetBatchAsync(string cursor) {
            var parameters = new Tweetinvi.Parameters.HomeTimelineParameters {
                MaximumNumberOfTweetsToRetrieve = 100
            };
            if (cursor != null && long.TryParse(cursor, out long l))
                parameters.MaxId = l - 1;

            var page = await Tweetinvi.Auth.ExecuteOperationWithCredentials(_token, () => {
                return Tweetinvi.TimelineAsync.GetHomeTimeline(parameters);
            });
            if (page == null) {
                var ex = Tweetinvi.ExceptionHandler.GetLastException();
                if (ex.StatusCode == 429)
                    throw new TooManyRequestsException();
                else
                    throw new Exception("Could not load tweets", ex as Tweetinvi.Exceptions.TwitterException);
            }

            return new FeedBatch {
                Cursor = page.Any()
                    ? $"{page.Select(x => x.Id).Min()}"
                    : cursor,
                HasMore = page.Count() > 0,
                FeedItems = Wrangle(page)
            };
        }

        public override string GetNotificationsUrl() => "https://twitter.com/notifications";
        public override string GetSubmitUrl() => "https://twitter.com/compose/tweet";
    }
}
