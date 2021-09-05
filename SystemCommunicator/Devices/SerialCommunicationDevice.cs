using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.IO.Ports;
using SystemCommunicator.Configuration;

namespace SystemCommunicator.Devices
{

    /// <summary>
    /// An implementation of <see cref="ICommunicationDevice"/> that uses a serial port
    /// </summary>
    public sealed class SerialCommunicationDevice : ICommunicationDevice
    {

        private readonly SerialPort sp;
        private readonly ILogger<SerialCommunicationDevice> logger;
        private bool shouldBeOpen = false;


        public bool IsOpen => sp.IsOpen && shouldBeOpen;


        /// <summary>
        /// Creates a new instance of the <see cref="SerialCommunicationDevice"/> 
        /// </summary>
        /// <param name="configuration">Instance of <see cref="IConfiguration"/> that contains a configuration node for the <see cref="SerialCommunicationDevice"/></param>
        /// <param name="logger">A logger implementation specifically for <see cref="SerialCommunicationDevice"/></param>
        public SerialCommunicationDevice(
            IConfiguration configuration,
            ILogger<SerialCommunicationDevice> logger)
        {
            this.logger = logger;
            var settings = GetSettings(configuration);
            logger.LogInformation($"AutoOpen: {settings.AutoOpen}; Port: {settings.Port}");
            sp = new SerialPort(settings.Port)
            {
                BaudRate = 9600,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Handshake = Handshake.None,
                DtrEnable = true,
                RtsEnable = false,
                ReadTimeout = settings.ReadTimeout,
                WriteTimeout = settings.WriteTimeout
            };
        }


        public void Close()
        {
            logger.LogInformation("Closing Connection");
            shouldBeOpen = false;
            sp.Close();
        }

        public void Dispose()
        {
            logger.LogInformation("Dispose()");
            sp.Dispose();
        }

        public void Open()
        {
            logger.LogInformation("Opening Connection");
            shouldBeOpen = true;
            sp.Open();
        }

        public string ReadLine()
        {
            logger.LogInformation("ReadLine()");
            return sp.ReadLine();
        }

        public void Write(string text)
        {
            logger.LogInformation($"Write('{text}')");
            sp.Write(text);
        }

        private static SerialCommunicationSettings GetSettings(IConfiguration configuration)
        {
            var scs = new SerialCommunicationSettings();
            configuration?.GetSection("Extron")?.GetSection("Serial")?.Bind(scs);
            return scs;
        }

    }

}
