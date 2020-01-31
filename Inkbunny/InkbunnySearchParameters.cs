using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ArtworkInbox.Inkbunny {
	public class InkbunnySearchParameters {
		public enum JoinType { or, and }
		public enum SalesStatus { forsale, digital, prints }
		public enum Order {
			create_datetime,
			last_file_update_datetime,
			unread_datetime,
			unread_datetime_reverse,
			views,
			total_print_sales,
			total_digital_sales,
			total_sales,
			username,
			fav_datetime,
			fav_stars,
			pool_order
		}
		public enum ScrapsOption { both, no, only }

		public JoinType FieldJoinType = JoinType.or;
		public string Text;
		public JoinType StringJoinType = JoinType.and;
		public bool Keywords = true;
		public bool Title = false;
		public bool Description = false;
		public bool MD5 = false;
		public int? KeywordId;
		public string Username;
		public int? UserId;
		public int? FavsUserId;
		public bool UnreadSubmissions;
		public InkbunnySubmissionType? Type;
		public SalesStatus? Sales;
		public int? PoolId;
		public Order OrderBy = Order.create_datetime;
		public int? DaysLimit;
		public bool Random;
		public ScrapsOption Scraps = ScrapsOption.both;
		public int CountLimit = 50000;

		internal Dictionary<string, string> ToPostParams() {
			return new Dictionary<string, string> {
				["field_join_type"] = FieldJoinType.ToString("g"),
				["text" ] = Text,
				["string_join_type"] = StringJoinType.ToString("g"),
				["keywords"] = Keywords.ToYesNo(),
				["title"] = Title.ToYesNo(),
				["description"] = Description.ToYesNo(),
				["md5"] = MD5.ToYesNo(),
				["keyword_id"] = KeywordId?.ToString(),
				["username"] = Username,
				["user_id"] = UserId?.ToString(),
				["favs_user_id"] = FavsUserId?.ToString(),
				["unread_submissions"] = UnreadSubmissions.ToYesNo(),
				["type"] = Type?.ToString("d"),
				["sales"] = Sales?.ToString("g"),
				["pool_id"] = PoolId?.ToString(),
				["orderby"] = OrderBy.ToString("g"),
				["dayslimit"] = DaysLimit?.ToString(),
				["random"] = Random.ToYesNo(),
				["scraps"] = Scraps.ToString("g"),
				["count_limit"] = CountLimit.ToString()
			};
		}
	}
}
