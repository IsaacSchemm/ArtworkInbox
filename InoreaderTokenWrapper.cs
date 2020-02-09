using ArtworkInbox.Data;
using InoreaderFs.Auth;
using InoreaderFs.Auth.OAuth;
using System;
using System.Threading.Tasks;

namespace ArtworkInbox {
    public class InoreaderTokenWrapper : IAutoRefreshToken {
        private readonly ApplicationDbContext _context;
        private readonly UserInoreaderToken _token;

        public App App { get; }

        public InoreaderTokenWrapper(App app, ApplicationDbContext context, UserInoreaderToken token) {
            App = app ?? throw new ArgumentNullException(nameof(app));
            _context = context;
            _token = token;
        }

        public string RefreshToken => _token.RefreshToken;

        public string AccessToken => _token.AccessToken;

        public async Task UpdateTokenAsync(IRefreshToken value) {
            _token.RefreshToken = value.RefreshToken;
            _token.AccessToken = value.AccessToken;
            await _context.SaveChangesAsync();
        }
    }
}
