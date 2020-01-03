using System;

namespace ArtworkInbox.Backend {
    public class TooManyRequestsException : Exception {
        public TooManyRequestsException() : base("Remote server reports too many requests") { }
    }
}
