using Crosspoint.Communicator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using System;
using System.Threading;
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
        public void Fact_ProperlyIdentifyVideoAudioMatrix()
        {
            var rand = new Random();
            var input = rand.Next(4, 32);
            var output = rand.Next(4, 16);

            var identify = $"V{input}X{output} A{input}X{output}";
            var ecc = new ExtronCrosspointCommunicator(new Fakes.FakeCommunicationDevice(identify), logger, GetConfiguration(false));
            ecc.OpenConnection();
            SpinWait.SpinUntil(() => ecc.IsSystemReady, Timeout.Infinite);
            Assert.Equal(input, ecc.Inputs);
            Assert.Equal(output, ecc.Outputs);
        }


        [Theory(DisplayName = "Map input to output with appropriate type")]
        [InlineData(1, 1, MappingTypeEnum.All),
            InlineData(1, 1, MappingTypeEnum.Audio),
            InlineData(1, 1, MappingTypeEnum.Video)]
        public async Task Theory_MapInputToOutput(int input, int output, MappingTypeEnum type)
        {
            var ecc = new ExtronCrosspointCommunicator(new Fakes.FakeCommunicationDevice(), logger, GetConfiguration(false));
            ecc.OpenConnection();
            SpinWait.SpinUntil(() => ecc.IsSystemReady, Timeout.Infinite);
            Assert.Equal(0, ecc.Mappings[output].VideoInput);
            Assert.Equal(0, ecc.Mappings[output].AudioInput);
            ecc.MapInputToOutput(input, output, type);
            await Task.Delay(150);

            switch (type)
            {
                case MappingTypeEnum.All:
                    Assert.Equal(input, ecc.Mappings[output].VideoInput);
                    Assert.Equal(input, ecc.Mappings[output].AudioInput);
                    break;
                case MappingTypeEnum.Video:
                    Assert.Equal(input, ecc.Mappings[output].VideoInput);
                    break;
                case MappingTypeEnum.Audio:
                    Assert.Equal(input, ecc.Mappings[output].AudioInput);
                    break;
            }
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
