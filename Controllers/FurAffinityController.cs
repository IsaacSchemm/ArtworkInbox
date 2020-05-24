using System;
using System.Threading.Tasks;
using ArtworkInbox.Backend.Sources;
using ArtworkInbox.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ArtworkInbox.Controllers {
    public class FurAffinityController : FeedController {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly ILogger<HomeController> _logger;

        public FurAffinityController(UserManager<ApplicationUser> userManager, ApplicationDbContext context, ILogger<HomeController> logger) {
            _userManager = userManager;
            _context = context;
            _logger = logger;
        }

        public IActionResult Index() {
            return RedirectToAction(nameof(Feed));
        }

        protected override Task<ApplicationUser> GetUserAsync() =>
            _userManager.GetUserAsync(User);

        protected override string GetSiteName() => "FurAffinity";

        protected override async Task<IFeedSource> GetFeedSourceAsync() {
            return new FurAffinityFeedSource("a=afcfaa2e-06f8-4f83-90f4-4dfdf7df93f0; b=6afd3cf9-8de9-4938-b9eb-2a39771337cf");
        }

        protected override async Task<DateTimeOffset> GetLastRead() {
            return DateTimeOffset.MinValue;
        }

        protected override async Task SetLastRead(DateTimeOffset lastRead) {
            throw new NotImplementedException();
        }
    }
}