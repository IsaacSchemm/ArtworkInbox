using Newtonsoft.Json;
using System;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace ArtworkInbox.FurAffinity {
    public class Notifications {
        public record CurrentUser {
            public string name;
            public string profile;
            public string profile_name;
        }

        public record NewSubmission {
            public int id;
            public string title;
            public string thumbnail;
            public string link;
            public string name;
            public string profile;
            public string profile_name;
        }

        public record Submissions {
            public CurrentUser current_user;
            public ImmutableList<NewSubmission> new_submissions;
        }

        public static async Task<Submissions> GetSubmissionsAsync(string fa_cookie, bool sfw, int from) {
            var req = WebRequest.CreateHttp($"https://faexport.spangle.org.uk/notifications/submissions.json?{(sfw ? "sfw=1" : "")}&from={from}");
            req.UserAgent = "ArtworkInbox/0.1 (https://github.com/IsaacSchemm/ArtworkInbox)";
            req.Headers.Set("FA_COOKIE", fa_cookie);
            using var resp = await req.GetResponseAsync();
            using var stream = resp.GetResponseStream();
            using var sr = new StreamReader(stream);
            string json = await sr.ReadToEndAsync();
            return JsonConvert.DeserializeObject<Submissions>(json);
        }

        public record NotificationCounts {
            public int submissions;
            public int comments;
            public int journals;
            public int favorites;
            public int watchers;
            public int notes;
            public int trouble_tickets;

            public int Sum => new[] {
                submissions,
                comments,
                journals,
                favorites,
                watchers,
                notes,
                trouble_tickets
            }.Sum();
        }

        public record NewJournal {
            public int journal_id;
            public string title;
            public string name;
            public string profile;
            public string profile_name;
            public string posted;
            public DateTimeOffset posted_at;
        }

        public record Others {
            public CurrentUser current_user;
            public NotificationCounts notification_counts;
            public ImmutableList<NewJournal> new_journals;
        }

        public static async Task<Others> GetOthersAsync(string fa_cookie) {
            var req = WebRequest.CreateHttp($"https://faexport.spangle.org.uk/notifications/others.json");
            req.UserAgent = "ArtworkInbox/0.1 (https://github.com/IsaacSchemm/ArtworkInbox)";
            req.Headers.Set("FA_COOKIE", fa_cookie);
            using var resp = await req.GetResponseAsync();
            using var stream = resp.GetResponseStream();
            using var sr = new StreamReader(stream);
            string json = await sr.ReadToEndAsync();
            return JsonConvert.DeserializeObject<Others>(json);
        }
    }
}
