using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SystemSw_Core.Extron.Devices;

namespace SystemSw_Tests.Fakes
{
    public sealed class FakeCommunicationDevice : ICommunicationDevice
    {

        private const string qsc = "1.99";
        private const string qpc = "1.99";
        private const int max = 4;
        private readonly string Identify = $"V1 A1 T1 P0 S0 Z0 R0 QSC{qsc} QPC{qpc} M{max}";

        private string response = "";
        private bool isDisposed = false;

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

        public void Dispose() => isDisposed = true;

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
            switch (text[0])
            {
                case 'I':
                case 'i':
                    response = Identify;
                    break;
                case 'Q':
                case 'q':
                    response = $"QSC{qsc}\r\nQPC{qpc}";
                    break;
                case 'b':
                    response = "MUT 1";
                    break;
                case 'B':
                    response = "MUT 0";
                    break;
                case '+':
                    response = "AMUT 1";
                    break;
                case '-':
                    response = "AMUT 0";
                    break;
                case '[':
                case ']':
                case '(':
                case ')':
                    break;
            }
            if (text.Length <= 1) return;

            switch (text.Last())
            {
                case '!':
                    response = $"C{text[0..^1]}";
                    break;
                case '&':
                    response = $"V{text[0..^1]}";
                    break;
                case '$':
                    response = $"A{text[0..^1]}";
                    break;
            }
        }


        private void AssertDisposed()
        {
            if (isDisposed) throw new System.ObjectDisposedException($"{nameof(TestCommDevice)}");
        }

    }
}
