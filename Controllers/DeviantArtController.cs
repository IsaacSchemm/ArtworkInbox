﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using ArtworkInbox.Backend;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Data;
using ArtworkInbox.Models;
using DeviantArtFs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArtworkInbox.Controllers {
    public class DeviantArtController : FeedController {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IDeviantArtAuth _auth;

        public DeviantArtController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger<HomeController> logger, IDeviantArtAuth auth) {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _auth = auth;
        }

        public IActionResult Index() {
            return RedirectToAction(nameof(Feed));
        }

        protected override Task<ApplicationUser> GetUserAsync() =>
            _userManager.GetUserAsync(User);

        protected override string GetSiteName() => "DeviantArt";

        protected override async Task<IFeedSource> GetFeedSourceAsync() {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserDeviantArtTokens
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                throw new NoTokenException();
            var token = new DeviantArtTokenWrapper(_auth, _context, dbToken);
            return new DeviantArtFeedSource(token);
        }

        protected override async Task<DateTimeOffset> GetLastRead() {
            var userId = _userManager.GetUserId(User);
            var dt = await _context.UserDeviantArtTokens
                .Where(t => t.UserId == userId)
                .Select(t => t.LastRead)
                .SingleOrDefaultAsync();
            return dt ?? DateTimeOffset.MinValue;
        }

        protected override async Task SetLastRead(DateTimeOffset lastRead) {
            var userId = _userManager.GetUserId(User);
            var o = await _context.UserDeviantArtTokens
                .Where(t => t.UserId == userId)
                .SingleAsync();

            o.LastRead = lastRead;
            await _context.SaveChangesAsync();
        }

        [HttpGet]
        public async Task<IActionResult> FeedSettings() {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserDeviantArtTokens
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                return View("NoToken");
            var token = new DeviantArtTokenWrapper(_auth, _context, dbToken);
            var feedSettings = await DeviantArtFs.Requests.Feed.FeedSettings.ExecuteAsync(token);
            return View(new DeviantArtFeedSettingsViewModel {
                Statuses = feedSettings.Include.Statuses,
                Deviations = feedSettings.Include.Deviations,
                Journals = feedSettings.Include.Journals,
                GroupDeviations = feedSettings.Include.GroupDeviations,
                Collections = feedSettings.Include.Collections,
                Misc = feedSettings.Include.Misc,
            });
        }

        [HttpPost, ValidateAntiForgeryToken]
        public async Task<IActionResult> FeedSettings(DeviantArtFeedSettingsViewModel model) {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserDeviantArtTokens
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                return View("NoToken");
            var token = new DeviantArtTokenWrapper(_auth, _context, dbToken);
            await DeviantArtFs.Requests.Feed.FeedSettingsUpdate.ExecuteAsync(token, new DeviantArtFs.Requests.Feed.FeedSettingsUpdateRequest {
                Statuses = model.Statuses,
                Deviations = model.Deviations,
                Journals = model.Journals,
                GroupDeviations = model.GroupDeviations,
                Collections = model.Collections,
                Misc = model.Misc
            });
            return View(model);
        }
    }
}