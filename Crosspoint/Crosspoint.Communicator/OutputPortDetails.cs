namespace Crosspoint.Communicator
{

    /// <summary>
    /// Stores details about what inputs currently map to a specific output
    /// </summary>
    public sealed class OutputPortDetails
    {

        /// <summary>
        /// Gets the Output port number
        /// </summary>
        public int Output { get; private set; }

        /// <summary>
        /// Gets the Video Input currently routed to this output
        /// </summary>
        /// <remarks>
        /// According to the manual, RGB and Video are used interchangeably
        /// </remarks>
        public int VideoInput { get; set; }

        /// <summary>
        /// Gets the audio input currently routed to this output
        /// </summary>
        public int AudioInput { get; set; }

        /// <summary>
        /// Gets or sets whether the audio is muted for this output
        /// </summary>
        /// <remarks>
        /// Currently not supported
        /// </remarks>
        public bool IsAudioMuted => false;

        /// <summary>
        /// Gets or sets whether the video is muted for this output
        /// </summary>
        /// <remarks>
        /// Currently not supported
        /// </remarks>
        public bool IsVideoMuted => false;


        public OutputPortDetails(int output)
        {
            Output = output;
        }

    }
}
