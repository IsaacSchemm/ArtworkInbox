using System;

namespace DANotify.Backend {
    public class FeedParameters {
        public DateTimeOffset StartAt { get; set; } = DateTimeOffset.MinValue;
        public string Cursor { get; set; } = null;
        public int StopAtCount { get; set; } = 200;
        public TimeSpan StopAtTime { get; set; } = TimeSpan.FromSeconds(15);
    }
}
