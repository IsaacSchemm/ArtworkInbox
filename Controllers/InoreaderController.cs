using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ArtworkInbox.Backend;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Data;
using ArtworkInbox.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArtworkInbox.Controllers {
    public class InoreaderController : FeedController {
        private readonly InoreaderFs.Auth.App _app;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public InoreaderController(InoreaderFs.Auth.App app, UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger<HomeController> logger) {
            _app = app;
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public IActionResult Index() {
            return RedirectToAction(nameof(Feed));
        }

        protected override Task<ApplicationUser> GetUserAsync() =>
            _userManager.GetUserAsync(User);

        protected override string GetSiteName() => "Inoreader";

        protected override async Task<IFeedSource> GetFeedSourceAsync() {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserInoreaderTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                throw new NoTokenException();
            var token = new InoreaderTokenWrapper(_app, _context, dbToken);
            return new InoreaderFeedSource(
                InoreaderFs.Auth.Credentials.NewOAuth(token),
                allAsText: dbToken.AllAsText,
                unreadOnly: dbToken.UnreadOnly);
        }

        protected override async Task<DateTimeOffset> GetLastRead() {
            var userId = _userManager.GetUserId(User);
            var dt = await _context.UserInoreaderTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .Select(t => t.LastRead)
                .SingleOrDefaultAsync();
            return dt ?? DateTimeOffset.MinValue;
        }

        protected override async Task SetLastRead(DateTimeOffset lastRead) {
            var userId = _userManager.GetUserId(User);
            var o = await _context.UserInoreaderTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .SingleAsync();

            o.LastRead = lastRead;
            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public async Task<IActionResult> Settings() {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserInoreaderTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                return View("NoToken");
            return View(new InoreaderSettingsViewModel {
                AllAsText = dbToken.AllAsText,
                UnreadOnly = dbToken.UnreadOnly
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> Settings(InoreaderSettingsViewModel model) {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserInoreaderTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                return View("NoToken");
            dbToken.AllAsText = model.AllAsText;
            dbToken.UnreadOnly = model.UnreadOnly;
            await _context.SaveChangesAsync();
            return View(model);
        }
    }
}