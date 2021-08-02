using SystemCommunicator.Devices;

namespace SystemCommunicator.Communication
{

    /// <summary>
    /// Defines the base line necessary interactions for communicating with an Extron system device
    /// </summary>
    /// <remarks>
    /// Given how the Extron systems work, there is almost never a synchronous response.
    /// The information returned by <see cref="ICommunicationResult"/> is moreso about the actual request being submitted
    /// and not the response being received after the fact. The way most of the communication classes work (so far) is that
    /// there is an internal reader that is constantly listening for responses / replies over the <see cref="ICommunicationDevice"/>
    /// and then the class reacts to them. This means that reading and writing happen at different times.
    /// </remarks>
    public interface IExtronDeviceCommunicator
    {

        /// <summary>
        /// Gets the current firmware version of the connected device
        /// </summary>
        string FirmwareVersion { get; }

        /// <summary>
        /// Gets whether or not there is currently an active and open connection to the Extron device
        /// </summary>
        bool IsConnectionOpen { get; }

        /// <summary>
        /// Gets the <see cref="ICommunicationDevice"/> used to talk to the Extron device
        /// </summary>
        ICommunicationDevice CommunicationDevice { get; }



        /// <summary>
        /// Attempts to open the connection to the Extron device
        /// </summary>
        /// <returns><see cref="ICommunicationResult"/></returns>
        ICommunicationResult OpenConnection();

        /// <summary>
        /// Attempts to close the connection to the Extron device
        /// </summary>
        /// <returns><see cref="ICommunicationResult"/></returns>
        ICommunicationResult CloseConnection();

        /// <summary>
        /// Attempts to perform an identification on the Extron device
        /// </summary>
        /// <returns><see cref="ICommunicationResult"/></returns>
        ICommunicationResult Identify();

    }

}
