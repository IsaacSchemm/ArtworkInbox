using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DeviantArtFs;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace DANotify.Controllers {
    public class DeviantArtController : Controller {
        private readonly ILogger<HomeController> _logger;
        private readonly IDeviantArtAuth _auth;

        public DeviantArtController(ILogger<HomeController> logger, IDeviantArtAuth auth) {
            _logger = logger;
            _auth = auth;
        }

        public IActionResult Index() {
            return RedirectToAction(nameof(Whoami));
        }

        public async Task<IActionResult> Whoami() {
            var authResult = await HttpContext.AuthenticateAsync();
            var token = new DeviantArtTokenWrapper(_auth, authResult.Properties);
            var tx = new DeviantArtCommonParameters {
                Expand = DeviantArtObjectExpansion.UserDetails | DeviantArtObjectExpansion.UserStats
            }.WrapToken(token);
            return Ok(await DeviantArtFs.Requests.User.Whoami.ExecuteAsync(tx));
        }
    }
}