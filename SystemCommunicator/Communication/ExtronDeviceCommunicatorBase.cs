using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Threading;
using System.Threading.Tasks;
using SystemCommunicator.Communication.Results;
using SystemCommunicator.Devices;

namespace SystemCommunicator.Communication
{

    /// <summary>
    /// The base class of all communication instances
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract class ExtronDeviceCommunicatorBase<T> : IExtronDeviceCommunicator
        where T: class
    {

        private CancellationTokenSource source;
        private Task readLoop;
        private readonly ICommunicationDevice com;
        private readonly ILogger<T> logger;
        private readonly IConfiguration configuration;
        private Action<string> errorHandler;
        private string lastCommand;

        /// <inheritdoc cref="IExtronDeviceCommunicator.FirmwareVersion"/>
        public string FirmwareVersion { get; protected set; }

        /// <inheritdoc cref="IExtronDeviceCommunicator.IsConnectionOpen"/>
        public bool IsConnectionOpen => com.IsOpen;

        /// <inheritdoc cref="IExtronDeviceCommunicator.CommunicationDevice"/>
        public ICommunicationDevice CommunicationDevice => com;

        /// <summary>
        /// Gets the <see cref="ILogger{TCategoryName}"/> instance
        /// </summary>
        protected ILogger<T> Logger => logger;

        /// <summary>
        /// Gets the <see cref="IConfiguration"/> instance
        /// </summary>
        protected IConfiguration Configuration => configuration;



        /// <summary>
        /// The base communicator class that abstracts away the content that should not change really between implementations of the communicator
        /// </summary>
        /// <param name="com">The device interface to communicate with</param>
        /// <param name="logger">Logging class</param>
        /// <param name="configuration">The configuration class</param>
        public ExtronDeviceCommunicatorBase(
            ICommunicationDevice com,
            ILogger<T> logger,
            IConfiguration configuration)
        {
            this.com = com;
            this.logger = logger;
            this.configuration = configuration;
        }



        /// <inheritdoc cref="IExtronDeviceCommunicator.CloseConnection"/>
        public virtual ICommunicationResult CloseConnection()
        {
            com.Close();
            source.Cancel();
            readLoop.Dispose();
            return CommunicationResult.Ok();
        }

        /// <inheritdoc cref="IExtronDeviceCommunicator.Identify"/>
        public virtual ICommunicationResult Identify()
        {
            Write(command: "I");
            return CommunicationResult.Ok();
        }

        /// <inheritdoc cref="IExtronDeviceCommunicator.OpenConnection"/>
        public virtual ICommunicationResult OpenConnection()
        {
            if (!com.IsOpen)
            {
                com.Open();
            }
            else
            {
                logger.LogTrace("OpenConnection() was called even though the device was already open.");
            }
            if (source == null || source.IsCancellationRequested)
            {
                source = new CancellationTokenSource();
                readLoop = Task.Run(InternalReadLoop);
                Identify();
                return CommunicationResult.Ok();
            }
            else
            {
                return CommunicationResult.Error("connection already open", ResultCode.Unknown, false);
            }
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
        /// Invokes the error handler(s) (if any are registered) with the given error message
        /// </summary>
        /// <param name="errorMessage">The error message</param>
        protected virtual void InvokeErrorHandler(string errorMessage)
        {
            errorHandler?.Invoke(errorMessage);
        }

        /// <summary>
        /// Writes a command to the serial port
        /// </summary>
        /// <param name="command">The command to write</param>
        protected void Write(string command)
        {
            lock(this)
            {
                lastCommand = command;
            }
            com.Write(command);
        }

        /// <summary>
        /// Handles the incoming response message
        /// </summary>
        /// <param name="lastCommand">The last command that was sent over the wire</param>
        /// <param name="response">The response string to interpret</param>
        /// <remarks>
        /// This is not an exact implementation of every response code that the Extron system can send back.
        /// I only implemented what I felt was currently necessary
        /// </remarks>
        protected abstract void HandleIncomingResponse(string lastCommand, string response);

        /// <summary>
        /// Polls the serial port for incoming data
        /// </summary>
        protected virtual void InternalReadLoop()
        {
            while (!source.IsCancellationRequested)
            {
                logger.LogInformation("Begin Read Loop");
                try
                {
                    var dataLine = com.ReadLine();
                    if (!string.IsNullOrEmpty(dataLine))
                    {
                        string cmd;
                        lock(this)
                        {
                            cmd = lastCommand;
                            lastCommand = string.Empty;
                        }
                        logger.LogInformation($"Command: {dataLine}");
                        HandleIncomingResponse(cmd, dataLine);
                    }
                }
                catch (TimeoutException te)
                {
                    // no worries
                    logger.LogWarning(te, $"There was a timeout reading from the {nameof(ICommunicationDevice)}. This is probably expected so it can be ignored");
                }
                catch (Exception ex)
                {
                    // something has gone horribly wrong [!]
                    logger.LogError(ex, "An error occurred reading from the stream");
                }

            }

        }

    }

}
