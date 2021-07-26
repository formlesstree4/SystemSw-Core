using Moq;
using System;
using Xunit;

using SystemSw_Core.Extron.Devices;
using SystemSw_Core.Extron;
using System.Threading.Tasks;

namespace SystemSw_Tests
{
    public class ExtronCommunicatorTests
    {
        [Fact]
        public void Test_OpenWhenFlagIsSetToTrue()
        {
            var isOpened = false;
            var icd = new Mock<ICommunicationDevice>();
            icd.Setup((c) => c.Open()).Callback(() => { isOpened = true; });
            var ec = new ExtronCommunicator(icd.Object, true);
            Assert.True(isOpened);
        }


        // [Fact]
        public async Task Test_VerificationStringIsParsedCorrectly()
        {
            var rnd = new Random();

            var channelCount = rnd.Next(1, 14);
            var vidChannel = rnd.Next(1, channelCount - 1);
            var audChannel = rnd.Next(1, channelCount - 1);
            
            var identifyString = $"V{vidChannel} A{audChannel} T1 P0 S0 Z0 R0 QSC1.11 QPC1.11 M{channelCount}";

            var icd = new Mock<ICommunicationDevice>();
            icd.Setup(c => c.ReadLine()).Returns(identifyString);
            
            var ec = new ExtronCommunicator(icd.Object, false);
            ec.OpenConnection(true);
            ec.Identify();


            Assert.Equal(vidChannel, ec.VideoChannel);
            Assert.Equal(audChannel, ec.AudioChannel);
            Assert.Equal(channelCount, ec.Channels);
        }
    }
}
