using DeviantArtFs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DANotify.Models {
    public class DeviantArtFeedViewModel {
        public DateTimeOffset Start { get; set; }
        public IEnumerable<IBclDeviantArtFeedItem> Items { get; set; }
        public string Cursor { get; set; }
        public bool More { get; set; }

        public IEnumerable<IBclDeviation> Deviations =>
            Items
            .Where(i => i.Type == "deviation_submitted")
            .SelectMany(i => i.Deviations);

        public IEnumerable<IGrouping<string, IBclDeviation>> DeviationsByUser =>
            Deviations
            .GroupBy(d => d.Author.Username)
            .OrderByDescending(g => g.Select(x => x.PublishedTime?.Ticks ?? 0L).Max());
    }
}
