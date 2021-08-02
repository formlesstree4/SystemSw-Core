using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Text.RegularExpressions;
using SystemCommunicator.Communication;
using SystemCommunicator.Devices;

namespace Crosspoint.Communicator
{

    /// <summary>
    /// A communication class for the Extron Crosspoint or Crosspoint-compatible devices
    /// </summary>
    /// <remarks>
    /// Unlike the System (4, 8, 10) Series of devices, the Crosspoint allows for mapping to one or more outputs for all the various inputs.
    /// As such, you can have quite versatile mappings setup and configured (input 1 mapping video to outputs 2, 3, 4 and mapping audio to 1).
    /// 
    /// The Crosspoint also allows for communicating via telnet (which led to the creation of the Telent communication class)
    /// </remarks>
    public sealed class ExtronCrosspointCommunicator : ExtronDeviceCommunicatorBase<ExtronCrosspointCommunicator>
    {
        private readonly Regex identityRegex = new(@"V(\d+)X(\d+) A(\d+)X(\d+)", RegexOptions.Compiled);



        /// <summary>
        /// Gets the current number of video inputs
        /// </summary>
        public int VideoInputs { get; private set; }

        /// <summary>
        /// Gets the current number of audio inputs
        /// </summary>
        public int AudioInputs { get; private set; }

        /// <summary>
        /// Gets the current number of video outputs
        /// </summary>
        public int VideoOutputs { get; private set; }

        /// <summary>
        /// Gets the current number of audio outputs
        /// </summary>
        public int AudioOutputs { get; private set; }


        /// <summary>
        /// Creates a new instance of the <see cref="ExtronCrosspointCommunicator"/>
        /// </summary>
        /// <param name="com">The device interface to communicate with</param>
        /// <param name="logger">Logging class</param>
        /// <param name="configuration">The configuration class</param>
        public ExtronCrosspointCommunicator(
            ICommunicationDevice com,
            ILogger<ExtronCrosspointCommunicator> logger,
            IConfiguration configuration) : base(com, logger, configuration) { }

        /// <inheritdoc cref="ExtronDeviceCommunicatorBase{T}.HandleIncomingResponse(string, string)" />
        protected override void HandleIncomingResponse(string lastCommand, string response)
        {
            
            // see if this is an information string
            if (lastCommand.Equals("I", StringComparison.OrdinalIgnoreCase) && identityRegex.IsMatch(response))
            {
                HandleIdentifyString(response);
                return;
            }

        }


        private void HandleIdentifyString(string identifyString)
        {
            var matches = identityRegex.Matches(identifyString)[0].Groups;
            VideoInputs = int.Parse(matches[1].Value);
            VideoOutputs = int.Parse(matches[2].Value);
            AudioInputs = int.Parse(matches[3].Value);
            AudioOutputs = int.Parse(matches[4].Value);
        }

    }

}
