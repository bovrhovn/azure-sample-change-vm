using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using VM.Web.Interfaces;
using VM.Web.Options;

namespace VM.Web.Controllers
{
    [ApiController]
    [Route("router")]
    public class AlertNotificationController : Controller
    {
        private readonly ILogger<AlertNotificationController> logger;
        private readonly IEmailService emailService;
        private readonly SendGridOptions sendGridOptions;

        public AlertNotificationController(ILogger<AlertNotificationController> logger,
            IEmailService emailService, IOptions<SendGridOptions> sendGridOptionsValue)
        {
            this.logger = logger;
            sendGridOptions = sendGridOptionsValue.Value;
            this.emailService = emailService;
        }

        [Route("size-request")]
        public async Task<IActionResult> ResizeRequest()
        {
            try
            {
                var subject = $"Virtual machine has reach limit and needs to be resized";
                string pathToList = Url.PageLink("/VM/List");
                string body =
                    $"<h1>{subject}</h1><p>Machine has reach the limit, click <a href='{pathToList}'>here</a> for more information";

                logger.LogInformation("Starting to send email");

                await emailService.SendEmailAsync(sendGridOptions.FromDefaultEmail,
                    sendGridOptions.ToDefaultEmail,
                    subject, body);
                
                logger.LogInformation("Email was sent");
            }
            catch (Exception e)
            {
                logger.LogError(e.Message);
                return BadRequest($"There was an error sending email {e.Message}");
            }

            return Ok("email was sent");
        }
    }
}