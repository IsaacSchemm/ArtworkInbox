using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtworkInbox.Inkbunny {
    public abstract class InkbunnySubmission {
        public int submission_id;
        public InkbunnyResponseBoolean hidden;
        public string username;
        public int user_id;
        public DateTimeOffset create_datetime;
        public string create_datetime_usertime;
        public DateTimeOffset last_file_update_datetime;
        public string last_file_update_datetime_usertime;
        public string file_name;
        public string thumbnail_url_medium;
        public string thumbnail_url_large;
        public string thumbnail_url_huge;
        public string thumbnail_url_medium_noncustom;
        public string thumbnail_url_large_noncustom;
        public string thumbnail_url_huge_noncustom;
		public string file_url_full;
		public string file_url_screen;
		public string file_url_preview;
		public string latest_file_name;
        public string latest_thumbnail_url_medium;
        public string latest_thumbnail_url_large;
        public string latest_thumbnail_url_huge;
        public string latest_thumbnail_url_medium_noncustom;
        public string latest_thumbnail_url_large_noncustom;
        public string latest_thumbnail_url_huge_noncustom;
        public string title;
        public InkbunnyResponseBoolean deleted;
        public InkbunnyResponseBoolean @public;
        public string mimetype;
        public int pagecount;
        public string latest_mimetype;
        public InkbunnyRating rating_id;
        public string rating_name;
		public int? thumb_huge_x;
		public int? thumb_huge_y;
		public int? thumb_large_x;
		public int? thumb_large_y;
		public int? thumb_medium_x;
		public int? thumb_medium_y;
		public int? thumb_huge_noncustom_x;
		public int? thumb_huge_noncustom_y;
		public int? thumb_large_noncustom_x;
		public int? thumb_large_noncustom_y;
		public int? thumb_medium_noncustom_x;
		public int? thumb_medium_noncustom_y;
		public int? latest_thumb_huge_x;
		public int? latest_thumb_huge_y;
		public int? latest_thumb_large_x;
		public int? latest_thumb_large_y;
		public int? latest_thumb_medium_x;
		public int? latest_thumb_medium_y;
		public int? latest_thumb_huge_noncustom_x;
		public int? latest_thumb_huge_noncustom_y;
		public int? latest_thumb_large_noncustom_x;
		public int? latest_thumb_large_noncustom_y;
		public int? latest_thumb_medium_noncustom_x;
		public int? latest_thumb_medium_noncustom_y;
		public InkbunnySubmissionType submission_type_id;
        public string type_name;
        public InkbunnyResponseBoolean digitalsales;
        public InkbunnyResponseBoolean printsales;
        public InkbunnyResponseBoolean friends_only;
        public InkbunnyResponseBoolean guest_block;
        public InkbunnyResponseBoolean scraps;
    }

	public class InkbunnySearchSubmission : InkbunnySubmission {
		public string unread_datetime;
		public string unread_datetime_usertime;
		public InkbunnyResponseBoolean updated;
		public int? stars;
	}

	public class InkbunnySubmissionDetail : InkbunnySubmission {
		public IEnumerable<InkbunnyFile> files;
		public IEnumerable<InkbunnyKeyword> keywords;
		// pools
		// prints
		public InkbunnyResponseBoolean favorite;
		public int favorites_count;
		public string user_icon_file_name;
		public string user_icon_url_large;
		public string user_icon_url_medium;
		public string user_icon_url_small;
		public string latest_file_url_full;
		public string latest_file_url_screen;
		public string latest_file_url_preview;
		public string description;
		public string description_bbcode_parsed;
		public string writing;
		public string writing_bbcode_parsed;
		public int pools_count;
		public IEnumerable<InkbunnyRatingTagInfo> ratings;
		public int comments_count;
		public int views;
		public string sales_description;
		public InkbunnyResponseBoolean forsale;
		public decimal? digital_price;
	}

	public class InkbunnyRatingTagInfo {
		public InkbunnyRatingTag content_tag_id;
		public string name;
		public string description;
		public InkbunnyRating rating_id;
	}

	public class InkbunnyKeyword {
		public int keyword_id;
		public string keyword_name;
		public InkbunnyResponseBoolean contributed;
		public int submissions_count;
	}

	public class InkbunnyFile {
		public int file_id;
		public string file_name;
		public string thumbnail_url_medium;
		public string thumbnail_url_large;
		public string thumbnail_url_huge;
		public string thumbnail_url_medium_noncustom;
		public string thumbnail_url_large_noncustom;
		public string thumbnail_url_huge_noncustom;
		public string file_url_full;
		public string file_url_screen;
		public string file_url_preview;
		public string mimetype;
		public int submission_id;
		public int user_id;
		public int submission_file_order;
		public int? full_size_x;
		public int? full_size_y;
		public int? screen_size_x;
		public int? screen_size_y;
		public int? preview_size_x;
		public int? preview_size_y;
		public int? thumb_huge_x;
		public int? thumb_huge_y;
		public int? thumb_large_x;
		public int? thumb_large_y;
		public int? thumb_medium_x;
		public int? thumb_medium_y;
		public int? thumb_huge_noncustom_x;
		public int? thumb_huge_noncustom_y;
		public int? thumb_large_noncustom_x;
		public int? thumb_large_noncustom_y;
		public int? thumb_medium_noncustom_x;
		public int? thumb_medium_noncustom_y;
		public string initial_file_md5;
		public string full_file_md5;
		public string large_file_md5;
		public string small_file_md5;
		public string thumbnail_file_md5;
		public string deleted;
		public string create_datetime;
		public string create_datetime_usertime;
	}
}
