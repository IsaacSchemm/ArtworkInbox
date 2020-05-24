using ArtworkInbox.Backend.Types;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class FurAffinityFeedSource : IFeedSource, INotificationsSource {
        private readonly string _fa_cookie;

        private FurAffinity.Notifications.CurrentUser _user = null;

        public FurAffinityFeedSource(string fa_cookie) {
            _fa_cookie = fa_cookie;
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            try {
                if (_user == null) {
                    var notifications = await FurAffinity.Notifications.GetSubmissionsAsync(_fa_cookie, sfw: true, from: 0);
                    _user = notifications.current_user;
                }
                return new Author {
                    Username = _user.profile_name,
                    AvatarUrl = null,
                    ProfileUrl = _user.profile
                };
            } catch (Exception ex) when (ex.Message == "Client is rate-limited (too many 429 responses)") {
                throw new TooManyRequestsException();
            }
        }

        public async Task<FeedBatch> GetBatchAsync(string cursor) {
            int from = int.TryParse(cursor, out int i)
                ? i
                : int.MaxValue;
            var sfw = await FurAffinity.Notifications.GetSubmissionsAsync(_fa_cookie, sfw: true, from: from);
            var nsfw = await FurAffinity.Notifications.GetSubmissionsAsync(_fa_cookie, sfw: false, from: from);

            if (_user == null)
                _user = sfw.current_user;

            return new FeedBatch {
                Cursor = $"{nsfw.new_submissions.Select(x => x.id).DefaultIfEmpty(1).Min() - 1}",
                HasMore = nsfw.new_submissions.Any(),
                FeedItems = nsfw.new_submissions.Select(x => new Artwork {
                    Author = new Author {
                        Username = x.profile_name,
                        ProfileUrl = x.profile
                    },
                    LinkUrl = x.link,
                    MatureContent = !sfw.new_submissions.Any(y => x.id == y.id),
                    Thumbnails = new Thumbnail[] {
                        new Thumbnail {
                            Url = x.thumbnail
                        }
                    },
                    Timestamp = new DateTimeOffset(x.id, TimeSpan.Zero),
                    Title = x.title
                })
            };
        }

        public async Task<int> GetNotificationsCountAsync() {
            var ns = await FurAffinity.Notifications.GetOthersAsync(_fa_cookie);

            if (_user == null)
                _user = ns.current_user;

            return ns.notification_counts.Sum - ns.notification_counts.submissions;
        }

        public string GetNotificationsUrl() => "https://www.furaffinity.net/msg/others/";
        public string GetSubmitUrl() => "https://www.furaffinity.net/submit/";
    }
}
