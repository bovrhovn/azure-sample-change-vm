using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VM.Web.Hub;
using VM.Web.Interfaces;
using VM.Web.Options;

namespace VM.Web.Pages.VM
{
    [Authorize]
    public class ListPageModel : PageModel
    {
        private readonly IHubContext<NotificationHub> notificationHubContext;
        private readonly ILogger<ListPageModel> logger;
        private readonly IAzureVmService azureVmService;
        
        public ListPageModel(IOptions<AzureAdOptions> azureAdOptionsValue,
            IHubContext<NotificationHub> notificationHubContext,
            ILogger<ListPageModel> logger, IAzureVmService azureVmService)
        {
            this.notificationHubContext = notificationHubContext;
            this.logger = logger;
            this.azureVmService = azureVmService;
        }

        public async Task OnGet()
        {
            logger.LogInformation("Getting virtual machines from subscriptions...");
            Vms = await azureVmService.GetVirtualMachinesAsync();
            logger.LogInformation($"Received {Vms.Count} machines");
        }

        public async Task<RedirectToPageResult> OnPostAsync()
        {
            logger.LogInformation($"Changing state for the machine to {(IsShutdown ? "start" : "stop")}");
            await azureVmService.ChangeStateAsync(VirtualMachineId, IsShutdown);
            return RedirectToPage("/Vm/List");
        }

        [BindProperty] public string VirtualMachineId { get; set; }
        [BindProperty] public bool IsShutdown { get; set; }
        [BindProperty] public List<IVirtualMachine> Vms { get; private set; } = new List<IVirtualMachine>();
    }
}