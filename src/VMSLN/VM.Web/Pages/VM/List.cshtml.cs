using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Extensions.Options;
using VM.Web.Models;
using VM.Web.Options;

namespace VM.Web.Pages.VM
{
    [Authorize]
    public class ListPageModel : PageModel
    {
        private readonly AzureAdOptions adOptions;

        public ListPageModel(IOptions<AzureAdOptions> azureAdOptionsValue) => adOptions = azureAdOptionsValue.Value;

        public async Task OnGet()
        {
            var credentials = SdkContext.AzureCredentialsFactory
                .FromServicePrincipal(adOptions.ClientId, adOptions.ClientSecret,
                    adOptions.TenantId,
                    AzureEnvironment.AzureGlobalCloud);
            
            var azure = Microsoft.Azure.Management.Fluent.Azure
                .Configure()
                .Authenticate(credentials)
                .WithDefaultSubscription();

            var virtualMachines = await azure.VirtualMachines.ListAsync();
            foreach (var virtualMachine in virtualMachines)
            {
                Vms.Add(new AzureVM
                {
                    Name = virtualMachine.Name,
                    OSName = virtualMachine.OSType.ToString(),
                    PowerType = virtualMachine.PowerState.Value
                });
            }
        }

        [BindProperty]
        public List<AzureVM> Vms { get; set; } = new List<AzureVM>();
    }
}