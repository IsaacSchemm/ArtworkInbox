using System;

namespace ArtworkInbox {
    public class ArtworkInboxRedditCredentials {
        public readonly string AppId, AppSecret;

        public ArtworkInboxRedditCredentials(string appId, string appSecret) {
            AppId = appId ?? throw new ArgumentNullException(nameof(appId));
            AppSecret = appSecret ?? throw new ArgumentNullException(nameof(appSecret));
        }
    }
}
