using ArtworkInbox.Backend.Types;
using ArtworkInbox.Data;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Atom;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml;

namespace ArtworkInbox.Backend.Sources {
    public class ExternalFeedSource : ISource {
        private readonly HttpClient _httpClient;
        private readonly UserExternalFeed _externalFeed;

        public ExternalFeedSource(IHttpClientFactory httpClientFactory, UserExternalFeed externalFeed) {
            _httpClient = httpClientFactory.CreateClient();
            _externalFeed = externalFeed;
        }

        public string Name => $"{_externalFeed.Type} feed {_externalFeed.Url}";

        public Task<Author> GetAuthenticatedUserAsync() => Task.FromResult<Author>(null);

        public string GetNotificationsUrl() => null;
        public string GetSubmitUrl() => null;

        public async IAsyncEnumerable<FeedItem> GetFeedItemsAsync() {
            string title = null;

            string xml = await _httpClient.GetStringAsync(_externalFeed.Url);
            using var tr = new StringReader(xml);
            using var xmlReader = XmlReader.Create(tr, new XmlReaderSettings() { Async = true });
            var feedReader = _externalFeed.Type == UserExternalFeed.FeedType.Atom
                ? new AtomFeedReader(xmlReader)
                : new RssFeedReader(xmlReader) as XmlFeedReader;
            while (await feedReader.Read()) {
                switch (feedReader.ElementType) {
                    // Read category
                    case SyndicationElementType.Category:
                        ISyndicationCategory category = await feedReader.ReadCategory();
                        break;

                    // Read Image
                    case SyndicationElementType.Image:
                        ISyndicationImage image = await feedReader.ReadImage();
                        break;

                    // Read Item
                    case SyndicationElementType.Item:
                        ISyndicationItem item = await feedReader.ReadItem();

                        DateTimeOffset ts = item.Published > item.LastUpdated ? item.Published : item.LastUpdated;
                        if (ts <= _externalFeed.LastRead)
                            yield break;

                        string summary = (item as IAtomEntry)?.Summary ?? item.Description;
                        var image_match = Regex.Match(summary, "img[^>]+src=['\"]([^'\"]+)['\"]");
                        yield return new Artwork {
                            Author = new Author {
                                Username = title ?? string.Join(" / ", item.Contributors.Select(x => x.Name ?? x.Email))
                            },
                            LinkUrl = item.Links.Select(x => x.Uri).Where(x => x != null).Select(x => x.AbsoluteUri).FirstOrDefault(),
                            Thumbnails = image_match.Success
                                ? new Thumbnail[] {
                                    new Thumbnail {
                                        Url = image_match.Groups[1].Value
                                    }
                                }
                                : Enumerable.Empty<Thumbnail>(),
                            Timestamp = ts,
                            Title = item.Title
                        };
                        break;

                    // Read link
                    case SyndicationElementType.Link:
                        ISyndicationLink link = await feedReader.ReadLink();
                        break;

                    // Read Person
                    case SyndicationElementType.Person:
                        ISyndicationPerson person = await feedReader.ReadPerson();
                        break;

                    // Read content
                    default:
                        ISyndicationContent content = await feedReader.ReadContent();
                        if (content.Name == "title")
                            title = content.Value;
                        break;
                }
            }

            yield break;
        }

        public Task<int?> GetNotificationCountAsync() => Task.FromResult<int?>(null);
    }
}
