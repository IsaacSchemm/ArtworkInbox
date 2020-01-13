using ArtworkInbox.Backend.Types;
using DontPanic.TumblrSharp.Client;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class TumblrFeedSource : FeedSource {
        private readonly TumblrClient _client;

        public TumblrFeedSource(TumblrClient client) {
            _client = client;
        }

        public class UserInfoResponse {
            public User user;
        }

        public class User {
            public string name;
            public IEnumerable<Blog> blogs;
        }

        public class Blog {
            public IEnumerable<Avatar> avatar;
            public bool primary;
            public string url;
        }

        public class Avatar {
            public int width;
            public int height;
            public string url;
        }

        public override async Task<Author> GetAuthenticatedUserAsync() {
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

        private static IEnumerable<FeedItem> Wrangle(IEnumerable<BasePost> posts) {
            foreach (var p in posts) {
                var author = new Author {
                    Username = p.BlogName,
                    AvatarUrl = null,
                    ProfileUrl = $"https://{p.BlogName}.tumblr.com"
                };
                if (p is PhotoPost pp) {
                    foreach (var ph in pp.PhotoSet) {
                        yield return new Artwork {
                            Author = author,
                            LinkUrl = pp.Url,
                            Thumbnails = new[] { ph.OriginalSize }
                                .Concat(ph.AlternateSizes)
                                .Select(s => new Thumbnail {
                                    Url = s.ImageUrl,
                                    Width = s.Width,
                                    Height = s.Height
                                }),
                            Timestamp = pp.Timestamp,
                            Title = ""
                        };
                    }
                } else if (p is TextPost tp) {
                    yield return new StatusUpdate {
                        Author = author,
                        Html = tp.Body,
                        LinkUrl = tp.Url,
                        Timestamp = tp.Timestamp
                    };
                } else {
                    yield return new StatusUpdate {
                        Author = author,
                        Html = p.Summary,
                        LinkUrl = p.Url,
                        Timestamp = p.Timestamp
                    };
                }
            }
        }

        public override async Task<FeedBatch> GetBatchAsync(string cursor) {
            long sinceId = 0;
            if (cursor != null && long.TryParse(cursor, out long l))
                sinceId = l;
            //var ps = new DontPanic.TumblrSharp.MethodParameterSet {
            //    { "npf", "true" }
            //};
            //var ts = new CancellationTokenSource();
            //var page1 = await _client.CallApiMethodAsync<Newtonsoft.Json.Linq.JObject>(new DontPanic.TumblrSharp.ApiMethod($"https://api.tumblr.com/v2/user/dashboard", _client.OAuthToken, System.Net.Http.HttpMethod.Get, ps), ts.Token);
            //System.Diagnostics.Debug.WriteLine(page1);
            var page = await _client.GetDashboardPostsAsync(sinceId: sinceId);

            return new FeedBatch {
                Cursor = page.Any()
                    ? $"{page.Select(x => x.Id).Min()}"
                    : cursor,
                HasMore = page.Count() > 0,
                FeedItems = Wrangle(page)
            };
        }

        public override string GetNotificationsUrl() => "https://www.tumblr.com/inbox";
        public override string GetSubmitUrl() => "https://www.tumblr.com/dashboard";
    }
}
