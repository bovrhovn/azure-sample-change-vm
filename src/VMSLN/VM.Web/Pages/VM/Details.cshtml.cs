using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
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
using VM.Web.Interfaces;
using VM.Web.Options;

namespace VM.Web.Pages.VM
{
    [Authorize]
    public class DetailsPageModel : PageModel
    {
        private readonly ILogger<DetailsPageModel> logger;
        private readonly IEmailService emailService;
        private readonly IAzure azure;
        private readonly SendGridOptions sendGridOptions;

        public DetailsPageModel(IOptions<AzureAdOptions> azureAdOptionsValue,
            ILogger<DetailsPageModel> logger, IEmailService emailService,
            IOptions<SendGridOptions> sendGridOptionsValue)
        {
            this.logger = logger;
            sendGridOptions = sendGridOptionsValue.Value;
            this.emailService = emailService;
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
                var text =
                    $"{virtualMachineSize.Name} with {virtualMachineSize.NumberOfCores} cores {virtualMachineSize.MemoryInMB} memory";
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
                
                var startWatch = Stopwatch.StartNew();
                
                try
                {
                    startWatch.Start();
                    logger.LogInformation("Machine size changing...");
                    virtualMachine.Inner.HardwareProfile.VmSize =
                        ExpandableStringEnum<VirtualMachineSizeTypes>.Parse(size);
                    virtualMachine.Update();
                    startWatch.Stop();
                    logger.LogInformation($"Machine size changing took {startWatch.ElapsedMilliseconds} ms ({startWatch.ElapsedMilliseconds/1000} s)");
                    
                    startWatch.Start();
                    logger.LogInformation("Starting the machine as it stopped");
                    await virtualMachine.StartAsync();
                    startWatch.Stop();
                    logger.LogInformation($"Machine updated with new size {size} and started machine in {startWatch.ElapsedMilliseconds} ms ({startWatch.ElapsedMilliseconds/1000} s).");

                    startWatch.Start();
                    var subject = $"Virtual machine {virtualMachine.Name} resized";
                    string pathToList = Url.PageLink("/VM/List");
                    string body =
                        $"<h1>{subject}</h1><p>Machine has been updated, click <a href='{pathToList}'>here</a> for more information"; //TODO: download html email and send templated email

                    logger.LogInformation("Starting to send email");
                    
                    await emailService.SendEmailAsync(sendGridOptions.FromDefaultEmail, sendGridOptions.ToDefaultEmail,
                        subject, body);
                    
                    startWatch.Stop();
                    
                    logger.LogInformation($"Email sent and it took {startWatch.ElapsedMilliseconds} ms ({startWatch.ElapsedMilliseconds/1000} s)");
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

        [BindProperty] public List<SelectListItem> PossibleSizes { get; set; }

        [BindProperty] public IVirtualMachine VirtualMachine { get; private set; }
    }
}