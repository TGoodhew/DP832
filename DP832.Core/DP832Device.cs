using System;
using System.Collections.Generic;
using NationalInstruments.Visa;

namespace DP832.Core
{
    /// <summary>
    /// NI-VISA implementation of <see cref="IDP832Device"/> for communicating with a
    /// Rigol DP832 programmable DC power supply over GPIB, TCPIP, or USB.
    /// </summary>
    public class DP832Device : IDP832Device, IDisposable
    {
        private ResourceManager _resourceManager;
        private MessageBasedSession _visaSession;
        private bool _disposed;

        /// <summary>
        /// Initialises a new instance of <see cref="DP832Device"/> with the given VISA address.
        /// </summary>
        /// <param name="deviceAddress">
        /// A VISA resource string such as <c>GPIB0::1::INSTR</c> or <c>TCPIP::192.168.1.100::INSTR</c>.
        /// </param>
        public DP832Device(string deviceAddress)
        {
            DeviceAddress = deviceAddress ?? throw new ArgumentNullException(nameof(deviceAddress));
        }

        /// <inheritdoc/>
        public string DeviceAddress { get; set; }

        /// <inheritdoc/>
        public bool IsConnected
        {
            get { return _visaSession != null; }
        }

        /// <inheritdoc/>
        public void Connect()
        {
            if (_visaSession != null)
                return;

            _resourceManager = new ResourceManager();
            _visaSession = (MessageBasedSession)_resourceManager.Open(DeviceAddress);
        }

        /// <inheritdoc/>
        public void Disconnect()
        {
            if (_visaSession != null)
            {
                _visaSession.Dispose();
                _visaSession = null;
            }
            if (_resourceManager != null)
            {
                _resourceManager.Dispose();
                _resourceManager = null;
            }
        }

        /// <inheritdoc/>
        public void SendCommand(string command)
        {
            if (_visaSession == null)
                throw new InvalidOperationException("Not connected to device.");
            _visaSession.RawIO.Write(command + "\n");
        }

        /// <inheritdoc/>
        public string SendQuery(string query)
        {
            if (_visaSession == null)
                throw new InvalidOperationException("Not connected to device.");
            _visaSession.RawIO.Write(query + "\n");
            return _visaSession.RawIO.ReadString().Trim();
        }

        /// <inheritdoc/>
        public string GetIdentification()
        {
            return SendQuery("*IDN?");
        }

        /// <inheritdoc/>
        public IList<string> GetErrors()
        {
            var errors = new List<string>();
            while (true)
            {
                string response = SendQuery(":SYSTem:ERRor?");
                if (response.StartsWith("0,", StringComparison.Ordinal))
                    break;
                errors.Add(response);
            }
            return errors;
        }

        /// <summary>Disposes the VISA session and resource manager.</summary>
        public void Dispose()
        {
            if (!_disposed)
            {
                Disconnect();
                _disposed = true;
            }
        }
    }
}
