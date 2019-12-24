using DANotify.Data;
using DeviantArtFs;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DANotify {
    public class DeviantArtTokenWrapper : IDeviantArtAutomaticRefreshToken {
        private readonly ApplicationDbContext _context;
        private readonly UserDeviantArtToken _token;

        public IDeviantArtAuth DeviantArtAuth { get; }

        public DeviantArtTokenWrapper(IDeviantArtAuth auth, ApplicationDbContext context, UserDeviantArtToken token) {
            DeviantArtAuth = auth ?? throw new ArgumentNullException(nameof(auth));
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
