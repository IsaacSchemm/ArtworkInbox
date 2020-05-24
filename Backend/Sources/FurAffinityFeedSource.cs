using ArtworkInbox.Backend.Types;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class FurAffinityFeedSource : IFeedSource, INotificationsSource {
        private readonly string _fa_cookie;

        public FurAffinityFeedSource(string fa_cookie) {
            _fa_cookie = fa_cookie;
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            try {
                var notifications = await FurAffinity.Notifications.GetSubmissionsAsync(_fa_cookie, 0);
                return new Author {
                    Username = notifications.current_user.profile_name,
                    AvatarUrl = null,
                    ProfileUrl = notifications.current_user.profile
                };
            } catch (Exception ex) when (ex.Message == "Client is rate-limited (too many 429 responses)") {
                throw new TooManyRequestsException();
            }
        }

        public async Task<FeedBatch> GetBatchAsync(string cursor) {
            int from = int.TryParse(cursor, out int i)
                ? i
                : int.MaxValue;
            var page = await FurAffinity.Notifications.GetSubmissionsAsync(_fa_cookie, from);
            return new FeedBatch {
                Cursor = $"{page.new_submissions.Select(x => x.id).DefaultIfEmpty(1).Min() - 1}",
                HasMore = page.new_submissions.Any(),
                FeedItems = page.new_submissions.Select(x => new Artwork {
                    Author = new Author {
                        Username = x.profile_name,
                        ProfileUrl = x.profile
                    },
                    LinkUrl = x.link,
                    Thumbnails = new Thumbnail[] {
                        new Thumbnail {
                            Url = x.thumbnail
                        }
                    },
                    Title = x.title
                })
            };
        }

        public async Task<int> GetNotificationsCountAsync() {
            var ns = await FurAffinity.Notifications.GetOthersAsync(_fa_cookie);
            return ns.notification_counts.Sum - ns.notification_counts.submissions;
        }

        public string GetNotificationsUrl() => "https://www.furaffinity.net/msg/others/";
        public string GetSubmitUrl() => "https://www.furaffinity.net/submit/";
    }
}
