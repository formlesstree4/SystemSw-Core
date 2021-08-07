using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private readonly Regex identityRegex = new(@"V(\d+)X(\d+) A(\d+)X(\d+)", RegexOptions.Compiled);
        private readonly Regex tieOutputRegex = new(@"Out(\d+) In(\d+) (All|RGB|Vid|Aud)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Dictionary<int, OutputPortDetails> mappings = new();


        /// <summary>
        /// Gets the current number of video inputs
        /// </summary>
        public int Inputs { get; private set; }

        /// <summary>
        /// Gets the current number of audio inputs
        /// </summary>
        public int Outputs { get; private set; }

        /// <summary>
        /// Gets the current lock information for the front panel
        /// </summary>
        public LockModeEnum LockMode { get; private set; }

        /// <summary>
        /// Gets the underlying mappings dictionary
        /// </summary>
        /// <remarks>
        /// Manipulating this can result in potentially screwing up the <see cref="ExtronCrosspointCommunicator"/>
        /// </remarks>
        public Dictionary<int, OutputPortDetails> Mappings => mappings;

        /// <summary>
        /// Gets a value that indicates whether or not the Communicator class is ready
        /// </summary>
        public bool IsSystemReady { get; private set; } = false;
        


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


        /// <inheritdoc cref="ExtronDeviceCommunicatorBase{T}.OpenConnection"/>
        public override ICommunicationResult OpenConnection()
        {
            IsSystemReady = false;
            return base.OpenConnection();
        }

        /// <inheritdoc cref="ExtronDeviceCommunicatorBase{T}.HandleIncomingResponse(string, string)" />
        protected override void HandleIncomingResponse(string lastCommand, string response)
        {
            
            if (lastCommand.Equals("I", StringComparison.OrdinalIgnoreCase) && identityRegex.IsMatch(response))
            {
                HandleIdentifyString(response);
                return;
            }
            if (lastCommand.Equals("Q", StringComparison.OrdinalIgnoreCase))
            {
                FirmwareVersion = response;
                return;
            }
            if (lastCommand.Equals("X", StringComparison.OrdinalIgnoreCase) && int.TryParse(response, out var lm))
            {
                LockMode = (LockModeEnum)lm;
                return;
            }
            if (tieOutputRegex.IsMatch(response))
            {
                var matches = tieOutputRegex.Matches(response)[0].Groups;
                if (int.TryParse(matches[1].Value, out var intOutput) && int.TryParse(matches[2].Value, out var intInput))
                {
                    UpdateInternalMapping(intOutput, intInput, GetMappingEnum(matches[3].Value));
                }
                return;
            }

            if (
                lastCommand.EndsWith("&") ||
                lastCommand.EndsWith("%") ||
                lastCommand.EndsWith("&") ||
                lastCommand.EndsWith("$"))
            {
                var output = lastCommand[0..^2];
                var input = response;
                var mapping = GetMappingEnum(lastCommand.Last().ToString());

                if (int.TryParse(output, out var intOutput) && int.TryParse(input, out var intInput))
                {
                    UpdateInternalMapping(intOutput, intInput, mapping);
                }
                return;
            }


        }

        /// <summary>
        /// Maps an input to a specified output
        /// </summary>
        /// <param name="input">The input port to map from</param>
        /// <param name="output">The output port to map to</param>
        /// <param name="mapping">The type of mapping to apply</param>
        /// <returns><see cref="ICommunicationResult"/></returns>
        /// <remarks>
        /// If input is zero that is the same as effectively removing a tie
        /// </remarks>
        public ICommunicationResult MapInputToOutput(int input, int output, MappingTypeEnum mapping)
        {
            if (input < 0 || input > Inputs)
            {
                return CommunicationResult.Error("input out of range", ResultCode.Error, false);
            }
            if (output < 0 || output > Outputs)
            {
                return CommunicationResult.Error("output out of range", ResultCode.Error, false);
            }
            Write($"{input}*{output}{GetMappingCharacter(mapping)}");
            return CommunicationResult.Ok();
        }

        /// <summary>
        /// Waits until <see cref="ExtronCrosspointCommunicator"/> is ready to communicate with the attached device
        /// </summary>
        public void WaitUntilReady()
        {
            SpinWait.SpinUntil(() => IsSystemReady, Timeout.Infinite);
        }

        /// <summary>
        /// Asynchronously waits until <see cref="ExtronCrosspointCommunicator"/> is ready to communicate with the attached device
        /// </summary>
        /// <returns></returns>
        public async Task WaitUntilReadyAsync()
        {
            await Task.Run(WaitUntilReady);
        }


        private void UpdateInternalMapping(int output, int input, MappingTypeEnum mappingType)
        {
            // remember: zero for the input just means it's not tied
            // to anything and that's TOTALLY OKAY AND ACCEPTABLE.
            lock (this)
            {
                var mapping = mappings[output];
                switch (mappingType)
                {
                    case MappingTypeEnum.All:
                        mapping.AudioInput = input;
                        mapping.VideoInput = input;
                        break;
                    case MappingTypeEnum.Video:
                        mapping.VideoInput = input;
                        break;
                    case MappingTypeEnum.Audio:
                        mapping.AudioInput = input;
                        break;
                }
            }
        }

        private void HandleIdentifyString(string identifyString)
        {
            // The documentation states that there are an equal number of inputs
            // and outputs for the Crosspoint 450 Plus. If this changes, we can
            // sorta revert and deal with this. The mappings setup will become
            // pretty fucking awkward but that's quite alright
            var matches = identityRegex.Matches(identifyString)[0].Groups;
            Inputs = int.Parse(matches[1].Value);
            Outputs = int.Parse(matches[2].Value);
            SetupOutputMappings();
            _ = QuerySystemForPortStatus();
        }

        private void SetupOutputMappings()
        {
            for (var index = 1; index <= Outputs; index++)
            {
                var output = new OutputPortDetails(index);
                mappings.Add(index, output);
            }
        }

        private async Task QuerySystemForPortStatus()
        {
            for (var index = 1; index <= Outputs; index++)
            {
                Write($"{index}%");
                await Task.Delay(150);
                Write($"{index}$");
                await Task.Delay(150);
            }
            IsSystemReady = true;
        }

        private void QuerySystemForMuteStatus()
        {
            // I'll implement this if it becomes necessary
            // but I don't think anyone actually uses this
            // at the moment; perhaps one day
            throw new NotImplementedException();
        }


        private static string GetMappingCharacter(MappingTypeEnum mapping)
        {
            return mapping switch
            {
                MappingTypeEnum.All => "!",
                MappingTypeEnum.Audio => "$",
                MappingTypeEnum.Video => "%",
                _ => throw new ArgumentOutOfRangeException(nameof(mapping)),
            };
        }

        private static MappingTypeEnum GetMappingEnum(string mappingType)
        {
            return mappingType.ToLowerInvariant() switch
            {
                "all" => MappingTypeEnum.All,
                "!" => MappingTypeEnum.All,
                
                "rgb" => MappingTypeEnum.Video,
                "vid" => MappingTypeEnum.Video,
                "&" => MappingTypeEnum.Video,
                "%" => MappingTypeEnum.Video,

                "aud" => MappingTypeEnum.Audio,
                "$" => MappingTypeEnum.Audio,
                _ => throw new ArgumentOutOfRangeException(nameof(mappingType)),
            };
        }

    }

}
