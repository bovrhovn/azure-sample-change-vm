using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using VM.Web.Interfaces;
using VM.Web.Models;
using VM.Web.Options;

namespace VM.Web.Controllers
{
    [ApiController]
    [Route("router")]
    public class AlertNotificationController : ControllerBase
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

        [Route("check")]
        public IActionResult HealthCheck() => Ok($"Call was successfully done at {DateTime.Now}");

        [Route("size-request")]
        [HttpPost]
        public async Task<IActionResult> ResizeRequest()
        {
            try
            {
                using var stream = new StreamReader(Request.Body);
                string reizeBody = await stream.ReadToEndAsync();
                var alertModel = JsonConvert.DeserializeObject<AlertModel>(reizeBody);
                
                //CAN RESIZE based on information received, if needed
                string additionalInformation = "";
                if (alertModel!= null)
                    if (alertModel.Data?.AlertContext?.Condition?.AllOf.Count > 0)
                        additionalInformation =
                            $"Metrics {alertModel.Data.AlertContext.Condition.AllOf[0].MetricName} Value {alertModel.Data.AlertContext.Condition.AllOf[0].MetricValue}";
                
                var subject = $"Virtual machine has reach limit and needs to be resized";
                string pathToList = Url.PageLink("/VM/List");
                string body =
                    $"<h1>{subject}</h1><p>Machine has reach the limit, click <a href='{pathToList}'>here</a> for more information. <p>{additionalInformation}</p>";

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