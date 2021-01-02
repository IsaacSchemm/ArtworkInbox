using ArtworkInbox.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace ArtworkInbox.Controllers {
    public class BotsinSpaceController : MastodonController {
        public BotsinSpaceController(UserManager<ApplicationUser> userManager, IMemoryCache cache, ApplicationDbContext context) : base(userManager, cache, context) { }

        protected override string Host => "botsin.space";
    }
}