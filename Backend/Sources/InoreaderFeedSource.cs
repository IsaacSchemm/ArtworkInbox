using ArtworkInbox.Backend.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class InoreaderFeedSource : ISource {
        private readonly InoreaderFs.Auth.Credentials _credentials;
        private readonly bool _allAsText;
        private readonly bool _unreadOnly;

        public string Name => "Inoreader";

        public InoreaderFeedSource(InoreaderFs.Auth.Credentials credentials, bool allAsText = false, bool unreadOnly = false) {
            _credentials = credentials;
            _allAsText = allAsText;
            _unreadOnly = unreadOnly;
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            try {
                var user = await InoreaderFs.Endpoints.UserInfo.ExecuteAsync(_credentials);
                return new Author {
                    Username = user.userName
                };
            } catch (WebException ex) when ((ex.Response as HttpWebResponse)?.StatusCode == HttpStatusCode.TooManyRequests) {
                throw new TooManyRequestsException();
            }
        }

        private static readonly Regex IMAGE_URL = new Regex(@"https?:\/\/[^'""]+\.(png|jpe?g|gif)");

        private IEnumerable<string> GetImageUrls(string html) {
            if (_allAsText)
                yield break;

            var matches = IMAGE_URL.Matches(html);
            for (int i = 0; i < matches.Count; i++) {
                yield return matches[i].Value;
            }
        }

        public string GetNotificationsUrl() => null;
        public string GetSubmitUrl() => null;

        public async IAsyncEnumerable<FeedItem> GetFeedItemsAsync() {
            string continuation = null;
            while (true) {
                var page = await InoreaderFs.Endpoints.StreamContents.ExecuteAsync(_credentials, new InoreaderFs.Endpoints.StreamContents.Request {
                    Continuation = continuation,
                    ExcludeRead = _unreadOnly,
                    Order = InoreaderFs.Endpoints.StreamContents.Order.NewestFirst,
                    Number = 100
                });

                continuation = page.GetContinuation();
                if (continuation == null)
                    break;

                foreach (var f in page.items) {
                    var images = GetImageUrls(f.summary.content);
                    if (images.Any()) {
                        foreach (var i in images) {
                            yield return new Artwork {
                                Author = new Author {
                                    Username = f.origin.title
                                },
                                LinkUrl = f.canonical.Select(x => x.href).FirstOrDefault(),
                                Thumbnails = new[] {
                                new Thumbnail {
                                    Url = i
                                }
                            },
                                Timestamp = f.GetTimestamp(),
                                Title = f.title
                            };
                        }
                    } else {
                        yield return new BlogPost {
                            Author = new Author {
                                Username = f.origin.title
                            },
                            Html = WebUtility.HtmlEncode(f.title),
                            LinkUrl = f.canonical.Select(x => x.href).FirstOrDefault(),
                            Timestamp = f.GetTimestamp()
                        };
                    }
                }
            }
        }

        public IAsyncEnumerable<string> GetNotificationsAsync() => AsyncEnumerable.Empty<string>();
    }
}
