using System;

namespace ArtworkInbox.Backend {
    public class NoTokenException : Exception {
        public NoTokenException() : base("The user is not logged into this site") { }
    }
}
