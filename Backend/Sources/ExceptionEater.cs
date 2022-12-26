using ArtworkInbox.Backend.Types;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class ExceptionEater : ISource {
        private readonly ISource _source;

        public ExceptionEater(ISource source) {
            _source = source;
        }

        public string Name => _source.Name;

        public Task<Author> GetAuthenticatedUserAsync() => _source.GetAuthenticatedUserAsync();

        public async IAsyncEnumerable<FeedItem> GetFeedItemsAsync() {
            var enumerator = _source.GetFeedItemsAsync().GetAsyncEnumerator();
            while (true) {
                FeedItem current;
                bool error;
                try {
                    current = await enumerator.MoveNextAsync()
                        ? enumerator.Current
                        : null;
                    error = false;
                } catch (Exception) {
                    current = null;
                    error = true;
                }

                if (current != null)
                    yield return current;

                if (error) {
                    yield return new StatusUpdate {
                        Author = new Author {
                            Username = "Artwork Inbox"
                        },
                        Html = $"An error occurred while reading from {_source.Name}.",
                        Timestamp = DateTimeOffset.UtcNow
                    };
                }

                if (current == null)
                    break;
            }
        }

        public Task<int?> GetNotificationCountAsync() => _source.GetNotificationCountAsync();

        public string GetNotificationsUrl() => _source.GetNotificationsUrl();

        public string GetSubmitUrl() => _source.GetSubmitUrl();
    }
}
