using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ArtworkInbox.Data;
using Microsoft.AspNetCore.Identity;
using DeviantArtFs;
using Microsoft.EntityFrameworkCore;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Models;

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

        private async IAsyncEnumerable<IPostDestination> GetPostDestinationsAsync() {
            var userId = _userManager.GetUserId(User);
            var daTokens = await _context.UserDeviantArtTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
            foreach (var t in daTokens) {
                var token = new DeviantArtTokenWrapper(_app, _context, t);
                yield return new DeviantArtFeedSource(token);
            }

            var tumblrTokens = await _context.UserTumblrTokens
                .Where(t => t.UserId == userId)
                .ToListAsync();
            foreach (var t in tumblrTokens) {
                foreach (var p in await new TumblrFeedSource(_factory.Create(t)).GetPostDestinationsAsync()) {
                    yield return p;
                }
            }
        }

        private async IAsyncEnumerable<IPostDestination> GetSelectedPostDestinationsAsync(string[] profileUrls) {
            await foreach (var p in GetPostDestinationsAsync())
                if (profileUrls.Contains(await p.GetProfileUrlAsync()))
                    yield return p;
        }

        [HttpGet]
        public async Task<IActionResult> Index() {
            var postDestinations = new List<IPostDestination>();
            await foreach (var p in GetPostDestinationsAsync())
                postDestinations.Add(p);

            return View(postDestinations);
        }

        private async Task<StatusPostedModel> PostStatusAsync(IPostDestination p, string statusText) {
            try {
                Uri uri = await p.PostStatusAsync(statusText);
                return new StatusPostedModel { Uris = new[] { uri } };
            } catch (Exception ex) {
                return new StatusPostedModel { Errors = new[] { $"Could not post to {await p.GetProfileUrlAsync()}: {ex.Message}" } };
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Post([FromForm]string[] profileUrls, [FromForm]string statusText) {
            var postDestinations = new List<IPostDestination>();
            await foreach (var p in GetSelectedPostDestinationsAsync(profileUrls))
                postDestinations.Add(p);

            if (!string.IsNullOrWhiteSpace(statusText)) {
                var results = await Task.WhenAll(postDestinations.Select(x => PostStatusAsync(x, statusText)));
                return View(new StatusPostedModel {
                    Uris = results.SelectMany(x => x.Uris),
                    Errors = results.SelectMany(x => x.Errors)
                });
            } else {
                return RedirectToAction(nameof(Index));
            }
        }
    }
}
