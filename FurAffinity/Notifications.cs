using Microsoft.FSharp.Collections;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;

namespace ArtworkInbox.FurAffinity {
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "IDE1006:Naming Styles")]
    public class Notifications {
        public const string FAExportHost = "faexport.spangle.org.uk";

        private readonly HttpClient _client;

        public Notifications(IHttpClientFactory httpClientFactory, string fa_cookie) {
            _client = httpClientFactory.CreateClient();
            _client.BaseAddress = new Uri($"https://{FAExportHost}");
            _client.DefaultRequestHeaders.Add("User-Agent", "ArtworkInbox/0.1 (https://github.com/IsaacSchemm/ArtworkInbox)");
            _client.DefaultRequestHeaders.Add("FA_COOKIE", fa_cookie);
        }

        public record CurrentUser(
            string name,
            string profile,
            string profile_name);

        public record NewSubmission(
            int id,
            string title,
            string thumbnail,
            string link,
            string name,
            string profile,
            string profile_name);

        public record Submissions(
            CurrentUser current_user,
            FSharpList<NewSubmission> new_submissions);

        public Task<Submissions> GetSubmissionsAsync(bool sfw, int from) =>
            _client.GetFromJsonAsync<Submissions>($"/notifications/submissions.json?{(sfw ? "sfw=1" : "")}&from={from}");

        public record NotificationCounts(
            int submissions,
            int comments,
            int journals,
            int favorites,
            int watchers,
            int notes,
            int trouble_tickets);

        public record NewJournal(
            int journal_id,
            string title,
            string name,
            string profile,
            string profile_name,
            string posted,
            DateTimeOffset posted_at);

        public record Others(
            CurrentUser current_user,
            NotificationCounts notification_counts,
            FSharpList<NewJournal> new_journals);

        public Task<Others> GetOthersAsync() =>
            _client.GetFromJsonAsync<Others>("/notifications/others.json");
    }
}
