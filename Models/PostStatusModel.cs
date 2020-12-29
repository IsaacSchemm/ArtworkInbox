using System.Collections.Generic;

namespace ArtworkInbox.Models {
    public class PostStatusModel {
        public class Site {
            public string Host { get; set; }
            public bool Checked { get; set; }
        }

        public List<Site> Sites { get; set; }
        public string Text { get; set; }
    }
}
