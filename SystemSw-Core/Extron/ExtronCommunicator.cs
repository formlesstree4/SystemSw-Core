using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SystemSw_Core.Extron
{

    /// <summary>
    /// A communication class for the Extron System 8 and System 10 PLUS
    /// </summary>
    /// <remarks>
    /// The code written here was based off the RS-232 programming guide located on pages A-3, A-4, and A-5.
    /// I will not be providing a copy of the manual as it can be found on Extron's website.
    /// </remarks>
    public sealed class ExtronCommunicator
    {

        private Stack<string> history;
        private string lastReceivedResponse;
        private CancellationTokenSource source;
        private Task readLoop;


        /// <summary>
        /// Gets the Serial Port that the communicator is utilizing
        /// </summary>
        public string SerialPortAddress { get; init; }

        /// <summary>
        /// Gets the actual <see cref="SerialPort"/> used to communicate with the Extron device
        /// </summary>
        public SerialPort SerialPort { get; init; }

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
        /// Creates a new instance of the <see cref="ExtronCommunicator"/> for the given serial port
        /// </summary>
        /// <param name="serialPort">The address of the serial port</param>
        /// <param name="open">Immediately invoke <see cref="OpenConnection"/> if true</param>
        /// <param name="readTimeout">How fast the <see cref="System.IO.Ports.SerialPort"/> class will timeout a read operation</param>
        public ExtronCommunicator(
            string serialPort, bool open = false, int readTimeout = 1000)
        {
            SerialPortAddress = serialPort;
            SerialPort = new SerialPort(serialPort)
            {
                BaudRate = 9600,
                Parity = Parity.None,
                StopBits = StopBits.One,
                DataBits = 8,
                Handshake = Handshake.None,
                DtrEnable = true,
                RtsEnable = false
            };
            lastReceivedResponse = "";
            history = new Stack<string>();
            if (open)
            {
                OpenConnection();
            }
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
        public void OpenConnection(bool doNotIdentify = false)
        {
            if (SerialPort.IsOpen) return;
            SerialPort.Open();
            source = new CancellationTokenSource();
            readLoop = Task.Run(InternalReadLoop);
            if (!doNotIdentify) Identify();
        }

        /// <summary>
        /// Attempts to close the connection
        /// </summary>
        public void CloseConnection()
        {
            SerialPort.Close();
            source.Cancel();
            readLoop.Dispose();
        }

        /// <summary>
        /// Ensures the input and output buffers are clean
        /// </summary>
        public void FlushBuffers()
        {
            SerialPort.DiscardInBuffer();
            SerialPort.DiscardOutBuffer();
        }


        /// <summary>
        /// Identifies what the connected system is and sets up initial settings.
        /// </summary>
        /// <returns>A promise to indicate if identification was successful</returns>
        public void Identify()
        {
            Write(command: "I");
        }

        /// <summary>
        /// Changes the audio and video channels
        /// </summary>
        /// <param name="channel">The channel to change to</param>
        /// <returns>A promise to indicate if it was successful or not</returns>
        public void ChangeChannel(int channel)
        {
            if (channel < 0 || channel > Channels)
            {
                return;
            }

            Write($"{channel}!");
        }

        /// <summary>
        /// Changes the video channel
        /// </summary>
        /// <param name="channel">The channel to change to</param>
        /// <returns>A promise to indicate if it was successful or not</returns>
        public void ChangeVideoChannel(int channel)
        {
            if (channel < 0 || channel > Channels)
            {
                return;
            }

            Write($"{channel}&");
        }

        /// <summary>
        /// Changes the audio channel
        /// </summary>
        /// <param name="channel">The channel to change to</param>
        /// <returns>A promise to indicate if it was successful or not</returns>
        public void ChangeAudioChannel(int channel)
        {
            if (channel < 0 || channel > Channels)
            {
                return;
            }

            Write($"{channel}$");
        }

        /// <summary>
        /// Updates the software version numbers for the Extron
        /// </summary>
        public void GetSoftwareVersions()
        {
            Write("q");
        }



        /// <summary>
        /// Sets the projector power if it is connected
        /// </summary>
        /// <param name="powered">true to set power on</param>
        public void SetProjectorPower(bool powered)
        {
            Write(powered ? "[" : "]");
        }

        /// <summary>
        /// Sets the projector visibility if it is connected
        /// </summary>
        /// <param name="visible">true to set visibility to on</param>
        public void SetProjectorVisibility(bool visible)
        {
            Write(visible ? ")": "(");
        }

        /// <summary>
        /// Set the RGB visibility state
        /// </summary>
        /// <param name="visible">true to set visibility to on</param>
        public void SetRgbVisibility(bool visible)
        {
            Write(visible ? "b" : "B");
        }

        /// <summary>
        /// Sets whether or not the audio line is muted or not
        /// </summary>
        /// <param name="muted">true to mute</param>
        public void SetAudioState(bool muted)
        {
            Write(muted ? "+" : "-");
        }



        /// <summary>
        /// Polls the serial port for incoming data
        /// </summary>
        private void InternalReadLoop()
        {
            while(!source.IsCancellationRequested)
            {
                try
                {
                    var dataLine = SerialPort.ReadLine();
                    if (!string.IsNullOrEmpty(dataLine))
                    {
                        HandleIncomingResponse(dataLine);
                    }
                }
                catch (TimeoutException)
                {
                    // no worries   
                }
                catch (Exception)
                {
                    // something has gone horribly wrong [!]
                }
            }
        }

        /// <summary>
        /// Handles the incoming response message
        /// </summary>
        /// <param name="response">The response string to interpret</param>
        private void HandleIncomingResponse(string response)
        {
            if (string.IsNullOrWhiteSpace(response) || string.IsNullOrEmpty(response)) return;
            history.Push($"< {response}");
            
            // let's handle all the permutations I care about
            switch (response[0])
            {
                case 'C':
                case 'c':
                    AudioChannel = int.Parse(response.Substring(1));
                    VideoChannel = AudioChannel;
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
                        AudioChannel = int.Parse(response.Substring(1));
                    }
                    break;
                case 'V':
                case 'v':
                    // Identify can also affect this command
                    if (response.Length > 3)
                    {
                        HandleIdentifyString(response);
                    }
                    else
                    {
                        VideoChannel = int.Parse(response.Substring(1));
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
                        SwitcherFirmwareVersion = response.Substring(3);
                    }
                    else
                    {
                        ProjectorFirmwareVersion = response.Substring(3);
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

                [0] Vx      - Video and Channel Number
                [1] Ax      - Audio and Channel Number
                [2] Tx      - Video Type (Enum)
                [3] Px      - Projector Power status
                [4] Sx      - Projector Display (muted) status
                [5] Zx      - Switcher Mute
                [6] Rx      - RGB Mute
                [7] QSCx.xx - Switcher Firmware Version
                [8] QPCx.xx - Switcher Projector Firmware Version
                [9] Mx      - Maximum Channels
             */
            if (int.TryParse(responseArray[9].Substring(1), out var mc))
            {
                Channels = mc;
            }
            if (int.TryParse(responseArray[0].Substring(1), out var vc))
            {
                VideoChannel = vc;
            }
            if (int.TryParse(responseArray[1].Substring(1), out var ac))
            {
                AudioChannel = vc;
            }
            if (Enum.TryParse<VideoTypeEnum>(responseArray[2].Substring(1), out var vte))
            {
                VideoType = vte;
            }
            if (int.TryParse(responseArray[3].Substring(1), out var pps))
            {
                IsProjecterPowered = pps == 1;
            }
            if (int.TryParse(responseArray[4].Substring(1), out var pds))
            {
                IsProjectorMuted = pds == 1;
            }
            if (int.TryParse(responseArray[5].Substring(1), out var sm))
            {
                IsAudioMuted = sm == 1;
            }
            if (int.TryParse(responseArray[6].Substring(1), out var rgbm))
            {
                IsRgbMuted = rgbm == 1;
            }
            SwitcherFirmwareVersion = responseArray[7].Substring(3);
            ProjectorFirmwareVersion = responseArray[8].Substring(3);
        }

        /// <summary>
        /// Writes a command to the serial port
        /// </summary>
        /// <param name="command">The command to write</param>
        private void Write(string command)
        {
            SerialPort.Write(command);
            history.Push("> {command}");
        }

        private bool IsResponseError(string response)
        {
            return response[0] == 'E' || response[0] == 'e';
        }

        private string GetErrorMessage(string errorCode)
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


    }

}
