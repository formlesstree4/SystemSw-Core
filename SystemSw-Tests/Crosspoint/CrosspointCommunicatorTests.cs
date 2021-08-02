using Crosspoint.Communicator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading.Tasks;
using Xunit;

namespace SystemSw.Sharp.Tests.Crosspoint
{
    public class CrosspointCommunicatorTests
    {

        private readonly ILogger<ExtronCrosspointCommunicator> logger;


        public CrosspointCommunicatorTests()
        {
            logger = new NullLogger<ExtronCrosspointCommunicator>();
        }


        [Fact(DisplayName = "When the 'Identify' string is properly parsed, the correct audio and video matrix mappings will be present")]
        public async Task Fact_ProperlyIdentifyVideoAudioMatrix()
        {
            var rand = new Random();
            var input = rand.Next(4, 32);
            var output = rand.Next(4, 16);

            var identify = $"V{input}X{output} A{input}X{output}";
            var ecc = new ExtronCrosspointCommunicator(new Fakes.FakeCommunicationDevice(identify), logger, GetConfiguration(false));
            ecc.OpenConnection();
            await Task.Delay(150);
            Assert.Equal(input, ecc.VideoInputs);
            Assert.Equal(input, ecc.AudioInputs);
            Assert.Equal(output, ecc.VideoOutputs);
            Assert.Equal(output, ecc.AudioOutputs);
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
