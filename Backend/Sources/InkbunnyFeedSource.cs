using ArtworkInbox.Backend.Types;
using ArtworkInbox.Inkbunny;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox.Backend.Sources {
    public class InkbunnyFeedSource : FeedSource {
        private readonly string _sid;

        public InkbunnyFeedSource(string sid) {
            _sid = sid;
        }

        public override async Task<Author> GetAuthenticatedUserAsync() {
            return new Author();
        }

        private static IEnumerable<Thumbnail> GetThumbnails(InkbunnySearchSubmission s) {
            if (s.thumbnail_url_huge_noncustom != null)
                yield return new Thumbnail {
                    Url = s.thumbnail_url_huge_noncustom,
                    Width = s.thumb_huge_noncustom_x ?? 0,
                    Height = s.thumb_huge_noncustom_y ?? 0
                };
            if (s.thumbnail_url_large_noncustom != null)
                yield return new Thumbnail {
                    Url = s.thumbnail_url_large_noncustom,
                    Width = s.thumb_large_noncustom_x ?? 0,
                    Height = s.thumb_large_noncustom_y ?? 0
                };
            if (s.thumbnail_url_medium_noncustom != null)
                yield return new Thumbnail {
                    Url = s.thumbnail_url_medium_noncustom,
                    Width = s.thumb_medium_noncustom_x ?? 0,
                    Height = s.thumb_medium_noncustom_y ?? 0
                };
        }

        private static IEnumerable<FeedItem> Wrangle(IEnumerable<InkbunnySearchSubmission> submissions) {
            foreach (var s in submissions) {
                yield return new Artwork {
                    Author = new Author {
                        Username = s.username,
                        ProfileUrl = $"https://inkbunny.net/{Uri.EscapeDataString(s.username)}"
                    },
                    LinkUrl = $"https://inkbunny.net/s/{s.submission_id}",
                    MatureContent = s.rating_id != InkbunnyRating.General,
                    Thumbnails = GetThumbnails(s),
                    Timestamp = s.last_file_update_datetime,
                    Title = s.title
                };
            }
        }

        public override async Task<FeedBatch> GetBatchAsync(string cursor) {
            var client = new InkbunnyClient(_sid);
            if (cursor == null) {
                var req = new InkbunnySearchParameters {
                    UnreadSubmissions = true
                };
                var results = await client.SearchAsync(req, get_rid: true);
                return new FeedBatch {
                    Cursor = results.page + "☺" + results.rid,
                    HasMore = results.page < results.pages_count,
                    FeedItems = Wrangle(results.submissions)
                };
            } else {
                string[] split = cursor.Split("☺");
                int last_page = int.Parse(split[0]);
                string rid = split[1];
                var results = await client.SearchAsync(rid, last_page + 1);
                return new FeedBatch {
                    Cursor = results.page + "☺" + results.rid,
                    HasMore = results.page < results.pages_count,
                    FeedItems = Wrangle(results.submissions)
                };
            }
        }

        public override string GetNotificationsUrl() => "https://inkbunny.net/portal.php";
        public override string GetSubmitUrl() => "https://inkbunny.net/filesedit.php?sales=no&wizardmode=yes";
    }
}
