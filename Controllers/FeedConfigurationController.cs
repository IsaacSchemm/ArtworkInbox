using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ArtworkInbox.Data;
using Microsoft.AspNetCore.Identity;

namespace ArtworkInbox.Controllers
{
    public class FeedConfigurationController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public FeedConfigurationController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        // GET: FeedConfiguration
        public async Task<IActionResult> Index()
        {
            string userId = _userManager.GetUserId(User);
            var applicationDbContext = _context.UserExternalFeeds.AsQueryable().Where(f => f.UserId == userId);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: FeedConfiguration/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string userId = _userManager.GetUserId(User);
            var userExternalFeed = await _context.UserExternalFeeds
                .AsQueryable()
                .Where(f => f.UserId == userId)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userExternalFeed == null)
            {
                return NotFound();
            }

            return View(userExternalFeed);
        }

        // GET: FeedConfiguration/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: FeedConfiguration/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Type,Url")] UserExternalFeed userExternalFeed)
        {
            if (ModelState.IsValid)
            {
                userExternalFeed.UserId = _userManager.GetUserId(User);
                userExternalFeed.Id = Guid.NewGuid();
                _context.Add(userExternalFeed);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(userExternalFeed);
        }

        // GET: FeedConfiguration/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            string userId = _userManager.GetUserId(User);
            var userExternalFeed = await _context.UserExternalFeeds
                .AsQueryable()
                .Where(f => f.UserId == userId)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (userExternalFeed == null)
            {
                return NotFound();
            }

            return View(userExternalFeed);
        }

        // POST: FeedConfiguration/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            string userId = _userManager.GetUserId(User);
            var userExternalFeed = await _context.UserExternalFeeds
                .AsQueryable()
                .Where(f => f.UserId == userId)
                .SingleAsync(m => m.Id == id);
            _context.UserExternalFeeds.Remove(userExternalFeed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }
    }
}
