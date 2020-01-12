using ArtworkInbox.Data;
using DontPanic.TumblrSharp;
using DontPanic.TumblrSharp.Client;
using DontPanic.TumblrSharp.OAuth;
using System;

namespace ArtworkInbox {
    public class ArtworkInboxTumblrClientFactory {
        private readonly ITumblrClientFactory _factory;
        private readonly string _consumerKey, _consumerSecret;

        public ArtworkInboxTumblrClientFactory(string consumerKey, string consumerSecret) {
            _factory = new TumblrClientFactory();
            _consumerKey = consumerKey ?? throw new ArgumentNullException(nameof(consumerKey));
            _consumerSecret = consumerSecret ?? throw new ArgumentNullException(nameof(consumerSecret));
        }

        public TumblrClient Create(UserTumblrToken token)  {
            return _factory.Create<TumblrClient>(_consumerKey, _consumerSecret, new Token(token.AccessToken, token.AccessTokenSecret));
        }
    }
}
