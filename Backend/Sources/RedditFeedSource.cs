using ArtworkInbox.Backend.Types;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Reddit;
using Reddit.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class RedditFeedSource : IFeedSource {
        private readonly RedditClient _client;

        public RedditFeedSource(RedditClient client) {
            _client = client;
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            var response = _client.Account.GetMe();
            return new Author {
                AvatarUrl = response.IconImg,
                ProfileUrl = $"https://www.reddit.com/user/{Uri.EscapeDataString(response.Name)}/",
                Username = response.Name
            };
        }

        private static readonly Uri REDDIT = new Uri("https://www.reddit.com");

        private IEnumerable<FeedItem> Wrangle(IEnumerable<Post> posts) {
            foreach (var p in posts) {
                if (p is LinkPost l && l.Thumbnail != null) {
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
        }

        public async Task<FeedBatch> GetBatchAsync(string cursor) {
            long offset = 0;
            if (cursor != null && long.TryParse(cursor, out long l))
                offset = l;

            var posts = _client.Subreddit().Posts.GetNew(after: cursor ?? "");

            return new FeedBatch {
                Cursor = posts.Select(x => x.Fullname).DefaultIfEmpty(cursor).Last(),
                HasMore = posts.Any(),
                FeedItems = Wrangle(posts)
            };
        }

        public string GetNotificationsUrl() => "https://www.reddit.com/message/inbox/";
        public string GetSubmitUrl() => "https://www.reddit.com/";
    }
}
