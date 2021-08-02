using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using SystemCommunicator.Communication;
using SystemCommunicator.Communication.Results;
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

        /// <summary>
        /// Gets the current number of inputs
        /// </summary>
        public int Inputs { get; private set; }

        /// <summary>
        /// Gets the current number of outputs
        /// </summary>
        public int Outputs { get; private set; }



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

        /// <inheritdoc cref="ExtronDeviceCommunicatorBase{T}.HandleIncomingResponse(string)"/>
        protected override void HandleIncomingResponse(string response)
        {
            throw new NotImplementedException();
        }

    }

}
