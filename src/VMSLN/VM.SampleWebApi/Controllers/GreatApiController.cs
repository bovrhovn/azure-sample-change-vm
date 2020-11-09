using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using VM.SampleWebApi.Models;

namespace VM.SampleWebApi.Controllers
{
    [ApiController]
    [Route("api")]
    public class GreatApiController : ControllerBase
    {
        private readonly ILogger<GreatApiController> logger;

        public GreatApiController(ILogger<GreatApiController> logger) => this.logger = logger;

        [HttpGet]
        [Route("random")]
        public RandomText Get()
        {
            var randomText = new[]
            {
                "I am your father", "It works, just I am really not productive", "Gin-o-time",
                "Beer is overated - hahha, virus found", "Happy hour - insert coin to continue",
                "I am calling you from my Linux machine", "Windows machine is working over the working hours"
            };
            var rng = new Random().Next(0, randomText.Length - 1);
            return new RandomText {Text = randomText[rng]};
        }
    }
}