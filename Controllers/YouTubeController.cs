using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ArtworkInbox.Backend;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Backend.Types;
using ArtworkInbox.Data;
using DeviantArtFs;
using DeviantArtFs.Extensions;
using Google.Apis.Auth.OAuth2;
using Google.Apis.Auth.OAuth2.Flows;
using Google.Apis.Auth.OAuth2.Responses;
using Google.Apis.Services;
using Google.Apis.Util.Store;
using Google.Apis.YouTube.v3;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace ArtworkInbox.Controllers {
    public class YouTubeController : SourceController {
        private readonly ClientSecrets _clientSecrets;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public YouTubeController(UserManager<ApplicationUser> userManager, IMemoryCache cache, ClientSecrets clientSecrets, ApplicationDbContext context, ILogger<HomeController> logger) : base(userManager, cache) {
            _clientSecrets = clientSecrets;
            _context = context;
            _logger = logger;
        }

        protected override string SiteName => "YouTube";

        protected override async Task<ISource> GetSourceAsync() {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserYouTubeTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync() ?? throw new NoTokenException();

            var flow = new GoogleAuthorizationCodeFlow(new GoogleAuthorizationCodeFlow.Initializer {
                ClientSecrets = _clientSecrets,
                Scopes = new[] { YouTubeService.Scope.YoutubeReadonly },
                DataStore = _context
            });

            var token = new TokenResponse {
                AccessToken = dbToken.AccessToken,
                RefreshToken = dbToken.RefreshToken
            };

            var credential = new UserCredential(flow, userId, token);

            var service = new YouTubeService(new BaseClientService.Initializer() {
                HttpClientInitializer = credential,
                ApplicationName = "Artwork Inbox"
            });

            return new YouTubeSubscriptionFeedSource(service);
        }

        protected override async Task<DateTimeOffset> GetLastReadAsync() {
            var userId = _userManager.GetUserId(User);
            var dt = await _context.UserYouTubeTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .Select(t => t.LastRead)
                .SingleOrDefaultAsync();
            return dt ?? DateTimeOffset.MinValue;
        }

        protected override async Task SetLastReadAsync(DateTimeOffset lastRead) {
            var userId = _userManager.GetUserId(User);
            var o = await _context.UserYouTubeTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .SingleAsync();

            o.LastRead = lastRead;
            await _context.SaveChangesAsync();
        }
    }
}