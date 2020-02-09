using ArtworkInbox.Backend.Types;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class InoreaderFeedSource : IFeedSource {
        private readonly InoreaderFs.Auth.Credentials _credentials;

        public InoreaderFeedSource(InoreaderFs.Auth.Credentials credentials) {
            _credentials = credentials;
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            try {
                var user = await InoreaderFs.Endpoints.UserInfo.ExecuteAsync(_credentials);
                return new Author {
                    Username = user.userName
                };
            } catch (Exception ex) when (ex.Message == "Client is rate-limited (too many 429 responses)") {
                throw new TooManyRequestsException();
            }
        }

        private static IEnumerable<string> GetImageUrls(string html) {
            var matches = System.Text.RegularExpressions.Regex.Matches(html, @"https?:\/\/[^'""]+\.(png|jpe?g|gif)");
            for (int i = 0; i < matches.Count; i++) {
                yield return matches[i].Value;
            }
        }

        private static IEnumerable<FeedItem> Wrangle(IEnumerable<InoreaderFs.Endpoints.StreamContents.Item> feedItems) {
            foreach (var f in feedItems) {
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
                    yield return new JournalEntry {
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

        public async Task<FeedBatch> GetBatchAsync(string cursor) {
            var page = await InoreaderFs.Endpoints.StreamContents.ExecuteAsync(_credentials, new InoreaderFs.Endpoints.StreamContents.Request {
                Continuation = cursor,
                Order = InoreaderFs.Endpoints.StreamContents.Order.NewestFirst,
                Number = 100
            });
            return new FeedBatch {
                Cursor = page.GetContinuation(),
                HasMore = page.GetContinuation() != null,
                FeedItems = Wrangle(page.items)
            };
        }

        public string GetNotificationsUrl() => null;
        public string GetSubmitUrl() => null;
    }
}
