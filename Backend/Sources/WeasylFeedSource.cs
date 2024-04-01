using ArtworkInbox.Backend.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeasylFs;

namespace ArtworkInbox.Backend.Sources {
    public class WeasylFeedSource : ISource {
        private readonly IWeasylCredentials _token;

        public WeasylFeedSource(IWeasylCredentials token) {
            _token = token;
        }

        public string Name => "Weasyl";

        public async Task<Author> GetAuthenticatedUserAsync() {
            var user = await WeasylFs.Endpoints.Whoami.ExecuteAsync(_token);
            var avatar = await WeasylFs.Endpoints.UserAvatar.ExecuteAsync(_token, user.login);
            return new Author {
                Username = user.login,
                AvatarUrl = avatar.avatar,
                ProfileUrl = $"https://www.weasyl.com/~{Uri.EscapeDataString(user.login)}"
            };
        }

        private static IEnumerable<Thumbnail> GetThumbnails(WeasylSubmission submission) {
            var pairs = submission.media
                .Where(p => p.Key == "thumbnail" || p.Key == "thumbnail-generated")
                .SelectMany(x => x.Value);
            foreach (var m in pairs)
                yield return new Thumbnail { Url = m.url };
        }

        public async IAsyncEnumerable<FeedItem> GetFeedItemsAsync() {
            var req = new WeasylFs.Endpoints.MessageSubmissions.Request();
            while (true) {
                var page = await WeasylFs.Endpoints.MessageSubmissions.ExecuteAsync(_token, req);

                foreach (var s in page.submissions) {
                    yield return new Artwork {
                        Author = new Author {
                            Username = s.owner,
                            ProfileUrl = $"https://www.weasyl.com/~{Uri.EscapeDataString(s.owner)}"
                        },
                        LinkUrl = s.link,
                        MatureContent = s.rating != "general",
                        RepostedFrom = s.type == "usercollect" ? "another user" : null,
                        Thumbnails = GetThumbnails(s),
                        Timestamp = s.posted_at,
                        Title = s.title
                    };
                }

                if (page.nexttime_or_null == null)
                    break;

                req.NextTime = page.nexttime_or_null;
            }
        }

        public string GetNotificationsUrl() => "https://www.weasyl.com/messages/notifications";
        public string GetSubmitUrl() => "https://www.weasyl.com/submit";

        public async Task<int?> GetNotificationCountAsync() {
            var o = await WeasylFs.Endpoints.MessageSummary.ExecuteAsync(_token);
            return new[] {
                o.comments,
                o.journals,
                o.notifications
            }.Sum();
        }
    }
}