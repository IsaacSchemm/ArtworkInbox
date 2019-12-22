using DeviantArtFs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DANotify.Models {
    public class DeviantArtFeedViewModel {
        public IEnumerable<IBclDeviantArtFeedItem> Items { get; set; }
    }
}
