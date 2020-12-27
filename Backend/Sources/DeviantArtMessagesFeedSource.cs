using ArtworkInbox.Backend.Types;
using DeviantArtFs;
using DeviantArtFs.Extensions;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class DeviantArtMessagesFeedSource : IFeedSource {
        private readonly IDeviantArtAccessToken _token;

        public DeviantArtMessagesFeedSource(IDeviantArtAccessToken token) {
            _token = token;
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            try {
                var user = await DeviantArtFs.Api.User.Whoami.ExecuteAsync(_token, DeviantArtObjectExpansion.None);
                return new Author {
                    Username = user.username,
                    AvatarUrl = user.usericon,
                    ProfileUrl = $"https://www.deviantart.com/{Uri.EscapeDataString(user.username)}"
                };
            } catch (Exception ex) when (ex.Message == "Client is rate-limited (too many 429 responses)") {
                throw new TooManyRequestsException();
            }
        }

        private static string Summarize(DeviantArtComment c) {
            string body = c.body;
            return body.Length > 50
                ? body.Substring(0, 49) + "…"
                : body;
        }

        private static string Summarize(DeviantArtStatus ss) {
            string body = ss.body.OrNull() ?? $"{ss.statusid.OrNull()}";
            return body.Length > 40
                ? body.Substring(0, 39) + "…"
                : body;
        }

        private static IEnumerable<FeedItem> Wrangle(IEnumerable<DeviantArtMessage> messages) {
            foreach (var m in messages) {
                StringBuilder sb = new StringBuilder();
                if (m.originator.OrNull() is DeviantArtUser o)
                    sb.Append($"From {o.username}: ");
                sb.Append(m.type);
                if (m.subject.OrNull() is DeviantArtMessageSubject s) {
                    sb.Append(" (subject: ");
                    if (s.collection.OrNull() is DeviantArtCollectionFolder f)
                        sb.Append($"collection folder \"{f.name}\"");
                    if (s.comment.OrNull() is DeviantArtComment c)
                        sb.Append($"comment \"{Summarize(c)}\" by user {c.user.username}");
                    if (s.deviation.OrNull() is Deviation d)
                        sb.Append($"deviation \"{d.title.OrNull() ?? d.deviationid.ToString()}\"");
                    if (s.gallery.OrNull() is DeviantArtGalleryFolder g)
                        sb.Append($"gallery folder \"{g.name}\"");
                    if (s.profile.OrNull() is DeviantArtUser u)
                        sb.Append($"user {u.username}");
                    if (s.status.OrNull() is DeviantArtStatus ss)
                        sb.Append($"status \"{Summarize(ss)}\" by user {ss.author.OrNull()?.username}");
                    sb.Append(')');
                }
                yield return new CustomFeedItem {
                    Description = sb.ToString(),
                    Timestamp = m.ts.OrNull() ?? DateTimeOffset.UtcNow
                };
            }
        }

        public async Task<FeedBatch> GetBatchAsync(string cursor) {
            try {
                var page = await DeviantArtFs.Api.Messages.MessagesFeed.ExecuteAsync(_token, new DeviantArtFs.Api.Messages.MessagesFeedRequest { Stack = false }, cursor);
                return new FeedBatch {
                    Cursor = page.cursor,
                    HasMore = page.has_more,
                    FeedItems = Wrangle(page.results)
                };
            } catch (Exception ex) when (ex.Message == "Client is rate-limited (too many 429 responses)") {
                throw new TooManyRequestsException();
            }
        }

        public string GetNotificationsUrl() => "https://www.deviantart.com/notifications/feedback";
        public string GetSubmitUrl() => "https://www.deviantart.com/submit";
    }
}
