using ArtworkInbox.Backend.Types;
using MapleFedNet.Common;
using MapleFedNet.Model;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class MastodonFeedSource : IFeedSource {
        private readonly IMastodonCredentials _token;

        public MastodonFeedSource(IMastodonCredentials token) {
            _token = token;
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            var user = await MapleFedNet.Api.Accounts.VerifyCredentials(_token);
            return new Author {
                Username = user.UserName,
                AvatarUrl = user.AvatarUrl,
                ProfileUrl = user.ProfileUrl
            };
        }

        private static IEnumerable<FeedItem> Wrangle(IEnumerable<Status> statuses) {
            foreach (var s in statuses) {
                var author = new Author {
                    Username = s.Account.UserName,
                    AvatarUrl = s.Account.AvatarUrl,
                    ProfileUrl = s.Account.ProfileUrl
                };
                var photos = s.MediaAttachments.Where(x => x.Type == "image");
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
        }

        public async Task<FeedBatch> GetBatchAsync(string cursor) {
            var page = await MapleFedNet.Api.Timelines.Home(_token, max_id: cursor ?? "", limit: 100);

            return new FeedBatch {
                Cursor = page.Any()
                    ? $"{page.Select(x => x.Id).Min()}"
                    : cursor,
                HasMore = page.Count() > 0,
                FeedItems = Wrangle(page)
            };
        }

        public string GetNotificationsUrl() => $"https://{_token.Domain}";
        public string GetSubmitUrl() => $"https://{_token.Domain}";
    }
}
