using System;
using System.Collections.Generic;
using System.Linq;

namespace ArtworkInbox.Models {
    public class StatusPostedModel {
        public IEnumerable<Uri> Uris { get; set; } = Enumerable.Empty<Uri>();
        public IEnumerable<string> Errors { get; set; } = Enumerable.Empty<string>();
    }
}
