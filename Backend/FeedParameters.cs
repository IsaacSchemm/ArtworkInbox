using System;

namespace DANotify.Backend {
    public class FeedParameters<T> {
        public DateTimeOffset StartAt { get; set; } = DateTimeOffset.MinValue;
        public T Cursor { get; set; } = default;
        public int StopAtCount { get; set; } = 200;
        public TimeSpan StopAtTime { get; set; } = TimeSpan.FromSeconds(15);
    }
}
