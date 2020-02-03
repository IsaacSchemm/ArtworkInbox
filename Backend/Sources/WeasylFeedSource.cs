using ArtworkInbox.Backend.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WeasylFs;

namespace ArtworkInbox.Backend.Sources {
    public class WeasylFeedSource : IFeedSource {
        private readonly IWeasylCredentials _token;

        public WeasylFeedSource(IWeasylCredentials token) {
            _token = token;
        }

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

        private static IEnumerable<FeedItem> Wrangle(IEnumerable<WeasylSubmission> submissions) {
            foreach (var s in submissions) {
                if (s.type != "submission")
                    throw new NotImplementedException();
                yield return new Artwork {
                    Author = new Author {
                        Username = s.owner,
                        ProfileUrl = $"https://www.weasyl.com/~{Uri.EscapeDataString(s.owner)}"
                    },
                    LinkUrl = s.link,
                    MatureContent = s.rating != "general",
                    Thumbnails = GetThumbnails(s),
                    Timestamp = s.posted_at,
                    Title = s.title
                };
            }
        }

        public async Task<FeedBatch> GetBatchAsync(string cursor) {
            var req = new WeasylFs.Endpoints.MessageSubmissions.Request();
            if (cursor != null)
                req.NextTime = DateTimeOffset.Parse(cursor);
            var page = await WeasylFs.Endpoints.MessageSubmissions.ExecuteAsync(_token, req);

            return new FeedBatch {
                Cursor = page.nexttime_or_null?.ToString("o"),
                HasMore = page.nexttime_or_null != null,
                FeedItems = Wrangle(page.submissions)
            };
        }

        public string GetNotificationsUrl() => "https://www.weasyl.com/messages/notifications#notifications";
        public string GetSubmitUrl() => "https://www.weasyl.com/submit";
    }
}