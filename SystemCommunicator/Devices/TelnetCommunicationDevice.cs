using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
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
        }

        public string ReadLine()
        {
            logger.LogTrace("ReadLine()");
            using var sr = new StreamReader(telnetClient.GetStream(), leaveOpen: true);
            return sr.ReadLine();
        }

        public void Write(string text)
        {
            logger.LogTrace($"Write('{text}')");
            using var sw = new StreamWriter(telnetClient.GetStream(), leaveOpen: true);
            sw.WriteLine(text);
        }

        private static TelnetCommunicationSettings GetSettings(IConfiguration configuration)
        {
            var tcs = new TelnetCommunicationSettings();
            configuration?.GetSection("Extron")?.GetSection("Telnet")?.Bind(tcs);
            return tcs;
        }

    }

}
