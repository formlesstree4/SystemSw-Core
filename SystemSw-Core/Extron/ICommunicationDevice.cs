using System;

namespace SystemSw_Core.Extron
{

    /// <summary>
    /// Defines a generic external communication device
    /// </summary>
    public interface ICommunicationDevice : IDisposable
    {

        /// <summary>
        /// Gets whether or not there is an open communication channel to the device
        /// </summary>
        bool IsOpen { get; }

        /// <summary>
        /// Opens the connection to the device
        /// </summary>
        void Open();

        /// <summary>
        /// Closes the connection to the device
        /// </summary>
        void Close();

        /// <summary>
        /// Synchronously writes the given string to the device
        /// </summary>
        /// <param name="text">The string of text to write to the device</param>
        void Write(string text);

        /// <summary>
        /// Reads characters from the input buffer until a <seealso cref="Environment.NewLine"/> is encountered
        /// </summary>
        /// <returns>The string that was read</returns>
        string ReadLine();

    }

}
