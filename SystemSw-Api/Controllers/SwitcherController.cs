using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SystemSw_Api.Models;
using SystemSw_Core.Extron;

namespace SystemSw_Api.Controllers
{

    [ApiController]
    [Route("[controller]")]
    public class SwitcherController : ControllerBase
    {
        
        private readonly ILogger<SwitcherController> logger;
        private readonly ExtronCommunicator ec;
        private readonly Dictionary<string, string> mappings;

        public SwitcherController(
            IConfiguration configuration,
            ILogger<SwitcherController> logger,
            ExtronCommunicator ec)
        {
            this.logger = logger ?? throw new ArgumentNullException(nameof(logger));
            this.ec = ec ?? throw new ArgumentNullException(nameof(ec));
            this.mappings = new Dictionary<string, string>();
            configuration.GetSection("ChannelMappings").Bind(this.mappings);
        }

        [HttpGet]
        public IEnumerable<ExtronMappedEntry> Get()
        {
            return GetExtronMappings();
        }

        [HttpPost]
        public async Task<IEnumerable<ExtronMappedEntry>> Switch([FromBody]ExtronMappedEntry entry)
        {
            ec.ChangeChannel(entry.Channel);
            await Task.Delay(100);
            return GetExtronMappings();
        }

        private IEnumerable<ExtronMappedEntry> GetExtronMappings()
        {
            var coll = new List<ExtronMappedEntry>();
            logger.LogInformation($"Generating Mappings for {ec.Channels} entries");
            for (var c = 0; c < ec.Channels; c++)
            {
                var cs = (c + 1).ToString();
                var eme = new ExtronMappedEntry()
                {
                    Channel = c + 1,
                    ChannelName = mappings.ContainsKey(cs) ? mappings[cs] : "?",
                    IsActiveChannel = c + 1 == ec.VideoChannel
                };
                coll.Add(eme);
            }
            return coll;
        }

    }
}