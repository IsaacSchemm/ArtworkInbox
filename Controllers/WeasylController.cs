﻿using System;
using System.Linq;
using System.Threading.Tasks;
using ArtworkInbox.Backend;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tweetinvi.Models;

namespace ArtworkInbox.Controllers {
    public class WeasylController : FeedController {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public WeasylController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger<HomeController> logger) {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public IActionResult Index() {
            return RedirectToAction(nameof(Feed));
        }

        protected override Task<ApplicationUser> GetUserAsync() =>
            _userManager.GetUserAsync(User);

        protected override string GetSiteName() => "Weasyl";

        protected override async Task<FeedSource> GetFeedSourceAsync() {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserWeasylTokens
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                throw new NoTokenException();
            return new WeasylFeedSource(dbToken);
        }

        protected override async Task<DateTimeOffset> GetLastRead() {
            var userId = _userManager.GetUserId(User);
            var dt = await _context.UserReadMarkers
                .Where(t => t.UserId == userId)
                .Select(t => t.WeasylLastRead)
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

            o.WeasylLastRead = lastRead;
            await _context.SaveChangesAsync();
        }
    }
}