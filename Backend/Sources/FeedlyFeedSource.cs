using ArtworkInbox.Backend.Types;
using FeedlySharp;
using FeedlySharp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class FeedlyFeedSource : IFeedSource {
        private readonly FeedlySharpHttpClient _client;

        public FeedlyFeedSource(FeedlySharpHttpClient client) {
            _client = client;
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            var profile = await _client.GetProfile();
            return new Author {
                AvatarUrl = profile.Picture,
                Username = profile.FullName
            };
        }

        private static IEnumerable<FeedItem> Wrangle(IEnumerable<Entry> entries) {
            foreach (var e in entries) {
                if (e.Visual != null) {
                    yield return new Artwork {
                        Author = new Author {
                            Username = e.Origin?.Title ?? e.Author
                        },
                        LinkUrl = e.Alternate
                            .Where(x => x.Type == "text/html")
                            .Select(x => x.Href)
                            .DefaultIfEmpty(null)
                            .First(),
                        Thumbnails = new[] {
                            new Types.Thumbnail {
                                Url = e.Visual.Url,
                                Width = e.Visual.Width,
                                Height = e.Visual.Height
                            }
                        },
                        Timestamp = e.Published,
                        Title = e.Title
                    };
                } else {
                    yield return new JournalEntry {
                        Author = new Author {
                            Username = e.Origin?.Title ?? e.Author
                        },
                        Html = e.Summary?.Content ?? "",
                        LinkUrl = e.Alternate
                            .Where(x => x.Type == "text/html")
                            .Select(x => x.Href)
                            .DefaultIfEmpty(null)
                            .First(),
                        Timestamp = e.Published
                    };
                }
            }
        }

        public async Task<FeedBatch> GetBatchAsync(string cursor) {
            var stream = await _client.GetStream(new StreamOptions {
                StreamId = $"user/a9487394-85bc-47f0-b179-037d6af906aa/category/global.all",
                Continuation = cursor,
                UnreadOnly = true
            });

            return new FeedBatch {
                Cursor = stream.Continuation,
                HasMore = stream.Continuation != null,
                FeedItems = Wrangle(stream.Items)
            };
        }

        public string GetNotificationsUrl() => null;
        public string GetSubmitUrl() => null;
    }
}
