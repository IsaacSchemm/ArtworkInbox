using DeviantArtFs;
using Microsoft.AspNetCore.Authentication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DANotify {
    public class DeviantArtTokenWrapper : IDeviantArtAutomaticRefreshToken {
        private readonly AuthenticationProperties _authenticationProperties;

        public IDeviantArtAuth DeviantArtAuth { get; }

        public DeviantArtTokenWrapper(IDeviantArtAuth auth, AuthenticationProperties authenticationProperties) {
            DeviantArtAuth = auth ?? throw new ArgumentNullException(nameof(auth));
            _authenticationProperties = authenticationProperties ?? throw new ArgumentNullException(nameof(authenticationProperties));
        }

        public string RefreshToken => _authenticationProperties.GetTokenValue("refresh_token");

        public string AccessToken => _authenticationProperties.GetTokenValue("access_token");

        public Task UpdateTokenAsync(IDeviantArtRefreshToken value) {
            _authenticationProperties.UpdateTokenValue("refresh_token", value.RefreshToken);
            _authenticationProperties.UpdateTokenValue("access_token", value.AccessToken);
            return Task.CompletedTask;
        }
    }
}
