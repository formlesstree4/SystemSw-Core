namespace SystemCommunicator.Communication
{

    /// <summary>
    /// All possible communication results
    /// </summary>
    public enum ResultCode
    {

        /// <summary>
        /// An unknown error has occurred
        /// </summary>
        Unknown = -1,

        /// <summary>
        /// The request was submitted successfully
        /// </summary>
        Success = 0,

        /// <summary>
        /// The command request attempted is not supported on this specific device
        /// </summary>
        NotSupported = 100,

        /// <summary>
        /// The command request attempted is not implemented in this class
        /// </summary>
        NotImplemented = 101,

        /// <summary>
        /// A generic error has occurred and more details will be provided in the error message
        /// </summary>
        Error = 102,

        /// <summary>
        /// The target device is either not connected or the connection has not been opened
        /// </summary>
        DeviceNotConnected = 200,

        /// <summary>
        /// The target device could not be read before the timeout
        /// </summary>
        ReadTimeout = 300,

        /// <summary>
        /// The target device could not be written to before the timeout
        /// </summary>
        WriteTimeout = 400

    }

}
