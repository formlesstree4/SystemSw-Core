using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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
        private readonly string Identify = "V32X16 A32X16";

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
            }
            return;
        }


        private void AssertDisposed()
        {
            if (isDisposed) throw new System.ObjectDisposedException($"{nameof(FakeCommunicationDevice)}");
        }

    }
}
