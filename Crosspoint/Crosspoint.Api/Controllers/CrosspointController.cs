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

        public CrosspointController(ILogger<CrosspointController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        public IEnumerable<Models.CrosspointMapping> Get()
        {
            return null;
        }
    }
}
