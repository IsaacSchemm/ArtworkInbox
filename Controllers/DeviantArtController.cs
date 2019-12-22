using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DANotify.Models;
using DeviantArtFs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DANotify.Controllers {
    public class DeviantArtController : Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly IDeviantArtAuth _auth;

        public DeviantArtController(ILogger<HomeController> logger, IDeviantArtAuth auth) {
            _logger = logger;
            _auth = auth;
        }

        public IActionResult Index() {
            return RedirectToAction(nameof(Feed));
        }

        public async Task<IActionResult> Feed() {
            var authResult = await HttpContext.AuthenticateAsync();
            var token = new DeviantArtTokenWrapper(_auth, authResult.Properties);
            var items = await DeviantArtFs.Requests.Feed.FeedHome.ToArrayAsync(token, null, 50);
            return View(new DeviantArtFeedViewModel { Items = items });
        }
    }
}