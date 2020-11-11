using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VM.Web.Interfaces;
using VM.Web.Options;

namespace VM.Web.Pages.Info
{
    public class SendEmailPageModel : PageModel
    {
        private readonly IEmailService emailService;
        private readonly ILogger<SendEmailPageModel> logger;
        private readonly SendGridOptions sendGridOptions;

        public SendEmailPageModel(IOptions<SendGridOptions> sendGridOptionsValue, IEmailService emailService,
            ILogger<SendEmailPageModel> logger)
        {
            this.emailService = emailService;
            this.logger = logger;
            sendGridOptions = sendGridOptionsValue.Value;
        }

        [TempData] 
        public string InfoText { get; set; } = "Email information";
        [BindProperty]
        public string Subject { get; set; }
        [BindProperty]
        public string Body { get; set; }

        public async Task<IActionResult> OnPostAsync()
        {
            try
            {
                logger.LogInformation("Sending email");
                await emailService.SendEmailAsync(sendGridOptions.FromDefaultEmail, sendGridOptions.ToDefaultEmail, Subject,
                    Body);
                logger.LogInformation("Email was sent");
                InfoText = "Email was sent";
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
            }

            return RedirectToPage("/Info/SendEmail");
        }
    }
}