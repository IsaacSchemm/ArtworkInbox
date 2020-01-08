using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using ArtworkInbox.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Logging;

namespace ArtworkInbox.Areas.Identity.Pages.Account
{
    [AllowAnonymous]
    public class RegisterModel : PageModel {
        public IActionResult OnGet() {
            return RedirectToPage("Login");
        }

        public IActionResult OnPost() {
            return RedirectToPage("Login");
        }
    }
}
