using Crosspoint.Communicator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;

namespace Crosspoint.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CrosspointController : ControllerBase
    {

        private readonly ILogger<CrosspointController> _logger;
        private readonly IConfiguration configuration;
        private readonly ExtronCrosspointCommunicator communicator;

        public CrosspointController(
            ILogger<CrosspointController> logger,
            IConfiguration configuration,
            ExtronCrosspointCommunicator communicator)
        {
            _logger = logger;
            this.configuration = configuration;
            this.communicator = communicator;
            this.communicator.WaitUntilReady();
        }

        [HttpGet]
        public IEnumerable<Models.CrosspointMapping> Get() =>
            communicator.Mappings.Select(mapping =>
            {
                return new Models.CrosspointMapping
                {
                    AudioInput = mapping.Value.AudioInput,
                    VideoInput = mapping.Value.VideoInput,
                    Output = mapping.Key,

                };
            });
    }
}
