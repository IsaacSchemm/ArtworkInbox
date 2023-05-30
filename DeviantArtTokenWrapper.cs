using ArtworkInbox.Data;
using DeviantArtFs;
using System;
using System.Threading.Tasks;

namespace ArtworkInbox {
    public class DeviantArtTokenWrapper : IDeviantArtRefreshableAccessToken {
        private readonly ApplicationDbContext _context;
        private readonly UserDeviantArtToken _token;

        public DeviantArtApp App { get; }

        public DeviantArtTokenWrapper(DeviantArtApp app, ApplicationDbContext context, UserDeviantArtToken token) {
            App = app ?? throw new ArgumentNullException(nameof(app));
            _context = context;
            _token = token;
        }

        public string RefreshToken => _token.RefreshToken;

        public string AccessToken => _token.AccessToken;

        public async Task RefreshAccessTokenAsync() {
            var resp = await DeviantArtAuth.RefreshAsync(App, _token.RefreshToken);
            _token.RefreshToken = resp.refresh_token;
            _token.AccessToken = resp.access_token;
            await _context.SaveChangesAsync();
        }
    }
}
