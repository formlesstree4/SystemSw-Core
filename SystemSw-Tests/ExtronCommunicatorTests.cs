using Moq;
using System;
using Xunit;

using SystemSw_Core.Extron.Devices;
using SystemSw_Core.Extron;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace SystemSw_Tests
{
    public class ExtronCommunicatorTests
    {

        private readonly ILogger<ExtronCommunicator> logger;


        public ExtronCommunicatorTests()
        {
            logger = new NullLogger<ExtronCommunicator>();
        }


        [Fact(DisplayName = "When 'IsOpen' is true (in CFG) the device should be opened")]
        public void Test_OpenWhenFlagIsSetToTrue()
        {
            var isOpened = false;
            var icd = new Mock<ICommunicationDevice>();
            icd.Setup((c) => c.Open()).Callback(() => { isOpened = true; });
            var ec = new ExtronCommunicator(icd.Object, logger, GetConfiguration(true));
            Assert.True(isOpened);
            Assert.True(ec.IsConnectionOpen);
            ec.Dispose();
        }

        [Fact(DisplayName = "When 'IsOpen' is false (in CFG) the device should not be opened")]
        public void Test_ClosedWhenFlagIsSetToFalse()
        {
            var ec = new ExtronCommunicator(new Fakes.FakeCommunicationDevice(), logger, GetConfiguration(false));
            Assert.False(ec.IsConnectionOpen);
        }

        [Fact(DisplayName = "Verification string is parsed accordingly for System 8/10")]
        public async Task Test_VerificationStringIsParsedCorrectly()
        {
            var rnd = new Random();
            var channelCount = rnd.Next(8, 11);
            var vidChannel = rnd.Next(1, channelCount - 1);
            var audChannel = rnd.Next(1, channelCount - 1);
            var qscVersion = rnd.Next(10, 99);
            var qpcVersion = rnd.Next(10, 99);
            var identifyString = $"V{vidChannel} A{audChannel} T1 P0 S0 Z0 R0 QSC1.{qscVersion} QPC1.{qpcVersion} M{channelCount}";
            var icd = new Fakes.FakeCommunicationDevice(identifyString);
            var ec = new ExtronCommunicator(icd, logger, GetConfiguration(false));
            
            ec.OpenConnection(true);
            ec.Identify();

            await Task.Delay(100);

            Assert.Equal(vidChannel, ec.VideoChannel);
            Assert.Equal(audChannel, ec.AudioChannel);
            Assert.Equal(channelCount, ec.Channels);
            Assert.Equal($"1.{qscVersion}", ec.SwitcherFirmwareVersion);
            Assert.Equal($"1.{qpcVersion}", ec.ProjectorFirmwareVersion);

            ec.Dispose();
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
