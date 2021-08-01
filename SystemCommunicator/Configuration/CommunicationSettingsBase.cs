namespace SystemCommunicator.Configuration
{

    /// <summary>
    /// The root of all communication configuration options
    /// </summary>
    public abstract class CommunicationSettingsBase
    {

        /// <summary>
        /// Gets or sets the amount of milliseconds the communication device should wait on reading before timing out
        /// </summary>
        public int ReadTimeout { get; set; }

        /// <summary>
        /// Gets or sets the amount of milliseconds the communication device should wait on writing before timing out
        /// </summary>
        public int WriteTimeout { get; set; }

        /// <summary>
        /// Gets or sets whether the communication device should automatically open the connection when instantiated
        /// </summary>
        public bool AutoOpen { get; set; }

    }

}
