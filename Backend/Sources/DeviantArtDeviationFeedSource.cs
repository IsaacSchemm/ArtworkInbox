using ArtworkInbox.Backend.Types;
using DeviantArtFs;
using DeviantArtFs.Extensions;
using DeviantArtFs.ParameterTypes;
using DeviantArtFs.ResponseTypes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class DeviantArtDeviationFeedSource : ISource {
        private readonly IDeviantArtAccessToken _token;

        public DeviantArtDeviationFeedSource(IDeviantArtAccessToken token) {
            _token = token;
        }

        public string Name => "DeviantArt (Deviations)";

        public async Task<Author> GetAuthenticatedUserAsync() {
            try {
                var user = await DeviantArtFs.Api.User.WhoamiAsync(_token);
                return new Author {
                    Username = user.username,
                    AvatarUrl = user.usericon,
                    ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(user.username)}"
                };
            } catch (Exception ex) when (ex.Message == "Client is rate-limited (too many 429 responses)") {
                throw new TooManyRequestsException();
            }
        }

        public string GetNotificationsUrl() => "https://www.deviantart.com/notifications/feedback";
        public string GetSubmitUrl() => "https://www.deviantart.com/submit";

        public async IAsyncEnumerable<FeedItem> GetFeedItemsAsync() {
            await foreach (var d in DeviantArtFs.Api.Browse.GetByDeviantsYouWatchAsync(_token, PagingLimit.MaximumPagingLimit, PagingOffset.StartingOffset)) {
                if (!d.is_deleted) {
                    yield return new Artwork {
                        Author = d.author.OrNull() is User a
                        ? new Author {
                            Username = a.username,
                            AvatarUrl = a.usericon,
                            ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(a.username)}"
                        } : new Author {
                            Username = "???",
                            AvatarUrl = null,
                            ProfileUrl = null
                        },
                        Timestamp = d.published_time.OrNull() ?? DateTimeOffset.UtcNow,
                        Title = d.title.OrNull() ?? "",
                        Thumbnails = d.thumbs.OrEmpty().Select(x => new Thumbnail {
                            Url = x.src,
                            Width = x.width,
                            Height = x.height
                        }),
                        LinkUrl = d.url.OrNull(),
                        MatureContent = d.is_mature.IsTrue()
                    };
                }
            }
        }

        public Task<int?> GetNotificationCountAsync() => Task.FromResult<int?>(null);
    }
}
