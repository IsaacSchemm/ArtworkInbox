using ArtworkInbox.Backend.Types;
using ArtworkInbox.FurAffinity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class FurAffinityFeedSource : ISource {
        private readonly Notifications _handler;

        private Notifications.CurrentUser _user = null;

        public FurAffinityFeedSource(IHttpClientFactory factory, string fa_cookie) {
            _handler = new Notifications(factory, fa_cookie);
        }

        public string Name => "Fur Affinity";

        public async Task<Author> GetAuthenticatedUserAsync() {
            try {
                var ns = await _handler.GetOthersAsync();
                return new Author {
                    Username = ns.current_user.profile_name,
                    AvatarUrl = null,
                    ProfileUrl = ns.current_user.profile
                };
            } catch (Exception ex) when (ex.Message == "Client is rate-limited (too many 429 responses)") {
                throw new TooManyRequestsException();
            }
        }

        public async IAsyncEnumerable<FeedItem> GetFeedItemsAsync() {
            int from = int.MaxValue;
            while (true) {
                Notifications.Submissions sfw, nsfw;
                try {
                    sfw = await _handler.GetSubmissionsAsync(sfw: true, from: from);
                    nsfw = await _handler.GetSubmissionsAsync(sfw: false, from: from);
                } catch (Exception ex) when (ex.Message == "Client is rate-limited (too many 429 responses)") {
                    throw new TooManyRequestsException();
                }
                if (!nsfw.new_submissions.Any())
                    break;

                if (_user == null)
                    _user = sfw.current_user;

                from = nsfw.new_submissions.Select(x => x.id).DefaultIfEmpty(1).Min() - 1;

                foreach (var x in nsfw.new_submissions) {
                    yield return new Artwork {
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
                    };
                }
            }
        }

        public async IAsyncEnumerable<string> GetNotificationsAsync() {
            Notifications.Others ns;
            try {
                ns = await _handler.GetOthersAsync();
            } catch (Exception ex) when(ex.Message == "Client is rate-limited (too many 429 responses)") {
                throw new TooManyRequestsException();
            }

            if (_user == null)
                _user = ns.current_user;

            for (int i = 0; i < ns.notification_counts.comments; i++)
                yield return "comment";
            for (int i = 0; i < ns.notification_counts.journals; i++)
                yield return "journal";
            for (int i = 0; i < ns.notification_counts.favorites; i++)
                yield return "favorite";
            for (int i = 0; i < ns.notification_counts.watchers; i++)
                yield return "watcher";
            for (int i = 0; i < ns.notification_counts.notes; i++)
                yield return "note";
            for (int i = 0; i < ns.notification_counts.trouble_tickets; i++)
                yield return "trouble_ticket";
        }

        public string GetNotificationsUrl() => "https://www.furaffinity.net/msg/others/";
        public string GetSubmitUrl() => "https://www.furaffinity.net/submit/";
    }
}
