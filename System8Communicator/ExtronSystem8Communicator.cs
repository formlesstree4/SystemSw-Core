using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using SystemCommunicator.Communication;
using SystemCommunicator.Communication.Results;
using SystemCommunicator.Devices;

namespace System8.Communicator
{

    /// <summary>
    /// A communication class for the Extron System 8 and System 10 PLUS
    /// </summary>
    /// <remarks>
    /// The code written here was based off the RS-232 programming guide located on pages A-3, A-4, and A-5.
    /// I will not be providing a copy of the manual as it can be found on Extron's website.
    /// </remarks>
    public sealed class ExtronSystem8Communicator : IExtronDeviceCommunicator, IDisposable
    {
        private CancellationTokenSource source;
        private Task readLoop;
        private readonly ICommunicationDevice com;
        private readonly ILogger<ExtronSystem8Communicator> logger;
        private Action<string> errorHandler;


        /// <summary>
        /// Gets the number of channels that the connected system supports
        /// </summary>
        public int Channels { get; private set; } = 0;

        /// <summary>
        /// Gets the current channel the video feed is playing on
        /// </summary>
        public int VideoChannel { get; private set; }

        /// <summary>
        /// Gets the current channel the audio feed is playing on
        /// </summary>
        public int AudioChannel { get; private set; }

        /// <summary>
        /// Gets the current output details for the connected system
        /// </summary>
        public VideoTypeEnum VideoType { get; private set; } = VideoTypeEnum.Unknown;

        /// <summary>
        /// Gets the current display power status flag
        /// </summary>
        /// <remarks>
        /// True - The display power status is on
        /// False - The display power status is off
        /// </remarks>
        public bool IsProjecterPowered { get; private set; }

        /// <summary>
        /// Gets the current display mute status flag
        /// </summary>
        /// <remarks>
        /// True - The display mute status is on
        /// False - The display mute status is off
        /// </remarks>
        public bool IsProjectorMuted { get; private set; }

        /// <summary>
        /// Gets whether or not the audio is muted
        /// </summary>
        public bool IsAudioMuted { get; private set; }

        /// <summary>
        /// Gets whether or not the RGB spectrum has been muted
        /// </summary>
        public bool IsRgbMuted { get; private set; }

        /// <summary>
        /// Gets the firmware version of the onboard system chip in the Extron that controls the Extron internals
        /// </summary>
        public string SwitcherFirmwareVersion { get; private set; }

        /// <summary>
        /// Gets the firmware version of the onboard system chip in the Extron that controls the projector features
        /// </summary>
        public string ProjectorFirmwareVersion { get; private set; }

        /// <summary>
        /// Gets whether or not the current connection is open
        /// </summary>
        public bool IsConnectionOpen => com.IsOpen;

        public string FirmwareVersion => SwitcherFirmwareVersion;

        public ICommunicationDevice CommunicationDevice => com;



        /// <summary>
        /// Creates a new instance of the <see cref="ExtronSystem8Communicator"/> for the given serial port
        /// </summary>
        /// <param name="com">The device interface to communicate with</param>
        /// <param name="open">Immediately invoke <see cref="OpenConnection"/> if true</param>
        public ExtronSystem8Communicator(
            ICommunicationDevice com,
            ILogger<ExtronSystem8Communicator> logger,
            IConfiguration configuration
            )
        {
            this.com = com ?? throw new ArgumentNullException(nameof(com));
            this.logger = logger;
            this.HandleAutoOpen(configuration);
        }



        /// <summary>
        /// Attempts to open the connection
        /// </summary>
        /// <param name="doNotIdentify">If set to true, will not run Identify</param>
        /// <exception cref="System.UnauthorizedAccessException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        /// <exception cref="System.ArgumentException"/>
        /// <exception cref="System.InvalidOperationException"/>
        /// <exception cref="System.IO.IOException"/>
        public ICommunicationResult OpenConnection(bool doNotIdentify = false)
        {
            if (com.IsOpen) return CommunicationResult.Error("connection already open", ResultCode.Unknown, false);
            com.Open();
            source = new CancellationTokenSource();
            readLoop = Task.Run(InternalReadLoop);
            if (!doNotIdentify) Identify();
            return CommunicationResult.Ok();
        }

        /// <summary>
        /// Attempts to open the connection to the Extron device
        /// </summary>
        /// <returns><see cref="ICommunicationResult"/></returns>
        public ICommunicationResult OpenConnection() => OpenConnection(false);

        /// <summary>
        /// Attempts to close the connection
        /// </summary>
        public ICommunicationResult CloseConnection()
        {
            com.Close();
            source.Cancel();
            readLoop.Dispose();
            return CommunicationResult.Ok();
        }

        /// <summary>
        /// Identifies what the connected system is and sets up initial settings.
        /// </summary>
        /// <returns>A promise to indicate if identification was successful</returns>
        public ICommunicationResult Identify()
        {
            Write(command: "I");
            return CommunicationResult.Ok();
        }

        /// <summary>
        /// Changes the audio and video channels
        /// </summary>
        /// <param name="channel">The channel to change to</param>
        /// <returns>A promise to indicate if it was successful or not</returns>
        public ICommunicationResult ChangeChannel(int channel)
        {
            if (channel < 0 || channel > Channels)
            {
                return CommunicationResult.Error("out of range", ResultCode.Error, false);
            }

            Write($"{channel}!");
            return CommunicationResult.Ok();
        }

        /// <summary>
        /// Changes the video channel
        /// </summary>
        /// <param name="channel">The channel to change to</param>
        /// <returns>A promise to indicate if it was successful or not</returns>
        public ICommunicationResult ChangeVideoChannel(int channel)
        {
            if (channel < 0 || channel > Channels)
            {
                return CommunicationResult.Error("out of range", ResultCode.Error, false);
            }

            Write($"{channel}&");
            return CommunicationResult.Ok();
        }

        /// <summary>
        /// Changes the audio channel
        /// </summary>
        /// <param name="channel">The channel to change to</param>
        /// <returns>A promise to indicate if it was successful or not</returns>
        public ICommunicationResult ChangeAudioChannel(int channel)
        {
            if (channel < 0 || channel > Channels)
            {
                return CommunicationResult.Error("out of range", ResultCode.Error, false);
            }

            Write($"{channel}$");
            return CommunicationResult.Ok();
        }

        /// <summary>
        /// Updates the software version numbers for the Extron
        /// </summary>
        public ICommunicationResult GetSoftwareVersions()
        {
            Write("q");
            return CommunicationResult.Ok();
        }

        /// <summary>
        /// Sets the projector power if it is connected
        /// </summary>
        /// <param name="powered">true to set power on</param>
        public ICommunicationResult SetProjectorPower(bool powered)
        {
            Write(powered ? "[" : "]");
            return CommunicationResult.Ok();
        }

        /// <summary>
        /// Sets the projector visibility if it is connected
        /// </summary>
        /// <param name="visible">true to set visibility to on</param>
        public ICommunicationResult SetProjectorVisibility(bool visible)
        {
            Write(visible ? ")" : "(");
            return CommunicationResult.Ok();
        }

        /// <summary>
        /// Set the RGB visibility state
        /// </summary>
        /// <param name="visible">true to set visibility to on</param>
        public ICommunicationResult SetRgbVisibility(bool visible)
        {
            Write(visible ? "b" : "B");
            return CommunicationResult.Ok();
        }

        /// <summary>
        /// Sets whether or not the audio line is muted or not
        /// </summary>
        /// <param name="muted">true to mute</param>
        public ICommunicationResult SetAudioState(bool muted)
        {
            Write(muted ? "+" : "-");
            return CommunicationResult.Ok();
        }

        public void Dispose()
        {
            com.Dispose();
        }

        /// <summary>
        /// Registers a handler to process when an error code is raised. Multiple handlers can be registered as a callback
        /// </summary>
        /// <param name="errorHandler">The error handler that will take the error message</param>
        public void RegisterErrorCallback(Action<string> errorHandler)
        {
            if (this.errorHandler is null)
            {
                this.errorHandler = errorHandler;
            }
            else
            {
                this.errorHandler += errorHandler;
            }
        }

        /// <summary>
        /// Polls the serial port for incoming data
        /// </summary>
        private void InternalReadLoop()
        {
            while (!source.IsCancellationRequested)
            {
                try
                {
                    var dataLine = com.ReadLine();
                    if (!string.IsNullOrEmpty(dataLine))
                    {
                        HandleIncomingResponse(dataLine);
                    }
                }
                catch (TimeoutException)
                {
                    // no worries  
                }
                catch (Exception ex)
                {
                    // something has gone horribly wrong [!]
                    logger.LogError(ex, "An error occurred reading from the stream");
                }
            }
        }

        /// <summary>
        /// Handles the incoming response message
        /// </summary>
        /// <param name="response">The response string to interpret</param>
        /// <remarks>
        /// This is not an exact implementation of every response code that the Extron system can send back. Since I currently do not use all the features
        /// </remarks>
        private void HandleIncomingResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response) || string.IsNullOrEmpty(response)) return;
            if (IsResponseError(response))
            {
                // raise the error
                try
                {
                    logger.LogWarning($"Error Code {response} was received");
                    errorHandler(GetErrorMessage(response));
                }
                catch (System.Exception ex)
                {
                    logger.LogError(ex, "There was an issue passing the error code out to the error handler");
                }
                return;
            }

            // let's handle all the permutations I care about
            switch (response[0])
            {
                case 'C':
                case 'c':
                    VideoChannel = int.Parse(response[1..]);
                    AudioChannel = int.Parse(response[1..]);
                    break;
                case 'A':
                case 'a':
                    // the 'AMUT' command can also fuck things up here
                    if (response.Length > 3 && response.StartsWith("AMUT", StringComparison.OrdinalIgnoreCase))
                    {
                        IsAudioMuted = response.Last() == '1';
                    }
                    else
                    {
                        AudioChannel = int.Parse(response[1..]);
                    }
                    break;
                case 'V':
                case 'v':
                    // Identify can also affect this command
                    if (response.Length > 4)
                    {
                        HandleIdentifyString(response);
                    }
                    else
                    {
                        VideoChannel = int.Parse(response[1..]);
                    }
                    break;
                case 'B':
                case 'b':
                    IsRgbMuted = response.Last() == '1';
                    break;
                case 'Q':
                case 'q':
                    if (response.StartsWith("qsc", StringComparison.OrdinalIgnoreCase))
                    {
                        SwitcherFirmwareVersion = response[3..];
                    }
                    else
                    {
                        ProjectorFirmwareVersion = response[3..];
                    }
                    break;
                case 'M':
                case 'm':
                    IsProjectorMuted = response.Last() == '1';
                    break;
                case 'P':
                case 'p':
                    if (response.StartsWith("PR"))
                    {
                        IsProjecterPowered = response.Last() == '1';
                    }
                    break;
            }

        }

        /// <summary>
        /// Properly parses the response of the 'I' / 'i' command
        /// </summary>
        /// <param name="identifyResponse">The identify command</param>
        private void HandleIdentifyString(string identifyResponse)
        {
            var responseArray = identifyResponse.Split(' ', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries);

            /*
                The response to identifying is as follows:

                [0] Vx      - Video Channel Number
                [1] Ax      - Audio Channel Number
                [2] Tx      - Video Type (Enum)
                [3] Px      - Projector Power status
                [4] Sx      - Projector Display (muted) status
                [5] Zx      - Switcher Mute
                [6] Rx      - RGB Mute
                [7] QSCx.xx - Switcher Firmware Version
                [8] QPCx.xx - Switcher Projector Firmware Version
                [9] Mx      - Maximum Channels
             */
            if (int.TryParse(responseArray[9][1..], out var mc))
            {
                Channels = mc;
            }
            if (int.TryParse(responseArray[0][1..], out var vc))
            {
                VideoChannel = vc;
            }
            if (int.TryParse(responseArray[1][1..], out var ac))
            {
                AudioChannel = ac;
            }
            if (Enum.TryParse<VideoTypeEnum>(responseArray[2][1..], out var vte))
            {
                VideoType = vte;
            }
            if (int.TryParse(responseArray[3][1..], out var pps))
            {
                IsProjecterPowered = pps == 1;
            }
            if (int.TryParse(responseArray[4][1..], out var pds))
            {
                IsProjectorMuted = pds == 1;
            }
            if (int.TryParse(responseArray[5][1..], out var sm))
            {
                IsAudioMuted = sm == 1;
            }
            if (int.TryParse(responseArray[6][1..], out var rgbm))
            {
                IsRgbMuted = rgbm == 1;
            }
            SwitcherFirmwareVersion = responseArray[7][3..];
            ProjectorFirmwareVersion = responseArray[8][3..];
        }

        /// <summary>
        /// Writes a command to the serial port
        /// </summary>
        /// <param name="command">The command to write</param>
        private void Write(string command)
        {
            com.Write(command);
        }


        private static bool IsResponseError(string response)
        {
            return response[0] == 'E' || response[0] == 'e';
        }

        private static string GetErrorMessage(string errorCode)
        {
            switch (errorCode.ToUpper())
            {
                case "E01":
                    return "Invalid Channel Number";
                case "E02":
                    return "Slave Communication Error";
                case "E03":
                    return "Projector is powered OFF";
                case "E04":
                    return "Projector Commmunication Error";
                case "E06":
                    return "VLB switch enabled & last input selected";
            }
            return $"Unknown Error Code {errorCode}";
        }

        private void HandleAutoOpen(IConfiguration configuration)
        {
            var extronNode = configuration.GetSection("Extron");
            if (extronNode is null) return;
            var serialNode = extronNode.GetSection("Serial");
            if (serialNode is null) return;
            var autoOpenFlag = serialNode["AutoOpen"];
            if (autoOpenFlag is null) return;
            if (bool.TryParse(autoOpenFlag, out var ao) && ao)
            {
                OpenConnection();
            }
        }

    }

}
