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
        public bool AnyNotifications { get; set; }

        public IEnumerable<IBclDeviation> Deviations =>
            Items
            .Where(i => i.Type == "deviation_submitted")
            .SelectMany(i => i.Deviations);

        public IEnumerable<IGrouping<string, IBclDeviation>> DeviationsByUser =>
            Deviations
            .GroupBy(d => d.Author.Username)
            .OrderByDescending(g => g.Select(x => x.PublishedTime?.Ticks ?? 0L).Max());

        public IEnumerable<IBclDeviation> Journals =>
            Items
            .Where(x => x.Type == "journal_submitted")
            .SelectMany(x => x.Deviations);

        public IEnumerable<IGrouping<string, IBclDeviantArtStatus>> StatusesByUser =>
            Items
            .Where(x => x.Type == "status")
            .Select(i => i.Status)
            .GroupBy(s => s.Author.Username)
            .OrderByDescending(g => g.Select(x => x.Ts.Ticks).Max());

        public IEnumerable<IBclDeviantArtFeedItem> UsernameChanges =>
            Items
            .Where(x => x.Type == "username_change");

        public IEnumerable<IBclDeviantArtFeedItem> CollectionUpdates =>
            Items
            .Where(x => x.Type == "collection_update");
    }
}
