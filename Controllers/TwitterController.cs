using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using DANotify.Data;
using DANotify.Models;
using DeviantArtFs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Tweetinvi.Models;

namespace DANotify.Controllers {
    public class TwitterController : Controller {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;
        private readonly IConsumerCredentials _consumerCredentials;

        public TwitterController(UserManager<IdentityUser> userManager, ApplicationDbContext context, ILogger<HomeController> logger, IConsumerCredentials consumerCredentials) {
            _userManager = userManager;
            _context = context;
            _logger = logger;
            _consumerCredentials = consumerCredentials;
        }

        private async Task<ITwitterCredentials> GetTwitterCredentialsAsync() {
            var userId = _userManager.GetUserId(User); 
            var dbToken = await _context.UserTwitterTokens
                .Where(t => t.UserId == userId)
                .SingleOrDefaultAsync();
            return dbToken == null
                ? null
                : new TwitterCredentials(
                    _consumerCredentials.ConsumerKey,
                    _consumerCredentials.ConsumerSecret,
                    dbToken.AccessToken,
                    dbToken.AccessTokenSecret);
        }

        public async Task<IActionResult> Index() {
            var credentials = await GetTwitterCredentialsAsync();
            if (credentials == null)
                return View("NoAccount");
            return Ok(Tweetinvi.Auth.ExecuteOperationWithCredentials(credentials, () => {
                return Tweetinvi.User.GetAuthenticatedUser();
            }));
        }
    }
}