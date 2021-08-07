using Crosspoint.Communicator;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Crosspoint.Api.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class CrosspointController : ControllerBase
    {

        private readonly ILogger<CrosspointController> _logger;
        private readonly ExtronCrosspointCommunicator communicator;

        public CrosspointController(
            ILogger<CrosspointController> logger,
            ExtronCrosspointCommunicator communicator)
        {
            _logger = logger;
            this.communicator = communicator;
        }

        [HttpGet]
        public IEnumerable<Models.CrosspointMapping> Get()
        {
            var mappings = new List<Models.CrosspointMapping>();
            if (!communicator.IsSystemReady)
            {
                return null;
            }
            foreach(var mapping in communicator.Mappings)
            {

            }
            return mappings;
        }
    }
}
