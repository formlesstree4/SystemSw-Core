using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SystemSw_Core.Extron;
using SystemSw_UI.Models;

namespace SystemSw_UI.Controllers
{
    public sealed class SwitcherController : Controller
    {

        private readonly ILogger<SwitcherController> logger;
        private readonly IConfiguration configuration;
        private readonly ExtronCommunicator ec;
        private readonly Dictionary<string, string> mappings;

        public int Channels { get; private set; }


        public SwitcherController(
            ILogger<SwitcherController> logger,
            IConfiguration configuration,
            ExtronCommunicator ec)
        {
            this.logger = logger ?? throw new System.ArgumentNullException(nameof(logger));
            this.configuration = configuration ?? throw new System.ArgumentNullException(nameof(configuration));
            this.ec = ec ?? throw new System.ArgumentNullException(nameof(ec));
            this.Channels = ec.Channels;
            this.mappings = new Dictionary<string, string>();
            configuration.GetSection("ChannelMappings").Bind(this.mappings);
        }


        public IActionResult Index()
        {
            return View(CreateModel());
        }

        [Route("switch/{port}")]
        public async Task<IActionResult> Switch(int port)
        {
            ec.ChangeChannel(port);
            await Task.Delay(100);
            return RedirectToAction("");
        }

        [Route("Restart")]
        public async Task<IActionResult> Reset()
        {
            ec.CloseConnection();
            ec.OpenConnection();
            await Task.Delay(100);
            return RedirectToAction("");
        }


        private object CreateModel()
        {
            return new SwitcherModel
            {
                Mappings = mappings,
                ActiveChannel = ec.VideoChannel,
                Channels = ec.Channels
            };
        }


    }
}