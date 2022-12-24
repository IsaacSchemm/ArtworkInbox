using ArtworkInbox.Backend.Types;
using Mastonet;
using Mastonet.Entities;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class MastodonFeedSource : ISource {
        private readonly IMastodonClient _client;

        public bool IgnoreMedia { get; set; } = false;

        public string Name => _client.Instance;

        public MastodonFeedSource(IHttpClientFactory httpClientFactory, string host, string accessToken) {
            var client = httpClientFactory.CreateClient();
            client.DefaultRequestHeaders.Add("User-Agent", "ArtworkInbox/0.1 (https://github.com/IsaacSchemm/ArtworkInbox)");
            _client = new MastodonClient(host, accessToken, client);
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            var user = await _client.GetCurrentUser();
            return new Author {
                Username = user.UserName,
                AvatarUrl = user.AvatarUrl,
                ProfileUrl = user.ProfileUrl
            };
        }

        public string GetNotificationsUrl() => $"https://{_client.Instance}";
        public string GetSubmitUrl() => $"https://{_client.Instance}";

        public async IAsyncEnumerable<FeedItem> GetFeedItemsAsync() {
            string max_id = "";
            while (true) {
                var statuses = await _client.GetHomeTimeline(new ArrayOptions {
                    MaxId = max_id
                });
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
                var notifications = await _client.GetNotifications(new ArrayOptions {
                    MaxId = max_id
                });
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
