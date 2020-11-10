using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Azure.Management.Compute.Fluent.Models;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VM.Web.Options;

namespace VM.Web.Pages.VM
{
    public class DetailsPageModel : PageModel
    {
        private readonly ILogger<DetailsPageModel> logger;
        private readonly IAzure azure;

        public DetailsPageModel(IOptions<AzureAdOptions> azureAdOptionsValue, ILogger<DetailsPageModel> logger)
        {
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
        
        public async Task OnGet(string id)
        {
            Id = id;
            logger.LogInformation($"Calling virtual machine by ID {Id}");
            var virtualMachine = await azure.VirtualMachines.GetByIdAsync(id);
            VirtualMachine = virtualMachine;
            
            logger.LogInformation("Getting back the information about available sizes");
            
            var list = new List<SelectListItem>();
            foreach (var virtualMachineSize in virtualMachine.AvailableSizes())
            {
                var text = $"{virtualMachineSize.Name} with {virtualMachineSize.NumberOfCores} cores {virtualMachineSize.MemoryInMB} memory";
                list.Add(new SelectListItem(text, virtualMachineSize.Name));
            }
            PossibleSizes = list;
        }

        public async Task<RedirectToPageResult> OnPostAsync()
        {
            var form = await Request.ReadFormAsync();
            
            var virtualMachine = await azure.VirtualMachines.GetByIdAsync(Id);

            logger.LogInformation("Doing resizing");
            
            var size = form["size"];
            if (!string.IsNullOrEmpty(size))
            {
                logger.LogInformation($"Doing resizing with size {size}");
                try
                {
                    virtualMachine.Inner.HardwareProfile.VmSize = ExpandableStringEnum<VirtualMachineSizeTypes>.Parse(size);
                    virtualMachine.Update();
                    logger.LogInformation($"Machine updated with new size {size}");
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    logger.LogError(e.Message);
                }
            }
            return RedirectToPage("/VM/Details", Id);
        }

        [BindProperty] public string Id { get; set; }
        
        [BindProperty]
        public List<SelectListItem> PossibleSizes { get; set; }

        [BindProperty]
        public IVirtualMachine VirtualMachine { get; private set; }
    }
}