using ArtworkInbox.Backend.Types;
using DeviantArtFs;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class CompositeFeedSource : IFeedSource {
        public class SourceTracker {
            public IFeedSource Source { get; set; }
            public string Name { get; set; }
            public string LastCursor { get; set; }
            public FeedBatch LastBatch { get; set; }
        }

        private readonly SourceTracker[] _sourceTracker;

        public CompositeFeedSource(IEnumerable<IFeedSource> feedSources) {
            _sourceTracker = feedSources.Select((x, i) => new SourceTracker {
                Source = x,
                Name = $"s{i}",
                LastCursor = null,
                LastBatch = null
            }).ToArray();
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            return await _sourceTracker.First().Source.GetAuthenticatedUserAsync();
        }

        public class IndividualState {
            [JsonProperty("c")]
            public string Cursor { get; set; }
            [JsonProperty("m")]
            public bool HasMore { get; set; }
        }

        public class CombinedState {
            public Dictionary<string, IndividualState> s = new Dictionary<string, IndividualState>();

            public string t = $"{DateTimeOffset.MaxValue.Ticks}";

            public IndividualState GetState(string sourceTrackerName) {
                return s.TryGetValue(sourceTrackerName, out IndividualState state)
                    ? state
                    : new IndividualState { Cursor = null, HasMore = true };
            }

            public void SetState(string sourceTrackerName, IndividualState state) {
                s[sourceTrackerName] = state;
            }

            [JsonIgnore]
            public DateTimeOffset SkipUntilThisOld {
                get {
                    return new DateTimeOffset(long.Parse(t), TimeSpan.Zero);
                }
                set {
                    t = value.UtcDateTime.Ticks.ToString();
                }
            }
        }

        public class BatchBeingProcessed {
            public string SourceTrackerName;
            public Stack<FeedItem> Stack;
            public IndividualState NextCursor;

            public override string ToString() {
                return $"{SourceTrackerName} ({Stack.Count})";
            }
        }

        public async Task<FeedBatch> GetBatchAsync(string cursor) {
            CombinedState fc = cursor == null
                ? new CombinedState()
                : JsonConvert.DeserializeObject<CombinedState>(cursor);

            var batches = new List<BatchBeingProcessed>();
            foreach (var sourceTracker in _sourceTracker) {
                var individual_cursor = fc.GetState(sourceTracker.Name);
                if (individual_cursor.HasMore) {
                    FeedBatch batch;
                    if (sourceTracker.LastBatch != null && sourceTracker.LastCursor == individual_cursor.Cursor)
                        batch = sourceTracker.LastBatch;
                    else {
                        batch = await sourceTracker.Source.GetBatchAsync(individual_cursor.Cursor);
                        sourceTracker.LastBatch = batch;
                        sourceTracker.LastCursor = individual_cursor.Cursor;
                    }
                    batches.Add(new BatchBeingProcessed {
                        SourceTrackerName = sourceTracker.Name,
                        Stack = new Stack<FeedItem>(batch.FeedItems.OrderBy(x => x.Timestamp)),
                        NextCursor = new IndividualState {
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

                if (item.Timestamp <= fc.SkipUntilThisOld) {
                    items.Add(item);
                    if (!batch_with_newest_item.Stack.Any()) {
                        fc.SetState(batch_with_newest_item.SourceTrackerName, batch_with_newest_item.NextCursor);
                        fc.SkipUntilThisOld = item.Timestamp;
                        return new FeedBatch {
                            FeedItems = items,
                            Cursor = JsonConvert.SerializeObject(fc),
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

        public string GetNotificationsUrl() => "https://www.deviantart.com/notifications/feedback";
        public string GetSubmitUrl() => "https://www.deviantart.com/submit";
    }
}
