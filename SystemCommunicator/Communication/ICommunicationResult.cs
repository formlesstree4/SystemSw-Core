namespace SystemCommunicator.Communication
{

    /// <summary>
    /// Defines a type of response that is returned to the caller
    /// </summary>
    public interface ICommunicationResult
    {

        /// <summary>
        /// Gets whether the request was a successful submission.
        /// </summary>
        bool Result { get; }

        /// <summary>
        /// Gets the actual status of the communication request
        /// </summary>
        ResultCode Status { get; }

        /// <summary>
        /// Gets any specialized error message that would provide more details if <see cref="Status"/> was not <see cref="ResultCode.Success"/>
        /// </summary>
        string Message { get; }

    }

}
