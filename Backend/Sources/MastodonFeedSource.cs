using ArtworkInbox.Backend.Types;
using Pleronet;
using Pleronet.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class MastodonFeedSource : ISource {
        private readonly IMastodonClient _token;

        public bool IgnoreMedia { get; set; } = false;

        public MastodonFeedSource(string host, string accessToken) {
            _token = new MastodonClient(
                new AppRegistration { Instance = host },
                new Auth { AccessToken = accessToken });
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            var user = await _token.GetCurrentUser();
            return new Author {
                Username = user.UserName,
                AvatarUrl = user.AvatarUrl,
                ProfileUrl = user.ProfileUrl
            };
        }

        public string GetNotificationsUrl() => $"https://{_token.Instance}";
        public string GetSubmitUrl() => $"https://{_token.Instance}";

        public async IAsyncEnumerable<FeedItem> GetFeedItemsAsync() {
            string max_id = "";
            while (true) {
                var statuses = await _token.GetHomeTimeline(max_id);
                if (!statuses.Any())
                    break;

                foreach (var s in statuses) {
                    var author = new Author {
                        Username = s.Account.UserName,
                        AvatarUrl = s.Account.AvatarUrl,
                        ProfileUrl = s.Account.ProfileUrl
                    };
                    var photos = IgnoreMedia
                        ? Enumerable.Empty<Attachment>()
                        : s.MediaAttachments.Where(x => x.Type == "image");
                    foreach (var media in photos) {
                        yield return new Artwork {
                            Author = author,
                            Timestamp = s.CreatedAt,
                            LinkUrl = s.Url,
                            Thumbnails = new[] {
                            new Thumbnail {
                                Url = media.PreviewUrl
                            }
                        },
                            RepostedFrom = s.Reblog?.Account?.UserName
                        };
                    }
                    if (!photos.Any()) {
                        yield return new StatusUpdate {
                            Author = author,
                            Timestamp = s.CreatedAt,
                            LinkUrl = s.Url,
                            Html = !string.IsNullOrEmpty(s.SpoilerText)
                                ? WebUtility.HtmlEncode(s.SpoilerText)
                                : s.Content,
                            RepostedFrom = s.Reblog?.Account?.UserName
                        };
                    }
                }
                max_id = statuses.NextPageMaxId;
            }
        }

        public async IAsyncEnumerable<string> GetNotificationsAsync() {
            string max_id = "";
            while (true) {
                var notifications = await _token.GetNotifications(max_id);
                if (!notifications.Any())
                    break;

                foreach (var n in notifications) {
                    yield return n.Type;
                }
                max_id = notifications.NextPageMaxId;
            }
        }
    }
}
