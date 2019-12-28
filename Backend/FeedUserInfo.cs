using DANotify.Backend.Types;

namespace DANotify.Backend {
    public class FeedUserInfo {
        public Author AuthenticatedUser { get; set; }
        public bool HasNotifications { get; set; }
        public string NotificationsUrl { get; set; }
    }
}
