namespace SystemCommunicator.Configuration
{

    /// <summary>
    /// Defines the object structure for the 'Serial' communication settings
    /// </summary>
    public sealed class SerialCommunicationSettings : CommunicationSettingsBase
    {

        /// <summary>
        /// Gets or sets the name of the serial port
        /// </summary>
        public string Port { get; set; }

    }

}
