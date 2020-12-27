using ArtworkInbox.Backend.Types;
using DeviantArtFs;
using DeviantArtFs.Extensions;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class DeviantArtFeedSource : IFeedSource {
        private readonly DeviantArtDeviationFeedSource _deviations;
        private readonly DeviantArtMessagesFeedSource _messages;
        private readonly DeviantArtPostFeedSource _posts;

        public DeviantArtFeedSource(IDeviantArtAccessToken token) {
            _deviations = new DeviantArtDeviationFeedSource(token);
            _messages = new DeviantArtMessagesFeedSource(token);
            _posts = new DeviantArtPostFeedSource(token);
        }

        public async Task<Author> GetAuthenticatedUserAsync() {
            return await _deviations.GetAuthenticatedUserAsync();
        }

        public record DeviantArtFeedCursor {
            public string d_cursor;
            public bool d_done;
            public string m_cursor;
            public bool m_done;
            public string p_cursor;
            public bool p_done;
            public string t;

            [JsonIgnore]
            public DateTimeOffset SkipUntilOlderThan {
                get {
                    return new DateTimeOffset(long.Parse(t), TimeSpan.Zero);
                }
                set {
                    t = value.UtcDateTime.Ticks.ToString();
                }
            }
        }

        public class BatchBeingProcessed {
            public string Name;
            public Stack<FeedItem> Stack;
            public DeviantArtFeedCursor NextCursor;

            public override string ToString() {
                return $"{Name} ({Stack.Count})";
            }
        }

        public async Task<FeedBatch> GetBatchAsync(string cursor) {
            DeviantArtFeedCursor fc = cursor == null
                ? new DeviantArtFeedCursor {
                    d_cursor = null,
                    d_done = false,
                    m_cursor = null,
                    m_done = false,
                    p_cursor = null,
                    p_done = false,
                    SkipUntilOlderThan = DateTimeOffset.MaxValue
                }
                : JsonConvert.DeserializeObject<DeviantArtFeedCursor>(cursor);
            var batches = new List<BatchBeingProcessed>();
            if (!fc.d_done) {
                var batch = await _deviations.GetBatchAsync(fc.d_cursor);
                batches.Add(new BatchBeingProcessed {
                    Name = "Deviations",
                    Stack = new Stack<FeedItem>(batch.FeedItems.OrderBy(x => x.Timestamp)),
                    NextCursor = fc with { d_cursor = batch.Cursor, d_done = !batch.HasMore }
                });
            }
            if (!fc.m_done) {
                var batch = await _messages.GetBatchAsync(fc.m_cursor);
                batches.Add(new BatchBeingProcessed {
                    Name = "Messages",
                    Stack = new Stack<FeedItem>(batch.FeedItems.OrderBy(x => x.Timestamp)),
                    NextCursor = fc with { m_cursor = batch.Cursor, m_done = !batch.HasMore }
                });
            }
            if (!fc.p_done) {
                var batch = await _posts.GetBatchAsync(fc.p_cursor);
                batches.Add(new BatchBeingProcessed {
                    Name = "Posts",
                    Stack = new Stack<FeedItem>(batch.FeedItems.OrderBy(x => x.Timestamp)),
                    NextCursor = fc with { p_cursor = batch.Cursor, p_done = !batch.HasMore }
                });
            }
            var items = new List<FeedItem>();
            System.Diagnostics.Debug.WriteLine("--------");
            while (batches.All(x => x.Stack.Any())) {
                System.Diagnostics.Debug.WriteLine(string.Join(", ", batches));
                var batch_with_newest_item = batches.OrderByDescending(x => x.Stack.Peek().Timestamp).First();
                var item = batch_with_newest_item.Stack.Pop();
                if (item.Timestamp < fc.SkipUntilOlderThan) {
                    items.Add(item);
                    if (!batch_with_newest_item.Stack.Any()) {
                        return new FeedBatch {
                            FeedItems = items,
                            Cursor = JsonConvert.SerializeObject(batch_with_newest_item.NextCursor with { SkipUntilOlderThan = item.Timestamp }),
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
