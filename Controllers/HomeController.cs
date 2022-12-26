using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ArtworkInbox.Models;
using System.Threading.Tasks;
using System.Net.Http;
using System;
using Microsoft.FSharp.Collections;
using System.Net.Http.Json;
using System.Linq;
using ArtworkInbox.Data;

namespace ArtworkInbox.Controllers {
    public class HomeController : Controller {
        private readonly ApplicationDbContext _context;
        private readonly HttpClient _httpClient;

        public HomeController(ApplicationDbContext context, IHttpClientFactory httpClientFactory) {
            _context = context;
            _httpClient = httpClientFactory.CreateClient();
            _httpClient.BaseAddress = new Uri("https://artworkinbox-db-management.azurewebsites.net");
        }

        public async Task<IActionResult> Index() {
            //await _context.Database.EnsureCreatedAsync();
            try {
                var powerStates = await _httpClient.GetFromJsonAsync<FSharpList<string>>("/api/power-states");
                ViewBag.DatabaseServerStatus = $"{powerStates}";
                ViewBag.CanStartDatabaseServer = !powerStates.Contains("PowerState/running");
            } catch (Exception) {
                ViewBag.DatabaseServerStatus = "unknown";
                ViewBag.CanStartDatabaseServer = false;
            }
            return View();
        }

        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StartDatabase() {
            var response = await _httpClient.PostAsync("/api/start", null);
            if (!response.IsSuccessStatusCode)
                return StatusCode(502);
            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
