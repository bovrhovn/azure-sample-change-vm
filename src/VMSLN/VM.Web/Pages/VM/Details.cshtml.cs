using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Azure.Management.Compute.Fluent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VM.Web.Interfaces;
using VM.Web.Models;
using VM.Web.Options;

namespace VM.Web.Pages.VM
{
    [Authorize]
    public class DetailsPageModel : PageModel
    {
        private readonly ILogger<DetailsPageModel> logger;
        private readonly IEmailService emailService;
        private readonly IAzureVmService azureVmService;
        private readonly SendGridOptions sendGridOptions;

        public DetailsPageModel(ILogger<DetailsPageModel> logger,
            IEmailService emailService, IAzureVmService azureVmService,
            IOptions<SendGridOptions> sendGridOptionsValue)
        {
            this.logger = logger;
            sendGridOptions = sendGridOptionsValue.Value;
            this.emailService = emailService;
            this.azureVmService = azureVmService;
        }

        public async Task OnGetAsync(string id)
        {
            Id = id;
            logger.LogInformation($"Calling virtual machine by ID {Id}");
            var virtualMachine = await azureVmService.GetMachineByIdAsync(id);
            VirtualMachine = new AzureMachineViewModel
            {
                Os = virtualMachine.OSType.ToString(),
                Powerstate = virtualMachine.PowerState == null
                    ? "Powerstate is not determined, refresh view"
                    : virtualMachine.PowerState.Value,
                Name = virtualMachine.Name,
                Size = virtualMachine.Size == null ? "Size not retrieved" : virtualMachine.Size.Value,
                ResourceGroup = virtualMachine.ResourceGroupName
            };

            logger.LogInformation("Getting back the information about available sizes");

            var list = new List<SelectListItem>();
            foreach (var virtualMachineSize in virtualMachine.AvailableSizes())
            {
                var text =
                    $"{virtualMachineSize.Name} with {virtualMachineSize.NumberOfCores} cores {virtualMachineSize.MemoryInMB} memory";
                list.Add(new SelectListItem(text, virtualMachineSize.Name));
            }

            InfoText = $"Machine {virtualMachine.Name} loaded with {list.Count} sizes";
            PossibleSizes = list;
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var form = await Request.ReadFormAsync();

            var size = form["size"];
            if (!string.IsNullOrEmpty(size))
            {
                var startWatch = Stopwatch.StartNew();

                try
                {
                    await azureVmService.ResizeMachineAsync(Id, size);

                    var subject = $"Virtual machine {Id} resized";
                    string pathToList = Url.PageLink("/VM/List");
                    string body =
                        $"<h1>{subject}</h1><p>Machine has been updated, click <a href='{pathToList}'>here</a> for more information"; //TODO: download html email and send templated email

                    logger.LogInformation("Starting to send email");

                    await emailService.SendEmailAsync(sendGridOptions.FromDefaultEmail, sendGridOptions.ToDefaultEmail,
                        subject, body);

                    startWatch.Stop();

                    var message =
                        $"Email sent and it took {startWatch.ElapsedMilliseconds} ms ({startWatch.ElapsedMilliseconds / 1000} s)";
                    InfoText = message;
                    logger.LogInformation(message);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e);
                    logger.LogError(e.Message);
                }
            }

            return RedirectToPage("/VM/Details", new {id = Id});
        }

        [BindProperty] public string Id { get; set; }
        [TempData] public string InfoText { get; set; }
        [BindProperty] public List<SelectListItem> PossibleSizes { get; set; }
        [BindProperty] public AzureMachineViewModel VirtualMachine { get; set; } = new AzureMachineViewModel();
    }
}