using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;
using ArtworkInbox.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;

namespace ArtworkInbox.Areas.Identity.Pages.Account.Manage
{
    public partial class CookiesAndKeysModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public CookiesAndKeysModel(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public string Username { get; set; }

        [TempData]
        public string StatusMessage { get; set; }

        [BindProperty]
        public InputModel Input { get; set; }

        public class InputModel
        {
            [Display(Name = "FA_COOKIE")]
            public string FurAffinityCookies { get; set; }

            [Display(Name = "Weasyl API key")]
            public string WeasylApiKey { get; set; }
        }

        private async Task LoadAsync(ApplicationUser user)
        {
            var furAffinity = await _context.UserFurAffinityTokens
                .Where(x => x.UserId == user.Id)
                .Select(x => x.FA_COOKIE)
                .FirstOrDefaultAsync();
            var weasyl = await _context.UserWeasylTokens
                .Where(x => x.UserId == user.Id)
                .Select(x => x.ApiKey)
                .FirstOrDefaultAsync();

            Input = new InputModel
            {
                FurAffinityCookies = furAffinity,
                WeasylApiKey = weasyl
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

            var furAffinity = await _context.UserFurAffinityTokens
                .Where(x => x.UserId == user.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(Input.FurAffinityCookies)) {
                if (furAffinity != null) {
                    _context.UserFurAffinityTokens.Remove(furAffinity);
                }
            } else {
                if (furAffinity == null) {
                    furAffinity = new UserFurAffinityToken {
                        UserId = user.Id,
                        LastRead = DateTimeOffset.MinValue
                    };
                    _context.UserFurAffinityTokens.Add(furAffinity);
                }
                furAffinity.FA_COOKIE = Input.FurAffinityCookies;
            }

            var weasyl = await _context.UserWeasylTokens
                .Where(x => x.UserId == user.Id)
                .FirstOrDefaultAsync();

            if (string.IsNullOrWhiteSpace(Input.WeasylApiKey)) {
                if (weasyl != null) {
                    _context.UserWeasylTokens.Remove(weasyl);
                }
            } else {
                if (weasyl == null) {
                    weasyl = new UserWeasylToken {
                        UserId = user.Id,
                        LastRead = DateTimeOffset.MinValue
                    };
                    _context.UserWeasylTokens.Add(weasyl);
                }
                weasyl.ApiKey = Input.WeasylApiKey;
            }

            await _context.SaveChangesAsync();

            StatusMessage = "Your cookies and keys have been updated";
            return RedirectToPage();
        }
    }
}
