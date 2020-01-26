using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ArtworkInbox.Backend;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Data;
using ArtworkInbox.Inkbunny;
using ArtworkInbox.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tweetinvi.Models;

namespace ArtworkInbox.Controllers {
    public class InkbunnyController : FeedController {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public InkbunnyController(UserManager<ApplicationUser> userManager, ApplicationDbContext context) {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult Index() {
            return RedirectToAction(nameof(Feed));
        }

        protected override Task<ApplicationUser> GetUserAsync() =>
            _userManager.GetUserAsync(User);

        protected override string GetSiteName() => "Inkbunny";

        protected override async Task<FeedSource> GetFeedSourceAsync() {
            var user = await _userManager.GetUserAsync(User);
            if (string.IsNullOrEmpty(user.InkbunnySessionId))
                throw new NoTokenException();
            return new InkbunnyFeedSource(user.InkbunnySessionId);
        }

        protected override async Task<DateTimeOffset> GetLastRead() {
            var userId = _userManager.GetUserId(User);
            var dt = await _context.UserReadMarkers
                .Where(t => t.UserId == userId)
                .Select(t => t.InkbunnyLastRead)
                .SingleOrDefaultAsync();
            return dt ?? DateTimeOffset.MinValue;

        }

        protected override async Task SetLastRead(DateTimeOffset lastRead) {
            var userId = _userManager.GetUserId(User);
            var o = await _context.UserReadMarkers
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();

            if (o == null) {
                o = new UserReadMarker {
                    UserId = userId
                };
                _context.UserReadMarkers.Add(o);
            }

            o.InkbunnyLastRead = lastRead;
            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public IActionResult Login() {
            return View(new InkbunnyLoginViewModel());
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(InkbunnyLoginViewModel model) {
            var c = await InkbunnyClient.CreateAsync(model.Username, model.Password);

            var user = await _userManager.GetUserAsync(User);
            user.InkbunnySessionId = c.Sid;
            await _userManager.UpdateAsync(user);

            return LocalRedirect("/Identity/Account/Manage");
        }

        [HttpGet]
        public IActionResult Logout() {
            return View();
        }

        [HttpPost, ValidateAntiForgeryToken, ActionName("Logout")]
        public async Task<IActionResult> LogoutPost() {
            var user = await _userManager.GetUserAsync(User);

            try {
                var c = new InkbunnyClient(user.InkbunnySessionId);
                await c.LogoutAsync();
            } catch (Exception) { }

            user.InkbunnySessionId = null;
            await _userManager.UpdateAsync(user);

            return LocalRedirect("/Identity/Account/Manage");
        }
    }
}