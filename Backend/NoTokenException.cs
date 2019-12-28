using System;

namespace DANotify.Backend {
    public class NoTokenException : Exception {
        public NoTokenException() : base("The user is not logged into this site") { }
    }
}
