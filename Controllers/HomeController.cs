using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace ArtworkInbox.Controllers {
    public class HomeController : Controller {
        public IActionResult Index() {
            return Redirect("https://github.com/IsaacSchemm/ArtworkInbox");
        }
    }
}
