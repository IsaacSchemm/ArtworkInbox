using ArtworkInbox.Backend.Types;
using Reddit;
using Reddit.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class RedditFeedSource : ISource {
        private readonly RedditClient _client;

        public RedditFeedSource(RedditClient client) {
            _client = client;
        }

        public string Name => "Reddit";

        public Task<Author> GetAuthenticatedUserAsync() {
            var response = _client.Account.GetMe();
            return Task.FromResult(new Author {
                AvatarUrl = response.IconImg,
                ProfileUrl = $"https://www.reddit.com/user/{Uri.EscapeDataString(response.Name)}/",
                Username = response.Name
            });
        }

        private static readonly Uri REDDIT = new Uri("https://www.reddit.com");

        public string GetNotificationsUrl() => "https://www.reddit.com/message/inbox/";
        public string GetSubmitUrl() => "https://www.reddit.com/";

        public IEnumerable<FeedItem> GetFeedItems() {
            string cursor = null;
            while (true) {
                var posts = _client.Subreddit().Posts.GetNew(after: cursor);
                if (!posts.Any())
                    break;

                foreach (var p in posts) {
                    if (p is LinkPost l && l.Thumbnail != null && l.Thumbnail != "default") {
                        yield return new Artwork {
                            Author = new Author { Username = $"/r/{p.Subreddit}" },
                            Thumbnails = new Thumbnail[] {
                            new Thumbnail {
                                Width = l.ThumbnailWidth ?? 0,
                                Height = l.ThumbnailHeight ?? 0,
                                Url = l.Thumbnail
                            }
                        },
                            LinkUrl = new Uri(REDDIT, p.Permalink).AbsoluteUri,
                            MatureContent = p.NSFW,
                            Timestamp = p.Created,
                            Title = p.Title
                        };
                    } else {
                        yield return new BlogPost {
                            Author = new Author { Username = $"/r/{p.Subreddit}" },
                            Html = p.Title,
                            LinkUrl = new Uri(REDDIT, p.Permalink).AbsoluteUri,
                            MatureContent = p.NSFW,
                            Timestamp = p.Created
                        };
                    }
                }

                cursor = posts.Select(x => x.Fullname).DefaultIfEmpty(cursor).Last();
            }
        }

        private IEnumerable<string> GetNotifications() {
            var unread = _client.Account.Messages.GetMessagesUnread(mark: false);
            foreach (var m in unread) {
                yield return m.Subject;
            }
        }

        IAsyncEnumerable<FeedItem> ISource.GetFeedItemsAsync() => GetFeedItems().ToAsyncEnumerable();

        Task<int?> ISource.GetNotificationCountAsync() => Task.FromResult<int?>(GetNotifications().Take(99).Count());
    }
}
