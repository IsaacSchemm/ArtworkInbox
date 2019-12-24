using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DANotify.Data;
using DANotify.Models;
using DeviantArtFs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DANotify.Controllers {
    public class DeviantArtController : Controller {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IDeviantArtAuth _auth;

        public DeviantArtController(UserManager<IdentityUser> userManager, ApplicationDbContext context, ILogger<HomeController> logger, IDeviantArtAuth auth) {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _auth = auth;
        }

        public IActionResult Index() {
            return RedirectToAction(nameof(Feed));
        }

        public async Task<IActionResult> Feed(int take = 200) {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserDeviantArtTokens
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                return View(new DeviantArtFeedViewModel { });

            var token = new DeviantArtTokenWrapper(_auth, _context, dbToken);
            var items = await DeviantArtFs.Requests.Feed.FeedHome.ToArrayAsync(token, null, take + 1);
            return View(new DeviantArtFeedViewModel { Items = items.Take(take), More = items.Length > take });
        }
    }
}