using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using ArtworkInbox.Models;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using Azure.Identity;
using Azure.ResourceManager;
using Azure.ResourceManager.Compute;
using System.Collections.Generic;
using System.Threading;

namespace ArtworkInbox.Controllers {
    public class HomeController : Controller {
        private static readonly SemaphoreSlim _sem = new(1, 1);

        private static VirtualMachineResource GetVM() {
            var credential = new ChainedTokenCredential(
                new ManagedIdentityCredential(),
                new VisualStudioCredential(new VisualStudioCredentialOptions { TenantId = "dd259809-e6e5-487a-bdfb-8bf0a973b11e" }));
            var client = new ArmClient(credential);
            return client.GetVirtualMachineResource(
                new Azure.Core.ResourceIdentifier(
                    "/subscriptions/533a2be0-3b41-4141-a2cd-a97d9dc4c201/resourcegroups/artworkinbox/providers/Microsoft.Compute/virtualMachines/artworkinbox-db"));
        }

        private static async IAsyncEnumerable<string> GetPowerStatesAsync() {
            var instanceView = await GetVM().InstanceViewAsync();
            foreach (var s in instanceView.Value.Statuses)
                yield return s.Code;
        }

        public async Task<IActionResult> Index() {
            ViewBag.PowerStates = await GetPowerStatesAsync().ToListAsync();
            return View();
        }

        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StartDatabase() {
            await _sem.WaitAsync();
            try {
                if (!await GetPowerStatesAsync().ContainsAsync("PowerState/running")) {
                    var resource = GetVM();
                    await resource.PowerOnAsync(Azure.WaitUntil.Completed);
                }
            } finally {
                _sem.Release();
            }

            return RedirectToAction(nameof(Index));
        }

        [ValidateAntiForgeryToken]
        public async Task<ActionResult> StopDatabase() {
            await _sem.WaitAsync();
            try {
                var resource = GetVM();
                await resource.PowerOffAsync(Azure.WaitUntil.Completed);
            } finally {
                _sem.Release();
            }

            return RedirectToAction(nameof(Index));
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error() {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
