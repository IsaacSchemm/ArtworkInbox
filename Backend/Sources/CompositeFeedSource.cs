using ArtworkInbox.Backend.Types;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class CompositeFeedSource : IFeedSource {
        private class SourceTracker {
            public IFeedSource Source { get; init; }
            public string Name { get; init; }
            public string LastCursor { get; set; }
            public FeedBatch LastBatch { get; set; }
        }

        private class BatchBeingProcessed {
            public string SourceTrackerName { get; init; }
            public Stack<FeedItem> Stack { get; init; }
            public SerializableTrackerState NextTrackerState { get; init; }
        }

        private class SerializableTrackerState {
            [JsonProperty("c")]
            public string Cursor { get; set; }
            [JsonProperty("m")]
            public bool HasMore { get; set; }
        }

        private class SerializableState {
            [JsonProperty("s")]
            public Dictionary<string, SerializableTrackerState> TrackerStates = new Dictionary<string, SerializableTrackerState>();
            [JsonProperty("t")]
            public DateTimeOffset SkipUntilThisOld { get; set; } = DateTimeOffset.MaxValue;
        }

        private readonly SourceTracker[] _sourceTrackers;

        public IEnumerable<IFeedSource> Sources => _sourceTrackers.Select(t => t.Source);
        public IFeedSource PrimarySource => Sources.First();

        public bool EnableCache = true;

        public CompositeFeedSource(IEnumerable<IFeedSource> feedSources) {
            _sourceTrackers = feedSources
                .DefaultIfEmpty(new EmptyFeedSource())
                .Select((source, index) => new SourceTracker {
                    Source = source,
                    Name = $"s{index}",
                    LastBatch = null
                }).ToArray();
        }

        public Task<Author> GetAuthenticatedUserAsync() => PrimarySource.GetAuthenticatedUserAsync();

        public async Task<FeedBatch> GetBatchAsync(string cursor) {
            SerializableState combinedState = cursor == null
                ? new SerializableState()
                : JsonConvert.DeserializeObject<SerializableState>(cursor);

            var batches = new List<BatchBeingProcessed>();
            foreach (var sourceTracker in _sourceTrackers) {
                var trackerState = combinedState.TrackerStates.TryGetValue(sourceTracker.Name, out SerializableTrackerState t)
                    ? t
                    : new SerializableTrackerState { Cursor = null, HasMore = true };
                if (trackerState.HasMore) {
                    FeedBatch batch;
                    if (sourceTracker.LastBatch != null && sourceTracker.LastCursor == trackerState.Cursor)
                        batch = sourceTracker.LastBatch;
                    else {
                        batch = await sourceTracker.Source.GetBatchAsync(trackerState.Cursor);
                        if (EnableCache) {
                            sourceTracker.LastCursor = trackerState.Cursor;
                            sourceTracker.LastBatch = batch;
                        }
                    }
                    batches.Add(new BatchBeingProcessed {
                        SourceTrackerName = sourceTracker.Name,
                        Stack = new Stack<FeedItem>(batch.FeedItems.OrderBy(x => x.Timestamp)),
                        NextTrackerState = new SerializableTrackerState {
                            Cursor = batch.Cursor,
                            HasMore = batch.HasMore
                        }
                    });
                }
            }

            var items = new List<FeedItem>();
            while (batches.Any() && batches.All(x => x.Stack.Any())) {
                var batch_with_newest_item = batches
                    .OrderByDescending(x => x.Stack.Peek().Timestamp)
                    .First();
                var item = batch_with_newest_item.Stack.Pop();

                if (item.Timestamp <= combinedState.SkipUntilThisOld) {
                    items.Add(item);
                    if (!batch_with_newest_item.Stack.Any()) {
                        combinedState.TrackerStates[batch_with_newest_item.SourceTrackerName] = batch_with_newest_item.NextTrackerState;
                        combinedState.SkipUntilThisOld = item.Timestamp;
                        return new FeedBatch {
                            FeedItems = items,
                            Cursor = JsonConvert.SerializeObject(combinedState),
                            HasMore = true
                        };
                    }
                }
            }

            return new FeedBatch {
                FeedItems = items,
                Cursor = cursor,
                HasMore = false
            };
        }

        public string GetNotificationsUrl() => PrimarySource.GetNotificationsUrl();
        public string GetSubmitUrl() => PrimarySource.GetSubmitUrl();
    }
}
