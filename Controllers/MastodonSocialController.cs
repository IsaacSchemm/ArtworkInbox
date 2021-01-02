using ArtworkInbox.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace ArtworkInbox.Controllers {
    public class MastodonSocialController : MastodonController {
        public MastodonSocialController(UserManager<ApplicationUser> userManager, IMemoryCache cache, ApplicationDbContext context) : base(userManager, cache, context) { }

        protected override string Host => "mastodon.social";
    }
}