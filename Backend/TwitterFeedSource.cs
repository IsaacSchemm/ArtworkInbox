using DANotify.Backend.Types;
using DeviantArtFs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tweetinvi.Models;

namespace DANotify.Backend {
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
                Username = user.ScreenName,
                AvatarUrl = user.ProfileImageUrl,
                ProfileUrl = user.Url
            };
        }

        private static IEnumerable<FeedItem> Wrangle(IEnumerable<ITweet> tweets) {
            foreach (var t in tweets) {
                var author = new Author {
                    Username = t.CreatedBy.ScreenName,
                    AvatarUrl = t.CreatedBy.ProfileImageUrl,
                    ProfileUrl = t.CreatedBy.Url
                };
                foreach (var media in t.Media) {
                    yield return new Artwork {
                        Author = author,
                        Timestamp = t.CreatedAt,
                        LinkUrl = t.Url,
                        ThumbnailUrl = media.MediaType == "photo" || media.MediaType == "animated_gif"
                            ? media.MediaURL
                            : null
                    };
                }
                if (!t.Media.Any()) {
                    yield return new StatusUpdate {
                        Author = author,
                        Timestamp = t.CreatedAt,
                        LinkUrl = t.Url,
                        Html = t.Text
                    };
                }
            }
        }

        public override async Task<FeedBatch> GetBatchAsync(string cursor) {
            var parameters = new Tweetinvi.Parameters.HomeTimelineParameters();
            if (cursor != null && long.TryParse(cursor, out long l))
                parameters.MaxId = l - 1;

            var page = await Tweetinvi.Auth.ExecuteOperationWithCredentials(_token, () => {
                return Tweetinvi.TimelineAsync.GetHomeTimeline(parameters);
            });
            if (page == null)
                throw Tweetinvi.ExceptionHandler.GetLastException() is Tweetinvi.Exceptions.TwitterException t
                    ? new Exception("Could not load tweets", t)
                    : new Exception("Could not load tweets");

            return new FeedBatch {
                Cursor = page.Any()
                    ? $"{page.Select(x => x.Id).Min()}"
                    : cursor,
                HasMore = page.Count() > 0,
                FeedItems = Wrangle(page)
            };
        }

        public override async Task<bool> HasNotificationsAsync() {
            return false;
        }

        public override string GetNotificationsUrl() => "https://twitter.com/notifications";
    }
}
