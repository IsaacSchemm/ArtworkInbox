using ArtworkInbox.Backend.Types;
using DontPanic.TumblrSharp.Client;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class TumblrFeedSource : ISource {
        private readonly TumblrClient _client;

        public TumblrFeedSource(TumblrClient client) {
            _client = client;
        }

        public string Name => "Tumblr";

        public class UserInfoResponse {
            public User user;
        }

        public class User {
            public string name;
            public IEnumerable<Blog> blogs;
        }

        public class Blog {
            public IEnumerable<Avatar> avatar;
            public string name;
            public bool primary;
            public string url;
        }

        public class Avatar {
            public int width;
            public int height;
            public string url;
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            var response = await _client.CallApiMethodAsync<UserInfoResponse>(
                new DontPanic.TumblrSharp.ApiMethod(
                    $"https://api.tumblr.com/v2/user/info",
                    _client.OAuthToken,
                    System.Net.Http.HttpMethod.Get),
                CancellationToken.None);
            return new Author {
                AvatarUrl = response.user.blogs
                    .Where(x => x.primary)
                    .SelectMany(x => x.avatar)
                    .OrderByDescending(x => x.width * x.height)
                    .Select(x => x.url)
                    .DefaultIfEmpty(null)
                    .First(),
                ProfileUrl = response.user.blogs
                    .Where(x => x.primary)
                    .Select(x => x.url)
                    .DefaultIfEmpty(null)
                    .First(),
                Username = response.user.name
            };
        }

        public class Dashboard {
            public IEnumerable<Post> posts;
        }

        public class Post {
            public string type; // = "blocks"
            public long id;
            public Blog blog;
            public string post_url;
            public long timestamp;
            public string summary;
            public IEnumerable<Content> content;
            public IEnumerable<Trail> trail;
        }

        public class Content {
            public string type;
            public JContainer media; // type = "image"
            public string text; // type = "text"

            public IEnumerable<Media> GetImages() =>
                type == "image"
                    ? JsonConvert.DeserializeObject<IEnumerable<Media>>(media.ToString())
                    : Enumerable.Empty<Media>();
        }

        public class Trail {
            public Blog blog;
            public IEnumerable<Content> content;
        }

        public class Media {
            public string type;
            public int width;
            public int height;
            public string url;
        }

        private static IEnumerable<Thumbnail> WrangleThumbnails(IEnumerable<Media> media) {
            return media.Select(x => new Thumbnail {
                Height = x.height,
                Width = x.width,
                Url = x.url
            });
        }

        private static IEnumerable<FeedItem> WranglePhotos(Post p) {
            var author = new Author {
                Username = p.blog.name,
                ProfileUrl = $"https://{p.blog.name}.tumblr.com"
            };

            foreach (var c in p.content) {
                if (c.type == "image") {
                    yield return new Artwork {
                        Author = author,
                        LinkUrl = p.post_url,
                        RepostedFrom = null,
                        Thumbnails = WrangleThumbnails(c.GetImages()),
                        Timestamp = DateTimeOffset.FromUnixTimeSeconds(p.timestamp),
                        Title = ""
                    };
                }
            }

            foreach (var t in p.trail) {
                foreach (var c in t.content) {
                    if (c.type == "image") {
                        yield return new Artwork {
                            Author = author,
                            LinkUrl = p.post_url,
                            RepostedFrom = t.blog.name,
                            Thumbnails = WrangleThumbnails(c.GetImages()),
                            Timestamp = DateTimeOffset.FromUnixTimeSeconds(p.timestamp),
                            Title = ""
                        };
                    }
                }
            }
        }

        public string GetNotificationsUrl() => "https://www.tumblr.com/inbox";
        public string GetSubmitUrl() => "https://www.tumblr.com/dashboard";

        public async IAsyncEnumerable<FeedItem> GetFeedItemsAsync() {
            long offset = 0;
            while (true) {
                var response = await _client.CallApiMethodAsync<Dashboard>(
                    new DontPanic.TumblrSharp.ApiMethod(
                        $"https://api.tumblr.com/v2/user/dashboard",
                        _client.OAuthToken,
                        System.Net.Http.HttpMethod.Get,
                        new DontPanic.TumblrSharp.MethodParameterSet {
                            { "npf", "true" },
                            { "offset", offset }
                        }),
                    CancellationToken.None);
                if (!response.posts.Any())
                    break;

                foreach (var p in response.posts) {
                    if (p.type != "blocks")
                        throw new NotImplementedException();

                    var photos = WranglePhotos(p);
                    foreach (var x in photos)
                        yield return x;

                    if (!photos.Any()) {
                        var author = new Author {
                            Username = p.blog.name,
                            ProfileUrl = $"https://{p.blog.name}.tumblr.com"
                        };
                        yield return new BlogPost {
                            Author = author,
                            Html = WebUtility.HtmlEncode(p.summary),
                            LinkUrl = p.post_url,
                            RepostedFrom = p.trail.Select(x => x.blog.name).DefaultIfEmpty(null).Last(),
                            Timestamp = DateTimeOffset.FromUnixTimeSeconds(p.timestamp)
                        };
                    }
                }

                offset += response.posts.Count();
            }
        }

        public Task<int?> GetNotificationCountAsync() => Task.FromResult<int?>(null);
    }
}
