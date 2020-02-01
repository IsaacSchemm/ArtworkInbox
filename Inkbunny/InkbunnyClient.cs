using ArtworkInbox.Inkbunny.Responses;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ArtworkInbox.Inkbunny {
	public class InkbunnyClient {
		public string Sid { get; private set; }

		public InkbunnyClient(string sid) {
            Sid = sid;
        }

        public static async Task<InkbunnyClient> CreateAsync(string username, string password) {
            HttpWebRequest req = WebRequest.CreateHttp("https://inkbunny.net/api_login.php");
            req.Method = "POST";
            req.ContentType = "application/x-www-form-urlencoded";
            req.UserAgent = "InkbunnyLib/1.2";

            using (Stream stream = await req.GetRequestStreamAsync()) {
                using StreamWriter sw = new StreamWriter(stream);
                await sw.WriteAsync($"username={WebUtility.UrlEncode(username)}&password={WebUtility.UrlEncode(password)}");
            }

            using WebResponse response = await req.GetResponseAsync();
            using Stream respStream = response.GetResponseStream();
            using StreamReader sr = new StreamReader(respStream);
            string json = await sr.ReadToEndAsync();
            var loginResponse = JsonConvert.DeserializeObject<InkbunnyLoginResponse>(json);
            if (loginResponse.error_code != null) {
                throw new InkbunnyException(loginResponse);
            }
            return new InkbunnyClient(loginResponse.sid);
        }

		private async Task<string> PostMultipartAsync(string url, Dictionary<string, string> parameters) {
            string boundary = "----InkbunnyLib" + DateTime.Now.Ticks.ToString("x");

            var request = WebRequest.Create(url);
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";

            if (!parameters.ContainsKey("sid")) {
                parameters.Add("sid", Sid);
            }

            using (var sw = new StreamWriter(await request.GetRequestStreamAsync())) {
                foreach (var pair in parameters) {
                    if (pair.Key.Contains("\"")) throw new Exception("Cannot include quotation marks in key name");
                    if (pair.Value == null) continue;
                    await sw.WriteLineAsync("--" + boundary);
                    await sw.WriteLineAsync($"Content-Disposition: form-data; name=\"{pair.Key}\"");
                    await sw.WriteLineAsync();
                    await sw.WriteLineAsync(pair.Value.ToString());
                }
                await sw.WriteLineAsync("--" + boundary + "--");
                await sw.FlushAsync();
            }

            using var response = await request.GetResponseAsync();
            using var responseStream = response.GetResponseStream();
            using var sr = new StreamReader(responseStream);
            string json = await sr.ReadToEndAsync();
            var r = JsonConvert.DeserializeObject<InkbunnyResponse>(json);
            if (r.error_code != null) {
                throw new InkbunnyException(r);
            }
            return json;
        }

        private async Task<InkbunnySearchResponse> SearchAsync(Dictionary<string, string> parameters) {
            string json = await PostMultipartAsync("https://inkbunny.net/api_search.php", parameters);
            return JsonConvert.DeserializeObject<InkbunnySearchResponse>(json);
        }

		public Task<InkbunnySearchResponse> SearchAsync(InkbunnySearchParameters searchParams = null, int? submissions_per_page = null, bool get_rid = true) {
			var dict = (searchParams ?? new InkbunnySearchParameters()).ToPostParams();
			dict.Add("submissions_per_page", submissions_per_page?.ToString());
			if (get_rid) {
				dict.Add("get_rid", "yes");
			}
			return SearchAsync(dict);
        }

        public Task<InkbunnySearchResponse> SearchAsync(string rid, int page, int? submissions_per_page = null) {
            if (rid == null) {
                throw new ArgumentNullException(nameof(rid));
            }
            return SearchAsync(new Dictionary<string, string> {
                ["rid"] = rid,
                ["submissions_per_page"] = submissions_per_page?.ToString(),
                ["page"] = page.ToString()
            });
        }

		public Task LogoutAsync() {
			return PostMultipartAsync("https://inkbunny.net/api_logout.php", new Dictionary<string, string>());
		}
    }
}
