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
using Microsoft.Extensions.Caching.Memory;

namespace ArtworkInbox.Controllers {
    public class InoreaderController : SourceController {
        private readonly InoreaderFs.Auth.App _app;
        private readonly ApplicationDbContext _context;

        public InoreaderController(InoreaderFs.Auth.App app, UserManager<ApplicationUser> userManager, IMemoryCache cache, ApplicationDbContext context) : base(userManager, cache) {
            _app = app;
            _context = context;
        }

        protected override string SiteName => "Inoreader";

        protected override async Task<ISource> GetSourceAsync() {
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

        protected override async Task<DateTimeOffset> GetLastReadAsync() {
            var userId = _userManager.GetUserId(User);
            var dt = await _context.UserInoreaderTokens
                .AsQueryable()
                .Where(t => t.UserId == userId)
                .Select(t => t.LastRead)
                .SingleOrDefaultAsync();
            return dt ?? DateTimeOffset.MinValue;
        }

        protected override async Task SetLastReadAsync(DateTimeOffset lastRead) {
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