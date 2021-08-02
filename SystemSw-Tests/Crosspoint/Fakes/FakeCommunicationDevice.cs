using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SystemCommunicator.Devices;

namespace SystemSw.Sharp.Tests.Crosspoint.Fakes
{

    /// <summary>
    /// Fake Communication device for testing the Crosspoint Communicator
    /// </summary>
    public sealed class FakeCommunicationDevice : ICommunicationDevice
    {

        private string response = "";
        private bool isDisposed = false;
        private readonly string Identify = "V2X2 A2X2";
        private readonly Regex tieRegexInput = new(@"(\d+)\*(\d+)(!|&|%|\$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex tieRegexOutput = new(@"(\d+)(!|&|%|\$)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public bool IsOpen { get; private set; }


        public FakeCommunicationDevice(string verify = null)
        {
            Identify = verify ?? Identify;
        }


        public void Close()
        {
            AssertDisposed();
            IsOpen = false;
        }

        public void Dispose()
        {
            AssertDisposed();
            IsOpen = false;
            isDisposed = true;
        }

        public void Open()
        {
            AssertDisposed();
            IsOpen = true;
        }

        public string ReadLine()
        {
            AssertDisposed();
            SpinWait.SpinUntil(() => !string.IsNullOrEmpty(response), Timeout.Infinite);
            var r = response;
            response = "";
            return r;
        }

        public void Write(string text)
        {
            AssertDisposed();
            switch(text[0])
            {
                case 'I':
                case 'i':
                    response = Identify;
                    break;
                case 'Q':
                case 'q':
                    response = "1.337";
                    break;
            }

            if (tieRegexInput.IsMatch(text))
            {
                var matches = tieRegexInput.Match(text).Groups;
                if (int.TryParse(matches[1].Value, out var input) && int.TryParse(matches[2].Value, out var output))
                {
                    response = $"Out{output} In{input} {MapToEnglish(matches[3].Value)}";
                }
                return;
            }

            if (tieRegexOutput.IsMatch(text))
            {
                response = "00";
            }

            return;
        }

        private void AssertDisposed()
        {
            if (isDisposed) throw new System.ObjectDisposedException($"{nameof(FakeCommunicationDevice)}");
        }

        private static string MapToEnglish(string input)
        {
            return input switch
            {
                "!" => "All",
                "&" => "RGB",
                "%" => "Vid",
                "$" => "Aud",
                _ => ""
            };
        }

    }
}
