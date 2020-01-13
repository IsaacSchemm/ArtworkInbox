using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ArtworkInbox.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ArtworkInbox.Areas.Identity.Pages.Account.Manage
{
    public partial class IndexModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly SignInManager<ApplicationUser> _signInManager;

        public IndexModel(
            UserManager<ApplicationUser> userManager,
            SignInManager<ApplicationUser> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "Weasyl API key (optional)")]
            public string WeasylApiKey { get; set; }

            [Display(Name = "Hide reposts")]
            public bool HideReposts { get; set; }

            [Display(Name = "Hide mature content")]
            public bool HideMature { get; set; }

            [Display(Name = "Hide mature content thumbnails")]
            public bool HideMatureThumbnails { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var userName = await _userManager.GetUserNameAsync(user);

            Username = userName;

            Input = new InputModel
            {
                WeasylApiKey = user.WeasylApiKey,
                HideReposts = user.HideReposts,
                HideMature = user.HideMature,
                HideMatureThumbnails = user.HideMatureThumbnails
            };
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            await LoadAsync(user);
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return NotFound($"Unable to load user with ID '{_userManager.GetUserId(User)}'.");
            }

            if (!ModelState.IsValid)
            {
                await LoadAsync(user);
                return Page();
            }

            if (string.IsNullOrWhiteSpace(Input.WeasylApiKey))
                Input.WeasylApiKey = null;

            user.WeasylApiKey = Input.WeasylApiKey;
            user.HideReposts = Input.HideReposts;
            user.HideMature = Input.HideMature;
            user.HideMatureThumbnails = Input.HideMatureThumbnails;
            await _userManager.UpdateAsync(user);

            await _signInManager.RefreshSignInAsync(user);
            StatusMessage = "Your profile has been updated";
            return RedirectToPage();
        }
    }
}
