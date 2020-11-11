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
using VM.Web.Options;

namespace VM.Web.Pages.VM
{
    [Authorize]
    public class ListPageModel : PageModel
    {
        private readonly IHubContext<NotificationHub> notificationHubContext;
        private readonly ILogger<ListPageModel> logger;
        private readonly IAzure azure;

        public ListPageModel(IOptions<AzureAdOptions> azureAdOptionsValue,
            IHubContext<NotificationHub> notificationHubContext,
            ILogger<ListPageModel> logger)
        {
            this.notificationHubContext = notificationHubContext;
            this.logger = logger;
            var adOptions = azureAdOptionsValue.Value;
            var credentials = SdkContext.AzureCredentialsFactory
                .FromServicePrincipal(adOptions.ClientId, adOptions.ClientSecret,
                    adOptions.TenantId, AzureEnvironment.AzureGlobalCloud);
            azure = Microsoft.Azure.Management.Fluent.Azure
                .Configure()
                .Authenticate(credentials)
                .WithSubscription(adOptions.SubscriptionId);
        }

        public async Task OnGet()
        {
            logger.LogInformation("Getting virtual machines from subscriptions...");
            var virtualMachines = await azure.VirtualMachines.ListAsync();
            foreach (var virtualMachine in virtualMachines)
            {
                Vms.Add(virtualMachine);
            }
        }

        public async Task<RedirectToPageResult> OnPostAsync()
        {
            logger.LogInformation($"Changing state for the machine to {(IsShutdown ? "start" : "stop")}");
            if (IsShutdown)
                await azure.VirtualMachines.StartAsync(ResourceGroupName, Name, CancellationToken.None);
            else
                await azure.VirtualMachines.PowerOffAsync(ResourceGroupName, Name, CancellationToken.None);

            return RedirectToPage("/Vm/List");
        }

        [BindProperty] public string Name { get; set; }
        [BindProperty] public bool IsShutdown { get; set; }
        [BindProperty] public string ResourceGroupName { get; set; }
        [BindProperty] public List<IVirtualMachine> Vms { get; } = new List<IVirtualMachine>();
    }
}