using ArtworkInbox.Data;
using Reddit;
using System;

namespace ArtworkInbox {
    public class ArtworkInboxRedditClientFactory {
        private readonly string _appId, _appSecret;

        public ArtworkInboxRedditClientFactory(string appId, string appSecret) {
            _appId = appId ?? throw new ArgumentNullException(nameof(appId));
            _appSecret = appSecret ?? throw new ArgumentNullException(nameof(appSecret));
        }

        public RedditClient Create(UserRedditToken token)  {
            return new RedditClient(_appId, token.RefreshToken, _appSecret, token.AccessToken, "ArtworkInbox/0.0 (https://artworkinbox.azurewebsites.net)");
        }
    }
}
