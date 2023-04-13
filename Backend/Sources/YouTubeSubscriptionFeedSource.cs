using ArtworkInbox.Backend.Types;
using Google.Apis.YouTube.v3;
using Google.Apis.YouTube.v3.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class YouTubeSubscriptionFeedSource : ISource {
        private readonly YouTubeService _service;

        public YouTubeSubscriptionFeedSource(YouTubeService service) {
            _service = service;
        }

        public string Name => "YouTube";

        public async Task<Author> GetAuthenticatedUserAsync() {
            var request1 = _service.Channels.List("id,snippet");
            request1.Mine = true;
            var response1 = await request1.ExecuteAsync();
            var channel = response1.Items.Single();

            return new Author {
                Username = channel.Snippet.Title,
                AvatarUrl = channel.Snippet.Thumbnails.Default__.Url,
                ProfileUrl = $"https://www.youtube.com/channel/{Uri.EscapeDataString(channel.Id)}"
            };
        }

        public async IAsyncEnumerable<Subscription> GetSubscriptionsAsync() {
            var request1 = _service.Channels.List("id");
            request1.Mine = true;
            var response1 = await request1.ExecuteAsync();

            var request = _service.Subscriptions.List("snippet");
            request.ChannelId = response1.Items.Single().Id;
            while (true) {
                var response = await request.ExecuteAsync();
                foreach (var item in response.Items) {
                    yield return item;
                }
                if (response.NextPageToken == null)
                    break;
                request.PageToken = response.NextPageToken;
            }
        }

        public async IAsyncEnumerable<FeedItem> GetFeedItemsAsync() {
            await foreach (var x in GetSubscriptionsAsync()) {
                var request = _service.Playlists.List("id");
                request.ChannelId = x.Snippet.ChannelId;
                var response = await request.ExecuteAsync();
                foreach (var y in response.Items) {
                    yield return new BlogPost {
                        Author = new Author {
                            Username = x.Id
                        },
                        Html = y.Id,
                        Timestamp = DateTimeOffset.UtcNow
                    };
                }
            }
        }

        public string GetNotificationsUrl() => null;
        public string GetSubmitUrl() => null;

        public async Task<int?> GetNotificationCountAsync() {
            return null;
        }
    }
}