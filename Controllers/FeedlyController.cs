using System;
using System.Linq;
using System.Threading.Tasks;
using ArtworkInbox.Backend;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Data;
using FeedlySharp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace ArtworkInbox.Controllers {
    public class FeedlyController : FeedController {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public FeedlyController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger<HomeController> logger) {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public IActionResult Index() {
            return RedirectToAction(nameof(Feed));
        }

        protected override Task<ApplicationUser> GetUserAsync() =>
            _userManager.GetUserAsync(User);

        protected override string GetSiteName() => "Feedly";

        protected override async Task<IFeedSource> GetFeedSourceAsync() {
            var client = new FeedlySharpHttpClient(new FeedlySharp.Models.FeedlyOptions {
                UserID = "a9487394-85bc-47f0-b179-037d6af906aa",
                AccessToken = "",
                Domain = "https://cloud.feedly.com"
            });
            return new FeedlyFeedSource(client);
        }

        protected override async Task<DateTimeOffset> GetLastRead() {
            return DateTimeOffset.MinValue;
        }

        protected override async Task SetLastRead(DateTimeOffset lastRead) {
            throw new NotImplementedException();
        }
    }
}