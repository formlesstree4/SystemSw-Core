using System.Collections.Generic;
using System.IO.Ports;

namespace SystemSw_Core.Extron.Devices
{

    /// <summary>
    /// An implementation of <see cref="ICommunicationDevice"/> that uses a serial port
    /// </summary>
    public sealed class SerialCommunicationDevice : ICommunicationDevice
    {
        
        private readonly SerialPort sp;
        private readonly Stack<string> history;

        public bool IsOpen => sp.IsOpen;



        /// <summary>
        /// Creates a new instance of the SerialCommunicationDevice 
        /// </summary>
        /// <param name="serialPort">The name of the serial port to attach to</param>
        /// <param name="open">Automatically open the serial port after instantiation</param>
        /// <param name="readTimeout">The time, in milliseconds, to timeout the read</param>
        public SerialCommunicationDevice(
            string serialPort,
            bool open = false,
            int readTimeout = 1000)
        {
            sp = new SerialPort(serialPort)
            {
                BaudRate = 9600,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Handshake = Handshake.None,
                DtrEnable = true,
                RtsEnable = false
            };
            if (open) Open();
        }


        public void Close() => sp.Close();

        public void Dispose() => sp.Dispose();

        public void Open() => sp.Open();

        public string ReadLine() => sp.ReadLine();

        public void Write(string text) => sp.Write(text);
    }
}