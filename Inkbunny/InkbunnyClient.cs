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
                using (StreamWriter sw = new StreamWriter(stream)) {
                    await sw.WriteAsync($"username={WebUtility.UrlEncode(username)}&password={WebUtility.UrlEncode(password)}");
                }
            }
            WebResponse response = await req.GetResponseAsync();
            using (Stream stream = response.GetResponseStream()) {
                using (StreamReader sr = new StreamReader(stream)) {
                    string json = await sr.ReadToEndAsync();
                    var loginResponse = JsonConvert.DeserializeObject<InkbunnyLoginResponse>(json);
                    if (loginResponse.error_code != null) {
                        throw new Exception(loginResponse.error_message);
                    }
                    return new InkbunnyClient(loginResponse.sid);
                }
            }
        }

        public async Task<long> UploadAsync(IEnumerable<byte[]> files = null) {
            string boundary = "----InkbunnyLib" + DateTime.Now.Ticks.ToString("x");

            var request = (HttpWebRequest)WebRequest.Create("https://inkbunny.net/api_upload.php");
            request.ContentType = "multipart/form-data; boundary=" + boundary;
            request.Method = "POST";

            using (var stream = await request.GetRequestStreamAsync()) {
                using (var sw = new StreamWriter(stream)) {
                    if (files != null) {
                        foreach (byte[] file in files) {
                            sw.WriteLine("--" + boundary);
                            sw.WriteLine("Content-Disposition: form-data; name=\"uploadedfile[]\"; filename=\"a.png\"");
                            sw.WriteLine();
                            sw.Flush();
                            stream.Write(file, 0, file.Length);
                            stream.Flush();
                            sw.WriteLine();
                        }
                    }

                    // The submission only seems to upload properly when I put this last...
                    sw.WriteLine("--" + boundary);
                    sw.WriteLine("Content-Disposition: form-data; name=\"sid\"");
                    sw.WriteLine();
                    sw.WriteLine(Sid);
                    sw.WriteLine("--" + boundary + "--");
                    sw.Flush();
                }
            }

            using (var response = await request.GetResponseAsync()) {
				using (var responseStream = response.GetResponseStream()) {
					using (var sr = new StreamReader(responseStream)) {
						InkbunnyUploadResponse r = JsonConvert.DeserializeObject<InkbunnyUploadResponse>(sr.ReadToEnd());
						if (r.error_code != null) {
							throw new Exception(r.error_message);
						}
						return r.submission_id;
					}
				}
			}
		}

        public async Task<InkbunnyEditSubmissionResponse> EditSubmissionAsync(
            long submission_id,
            string title = null,
            string desc = null,
            string story = null,
            bool convert_html_entities = false,
            InkbunnySubmissionType? type = null,
            bool? scraps = null,
            bool? use_twitter = null,
            InkbunnyTwitterImagePref? twitter_image_pref = null,
            bool? isPublic = null,
            bool notifyWatchersWhenPublic = false,
            IEnumerable<string> keywords = null,
			IEnumerable<InkbunnyRatingTag> tag = null,
            bool guest_block = false,
            bool friends_only = false
        ) {
            var dict = new Dictionary<string, string> {
                ["submission_id"] = submission_id.ToString(),
                ["title"] = title,
                ["desc"] = desc,
                ["story"] = story,
                ["convert_html_entities"] = convert_html_entities.ToYesNo(),
                ["type"] = type?.ToString("d"),
                ["scraps"] = scraps?.ToYesNo(),
                ["use_twitter"] = use_twitter?.ToYesNo(),
                ["twitter_image_pref"] = twitter_image_pref?.ToString("d"),
                ["visibility"] = isPublic == false ? "no"
                            : notifyWatchersWhenPublic ? "yes"
                            : "yes_nowatch",
                ["keywords"] = keywords == null
                            ? null
                            : string.Join(" ", keywords.Select(s => s.Replace(',', '_').Replace(' ', '_'))),
                ["friends_only"] = guest_block.ToYesNo(),
                ["friends_only"] = friends_only.ToYesNo(),
            };
            if (tag != null) {
                foreach (var t in tag) {
                    dict.Add($"tag[{t.ToString("d")}]", "yes");
                }
            }

            string json = await PostMultipartAsync("https://inkbunny.net/api_editsubmission.php", dict);
            return JsonConvert.DeserializeObject<InkbunnyEditSubmissionResponse>(json);
        }

        public async Task DeleteSubmissionAsync(int submission_id) {
            var dict = new Dictionary<string, string> {
                ["submission_id"] = submission_id.ToString()
            };

            await PostMultipartAsync("https://inkbunny.net/api_delsubmission.php", dict);
		}

		public async Task<InkbunnySubmissionDetail> GetSubmissionAsync(
			int submission_id,
			bool show_description = false,
			bool show_description_bbcode_parsed = false,
			bool show_writing = false,
			bool show_writing_bbcode_parsed = false
		) {
			var resp = await GetSubmissionsAsync(
				submission_ids: new[] { submission_id },
				show_description: show_description,
				show_description_bbcode_parsed: show_description_bbcode_parsed,
				show_writing: show_writing,
				show_writing_bbcode_parsed: show_writing_bbcode_parsed
			);
			return resp.submissions.First();
		}

		public async Task<InkbunnySubmissionDetailsResponse> GetSubmissionsAsync(
			IEnumerable<int> submission_ids,
			bool show_description = false,
			bool show_description_bbcode_parsed = false,
			bool show_writing = false,
			bool show_writing_bbcode_parsed = false
		) {
			var dict = new Dictionary<string, string> {
				["submission_ids"] = string.Join(",", submission_ids),
				["show_description"] = show_description.ToYesNo(),
				["show_description_bbcode_parsed"] = show_description_bbcode_parsed.ToYesNo(),
				["show_writing"] = show_writing.ToYesNo(),
				["show_writing_bbcode_parsed"] = show_writing_bbcode_parsed.ToYesNo(),
			};

			var json = await PostMultipartAsync("https://inkbunny.net/api_submissions.php", dict);
			return JsonConvert.DeserializeObject<InkbunnySubmissionDetailsResponse>(json);
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

            using (var response = await request.GetResponseAsync()) {
                using (var responseStream = response.GetResponseStream()) {
                    using (var sr = new StreamReader(responseStream)) {
                        string json = await sr.ReadToEndAsync();
                        var r = JsonConvert.DeserializeObject<InkbunnyResponse>(json);
                        if (r.error_code != null) {
                            throw new Exception(r.error_message);
                        }
                        return json;
                    }
                }
            }
        }

        private async Task<InkbunnySearchResponse> SearchAsync(Dictionary<string, string> parameters) {
            string json = await PostMultipartAsync("https://inkbunny.net/api_search.php", parameters);
            return JsonConvert.DeserializeObject<InkbunnySearchResponse>(json);
        }

		public async Task<InkbunnySearchSubmission> SearchFirstOrDefaultAsync(InkbunnySearchParameters searchParams) {
			var resp = await SearchAsync(searchParams, 1, false);
			return resp.submissions.FirstOrDefault();
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
