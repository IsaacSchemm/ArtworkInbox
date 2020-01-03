using System;

namespace ArtworkInbox.Models {
    public class ErrorViewModel {
        public string RequestId { get; set; }

        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
