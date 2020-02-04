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
    public class TwitterController : FeedController {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IConsumerCredentials _consumerCredentials;

        public TwitterController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger<HomeController> logger, IConsumerCredentials consumerCredentials) {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _consumerCredentials = consumerCredentials;
        }

        public IActionResult Index() {
            return RedirectToAction(nameof(Feed));
        }

        protected override Task<ApplicationUser> GetUserAsync() =>
            _userManager.GetUserAsync(User);

        protected override string GetSiteName() => "Twitter";

        protected override async Task<IFeedSource> GetFeedSourceAsync() {
            var userId = _userManager.GetUserId(User);
            var dbToken = await _context.UserTwitterTokens
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            if (dbToken == null)
                throw new NoTokenException();
            var credentials = new TwitterCredentials(
                _consumerCredentials.ConsumerKey,
                _consumerCredentials.ConsumerSecret,
                dbToken.AccessToken,
                dbToken.AccessTokenSecret);
            return new TwitterFeedSource(credentials);
        }

        protected override async Task<DateTimeOffset> GetLastRead() {
            var userId = _userManager.GetUserId(User);
            var dt = await _context.UserTwitterTokens
                .Where(t => t.UserId == userId)
                .Select(t => t.LastRead)
                .SingleOrDefaultAsync();
            return dt ?? DateTimeOffset.MinValue;
        }

        protected override async Task SetLastRead(DateTimeOffset lastRead) {
            var userId = _userManager.GetUserId(User);
            var o = await _context.UserTwitterTokens
                .Where(t => t.UserId == userId)
                .SingleAsync();

            o.LastRead = lastRead;
            await _context.SaveChangesAsync();
        }
    }
}