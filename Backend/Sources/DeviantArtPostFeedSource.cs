using ArtworkInbox.Backend.Types;
using DeviantArtFs;
using DeviantArtFs.Extensions;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class DeviantArtPostFeedSource : ISource {
        private readonly IDeviantArtAccessToken _token;

        public bool IncludeJournals { get; set; } = true;
        public bool IncludeStatuses { get; set; } = true;

        public DeviantArtPostFeedSource(IDeviantArtAccessToken token) {
            _token = token;
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            try {
                var user = await DeviantArtFs.Api.User.AsyncWhoami(_token, DeviantArtObjectExpansion.None).StartAsTask();
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
            var asyncEnum = DeviantArtFs.Api.Messages.AsyncGetFeed(
                _token,
                new DeviantArtFs.Api.Messages.MessagesFeedRequest { Stack = false },
                Microsoft.FSharp.Core.FSharpOption<string>.None).ToAsyncEnumerable();
            await foreach (var n in asyncEnum) {
                if (n.comment.OrNull() is DeviantArtComment comment) {
                    var parent_author = n.subject.OrNull()?.comment?.OrNull()?.user
                        ?? n.subject.OrNull()?.status?.OrNull()?.author?.OrNull();
                    var status = n.subject.OrNull()?.status?.OrNull()
                        ?? n.status.OrNull();
                    var user = comment.user;
                    yield return new StatusUpdate {
                        Author = new Author {
                            Username = user.username,
                            AvatarUrl = user.usericon,
                            ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(user.username)}"
                        },
                        Html = (parent_author is DeviantArtUser a ? $"@{a.username} " : "") + comment.body,
                        LinkUrl = status?.url?.OrNull(),
                        Timestamp = comment.posted
                    };
                }
            }
        }

        public async IAsyncEnumerable<string> GetNotificationsAsync() {
            var asyncEnum = DeviantArtFs.Api.Messages.AsyncGetFeed(
                _token,
                new DeviantArtFs.Api.Messages.MessagesFeedRequest { Stack = false },
                Microsoft.FSharp.Core.FSharpOption<string>.None).ToAsyncEnumerable();
            await foreach (var n in asyncEnum) {
                yield return $"{n}";
            }
        }
    }
}
