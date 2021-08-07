using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using SystemCommunicator.Configuration;

namespace SystemCommunicator.Devices
{

    /// <summary>
    /// An implementation of <see cref="ICommunicationDevice"/> that uses a TCP connection
    /// </summary>
    public sealed class TelnetCommunicationDevice : ICommunicationDevice
    {

        private readonly TcpClient telnetClient;
        private readonly string ipAddress;
        private readonly int port;
        private readonly string password;
        private readonly ILogger<TelnetCommunicationDevice> logger;
        private readonly IPEndPoint endpoint;


        public bool IsOpen { get; private set; }

        /// <summary>
        /// Creates a new instance of <see cref="TelnetCommunicationDevice"/>
        /// </summary>
        /// <param name="configuration">Instance of <see cref="IConfiguration"/> that contains a configuration node for the <see cref="TelnetCommunicationDevice"/></param>
        /// <param name="logger">A logger implementation specifically for <see cref="TelnetCommunicationDevice"/></param>
        public TelnetCommunicationDevice(
            IConfiguration configuration,
            ILogger<TelnetCommunicationDevice> logger)
        {
            var settings = GetSettings(configuration);
            this.logger = logger;
            this.ipAddress = settings.IpAddress;
            this.port = settings.Port;
            this.password = settings.Password;
            if (!IPEndPoint.TryParse($"{ipAddress}:{port}", out var ip))
            {
                logger.LogError($"Could not parse {ipAddress}:{port} into an instance of ${nameof(System.Net.IPEndPoint)}");
            }
            this.endpoint = ip;
            telnetClient = new TcpClient();
        }


        public void Close()
        {
            logger.LogTrace("Closing Connection");
            telnetClient.Close();
            IsOpen = false;
        }

        public void Dispose()
        {
            logger.LogTrace("Dispose()");
            telnetClient.Dispose();
        }

        public void Open()
        {
            logger.LogTrace("Opening Connection");
            telnetClient.Connect(endpoint);
            HandleAuthentication();
            IsOpen = true;
        }

        public string ReadLine()
        {
            logger.LogTrace("ReadLine()");
            try
            {
                using var sr = new StreamReader(telnetClient.GetStream(), leaveOpen: true);
                return sr.ReadLine();
            }
            catch (InvalidOperationException ioe)
            {
                // we actually might be hosed here! The connection has been lost
                logger.LogError(ioe, "the connection with the remote server has been lost");
                Close();
                return "";
            }
        }

        public void Write(string text)
        {
            logger.LogTrace($"Write('{text}')");
            using var sw = new StreamWriter(telnetClient.GetStream(), leaveOpen: true);
            sw.WriteLine(text);
        }


        private void HandleAuthentication()
        {
            if (!string.IsNullOrEmpty(password))
            {
                Write(password);
            }
        }

        private static TelnetCommunicationSettings GetSettings(IConfiguration configuration)
        {
            var tcs = new TelnetCommunicationSettings();
            configuration?.GetSection("Extron")?.GetSection("Telnet")?.Bind(tcs);
            return tcs;
        }

    }

}
