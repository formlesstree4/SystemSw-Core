using Crosspoint.Communicator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SystemSw.Sharp.Tests.Crosspoint
{
    public class CrosspointCommunicatorTests
    {

        private readonly ILogger<ExtronCrosspointCommunicator> logger;


        public CrosspointCommunicatorTests()
        {
            logger = new NullLogger<ExtronCrosspointCommunicator>();
        }


        private static IConfiguration GetConfiguration(bool autoload)
        {
            var filename = autoload ? "appsettings.json" : "appsettings.noautoload.json";
            return new ConfigurationBuilder()
                .AddJsonFile(filename, false, true)
                .Build();
        }

    }

}
