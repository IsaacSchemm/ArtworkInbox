using ArtworkInbox.Data;
using DeviantArtFs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ArtworkInbox {
    public class DeviantArtTokenWrapper : IDeviantArtAutomaticRefreshToken {
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

        public async Task UpdateTokenAsync(IDeviantArtRefreshToken value) {
            _token.RefreshToken = value.RefreshToken;
            _token.AccessToken = value.AccessToken;
            await _context.SaveChangesAsync();
        }
    }
}
