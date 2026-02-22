using System.Collections.Generic;

namespace DP832.Core
{
    /// <summary>
    /// Defines the contract for communicating with a Rigol DP832 programmable DC power supply.
    /// Implement this interface to provide a concrete communication layer (e.g. NI-VISA, mock).
    /// </summary>
    public interface IDP832Device
    {
        /// <summary>Gets a value indicating whether a VISA session is currently open.</summary>
        bool IsConnected { get; }

        /// <summary>
        /// Opens a VISA session to the device at the configured address.
        /// </summary>
        void Connect();

        /// <summary>
        /// Closes the VISA session and releases all resources.
        /// </summary>
        void Disconnect();

        /// <summary>
        /// Sends a raw SCPI command string to the device (write-only, no response expected).
        /// </summary>
        /// <param name="command">The SCPI command to send.</param>
        void SendCommand(string command);

        /// <summary>
        /// Sends a raw SCPI query string and returns the trimmed response.
        /// </summary>
        /// <param name="query">The SCPI query to send (usually ends with '?').</param>
        /// <returns>Trimmed response string from the instrument.</returns>
        string SendQuery(string query);

        /// <summary>
        /// Queries the device identification string via *IDN?.
        /// </summary>
        /// <returns>The instrument identification string.</returns>
        string GetIdentification();

        /// <summary>
        /// Reads and returns all pending SCPI error queue entries.
        /// Returns an empty list when the queue contains only "0, No error".
        /// </summary>
        IList<string> GetErrors();

        /// <summary>Gets or sets the VISA resource address string used when connecting.</summary>
        string DeviceAddress { get; set; }
    }
}
