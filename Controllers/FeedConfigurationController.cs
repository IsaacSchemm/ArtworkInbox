using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ArtworkInbox.Data;

namespace ArtworkInbox.Controllers
{
    public class FeedConfigurationController : Controller
    {
        private readonly ApplicationDbContext _context;

        public FeedConfigurationController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: FeedConfiguration
        public async Task<IActionResult> Index()
        {
            var applicationDbContext = _context.UserExternalFeeds.Include(u => u.User);
            return View(await applicationDbContext.ToListAsync());
        }

        // GET: FeedConfiguration/Details/5
        public async Task<IActionResult> Details(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userExternalFeed = await _context.UserExternalFeeds
                .Include(u => u.User)
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
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: FeedConfiguration/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,Type,Url,LastRead")] UserExternalFeed userExternalFeed)
        {
            if (ModelState.IsValid)
            {
                userExternalFeed.Id = Guid.NewGuid();
                _context.Add(userExternalFeed);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", userExternalFeed.UserId);
            return View(userExternalFeed);
        }

        // GET: FeedConfiguration/Edit/5
        public async Task<IActionResult> Edit(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userExternalFeed = await _context.UserExternalFeeds.FindAsync(id);
            if (userExternalFeed == null)
            {
                return NotFound();
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", userExternalFeed.UserId);
            return View(userExternalFeed);
        }

        // POST: FeedConfiguration/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, [Bind("Id,UserId,Type,Url,LastRead")] UserExternalFeed userExternalFeed)
        {
            if (id != userExternalFeed.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(userExternalFeed);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!UserExternalFeedExists(userExternalFeed.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", userExternalFeed.UserId);
            return View(userExternalFeed);
        }

        // GET: FeedConfiguration/Delete/5
        public async Task<IActionResult> Delete(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userExternalFeed = await _context.UserExternalFeeds
                .Include(u => u.User)
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
            var userExternalFeed = await _context.UserExternalFeeds.FindAsync(id);
            _context.UserExternalFeeds.Remove(userExternalFeed);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool UserExternalFeedExists(Guid id)
        {
            return _context.UserExternalFeeds.Any(e => e.Id == id);
        }
    }
}
