﻿using ArtworkInbox.Backend.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class CompositeSource : ISource {
        public readonly IEnumerable<ISource> Sources;

        public CompositeSource(IEnumerable<ISource> sources) {
            Sources = sources;
        }

        public string Name => string.Join(", ", Sources.Select(x => x.Name));

        public async Task<Author> GetAuthenticatedUserAsync() {
            var authors = await Task.WhenAll(Sources.Select(x => x.GetAuthenticatedUserAsync()));
            return authors.Distinct().Count() == 1
                ? authors.First()
                : new Author();
        }

        public async IAsyncEnumerable<FeedItem> GetFeedItemsAsync() {
            var enumerators = Sources
                .Select(x => x.GetFeedItemsAsync().GetAsyncEnumerator())
                .ToList();
            foreach (var e in enumerators.ToList()) {
                bool result = await e.MoveNextAsync();
                if (!result)
                    enumerators.Remove(e);
            }
            while (enumerators.Any()) {
                var most_recent = enumerators.OrderByDescending(x => x.Current.Timestamp).First();
                yield return most_recent.Current;
                if (!await most_recent.MoveNextAsync())
                    enumerators.Remove(most_recent);
            }
        }

        public async Task<int?> GetNotificationCountAsync() {
            var counts = new List<int>();
            foreach (var s in Sources)
                if (await s.GetNotificationCountAsync() is int c)
                    counts.Add(c);
            if (counts.Any()) return counts.Sum();
            else return null;
        }

        public string GetNotificationsUrl() {
            var options = Sources.Select(x => x.GetNotificationsUrl()).Distinct();
            return options.Count() == 1 ? options.Single() : null;
        }

        public string GetSubmitUrl() {
            var options = Sources.Select(x => x.GetSubmitUrl()).Distinct();
            return options.Count() == 1 ? options.Single() : null;
        }
    }
}
