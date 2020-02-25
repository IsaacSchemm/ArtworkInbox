using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ArtworkInbox.Models;
using Microsoft.AspNetCore.Authentication;
using ArtworkInbox.Data;
using Microsoft.AspNetCore.Identity;
using DeviantArtFs;
using Microsoft.EntityFrameworkCore;
using ArtworkInbox.Backend.Sources;

namespace ArtworkInbox.Controllers {
    public class StatusController : Controller {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly DeviantArtApp _app;
        private readonly ArtworkInboxTumblrClientFactory _factory;

        public StatusController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger<HomeController> logger, DeviantArtApp app, ArtworkInboxTumblrClientFactory factory) {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _app = app;
            _factory = factory;
        }

        private async Task<IEnumerable<IPostDestination>> GetPostDestinationsAsync() {
            var postDestinations = new List<IPostDestination>();

            var userId = _userManager.GetUserId(User);
            var daTokens = await _context.UserDeviantArtTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
            foreach (var t in daTokens) {
                var token = new DeviantArtTokenWrapper(_app, _context, t);
                postDestinations.Add(new DeviantArtFeedSource(token));
            }

            var tumblrTokens = await _context.UserTumblrTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
            foreach (var t in tumblrTokens) {
                postDestinations.AddRange(await new TumblrFeedSource(_factory.Create(t)).GetPostDestinationsAsync());
            }

            return postDestinations;
        }

        public async Task<IActionResult> Index() {
            var postDestinations = await GetPostDestinationsAsync();

            return View(postDestinations);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Post([FromForm]string[] profileUrls, [FromForm]string statusText) {
            var postDestinations = await GetPostDestinationsAsync();

            var toUse = new List<IPostDestination>();
            foreach (var p in postDestinations) {
                try {
                    if (profileUrls.Contains(await p.GetProfileUrlAsync())) {
                        toUse.Add(p);
                    }
                } catch (Exception) { }
            }

            var uris = new List<Uri>();
            var errors = new List<string>();
            if (!string.IsNullOrWhiteSpace(statusText)) {
                foreach (var p in toUse) {
                    try {
                        Uri uri = await p.PostStatusAsync(statusText);
                        uris.Add(uri);
                    } catch (Exception ex) {
                        errors.Add($"Could not post to {await p.GetProfileUrlAsync()}: {ex.Message}");
                    }
                }
            }

            return Content(string.Join('\n', uris.Select(x => $"Posted to {x}").Concat(errors)));
        }
    }
}
