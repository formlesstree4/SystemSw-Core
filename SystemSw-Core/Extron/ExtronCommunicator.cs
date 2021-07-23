using System;
using System.Collections.Generic;
using System.IO.Ports;
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
            string serialPort, bool open, int readTimeout = 1000)
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
                RtsEnable = false,
                ReadTimeout = readTimeout,
                Encoding = System.Text.Encoding.ASCII
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
        /// <exception cref="System.UnauthorizedAccessException"/>
        /// <exception cref="System.ArgumentOutOfRangeException"/>
        /// <exception cref="System.ArgumentException"/>
        /// <exception cref="System.InvalidOperationException"/>
        /// <exception cref="System.IO.IOException"/>
        public void OpenConnection()
        {
            if (SerialPort.IsOpen) return;
            SerialPort.Open();
            source = new CancellationTokenSource();
            readLoop = Task.Run(InternalReadLoop);
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
        /// Identifies what the connected system is.
        /// </summary>
        /// <returns>A boolean value to </returns>
        public async Task<bool> Identify()
        {
            Write(command: "I");

            var identifyResponse = await WaitForResponseAsync();
            /*
                The response to identifying is as follows:

                Vx      - Video and Channel Number
                Ax      - Audio and Channel Number
                Tx      - Video Type (Enum)
                Px      - Display Power status
                Sx      - Display Mute status
                Zx      - Switcher Mute
                Rx      - RGB Mute
                QSCx.xx
                QPCx.xx
                Mx
             
             */

        }





        private void InternalReadLoop()
        {
            while(!source.IsCancellationRequested)
            {
                try
                {
                    var emptyLine = SerialPort.ReadLine();
                    var dataLine = SerialPort.ReadLine();
                    var emptyLine2 = SerialPort.ReadLine();
                }
                catch (TimeoutException te)
                {

                    
                }
            }
        }


        /// <summary>
        /// Waits until there is a response in the buffer and returns it to the caller
        /// </summary>
        /// <param name="cancellationToken">An optional <see cref="CancellationToken"/> that allows for the wait to be aborted</param>
        /// <returns>A promise to return a string</returns>
        private async Task<string> WaitForResponseAsync(CancellationToken cancellationToken = default)
        {
            while(string.IsNullOrEmpty(lastReceivedResponse) && !cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(100, cancellationToken);
            }
            lock(lastReceivedResponse)
            {
                var response = lastReceivedResponse;
                lastReceivedResponse = "";
                return response;
            }
        }

        private void Write(string command)
        {
            SerialPort.Write(command);
        }


    }

}
