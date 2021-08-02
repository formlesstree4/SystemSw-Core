using System.Net;

namespace SystemCommunicator.Configuration
{

    /// <summary>
    /// Defines the object structure for the 'telnet' communication settings
    /// </summary>
    public sealed class TelnetCommunicationSettings : CommunicationSettingsBase
    {

        /// <summary>
        /// Gets or sets the remote IP address to attempt a connection to
        /// </summary>
        public string IpAddress { get; set; }

        /// <summary>
        /// Gets or sets the remote Port to attempt a connection to
        /// </summary>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets the Username used to login to the telnet device
        /// </summary>
        public string Username { get; set; }

        /// <summary>
        /// Gets or sets the Password used to login to the telnet device
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// Gets the remote connection as an instance of <see cref="IPEndPoint"/>
        /// </summary>
        /// <remarks>
        /// This invokes <see cref="IPEndPoint.Parse(string)"/> under the hood and will throw the appropriate exceptions if the address and port combinations could not be parsed
        /// </remarks>
        public IPEndPoint AsEndpoint => IPEndPoint.Parse($"{IpAddress}:{Port}");

    }

}
