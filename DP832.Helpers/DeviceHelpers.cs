using System;
using System.Collections.Generic;
using System.Globalization;

namespace DP832PowerSupply
{
    /// <summary>
    /// Pure helper methods for the DP832 Power Supply Controller.
    /// These methods contain no I/O or hardware dependencies and are fully unit-testable.
    /// </summary>
    public static class DeviceHelpers
    {
        /// <summary>
        /// Parses a SCPI protection/output state response string to a boolean.
        /// "ON" (case-insensitive) and "1" map to true; everything else maps to false.
        /// </summary>
        public static bool ParseProtectionState(string stateStr)
        {
            string trimmedState = stateStr.Trim();
            return trimmedState.Equals("ON", StringComparison.OrdinalIgnoreCase) || trimmedState == "1";
        }

        /// <summary>
        /// Returns the maximum rated voltage for the given DP832 channel.
        /// CH3 is rated at 5 V; CH1 and CH2 are rated at 30 V.
        /// </summary>
        public static double GetChannelMaxVoltage(int channelNum)
        {
            return channelNum == 3 ? 5.0 : 30.0;
        }

        /// <summary>
        /// Returns the maximum rated current (in amps) for any DP832 channel (always 3 A).
        /// </summary>
        public static double GetChannelMaxCurrent()
        {
            return 3.0;
        }

        /// <summary>
        /// Parses the key=value lines of a DP832 state file, skipping comment lines
        /// (starting with '#') and blank lines.
        /// </summary>
        /// <param name="lines">Lines read from the state file.</param>
        /// <returns>A case-insensitive dictionary mapping setting keys to their values.</returns>
        public static Dictionary<string, string> ParseStateFile(IEnumerable<string> lines)
        {
            var settings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (string line in lines)
            {
                string trimmed = line.Trim();
                if (string.IsNullOrEmpty(trimmed) || trimmed.StartsWith("#", StringComparison.Ordinal) || !trimmed.Contains("="))
                    continue;
                int eqIdx = trimmed.IndexOf('=');
                string key = trimmed.Substring(0, eqIdx).Trim();
                string val = trimmed.Substring(eqIdx + 1).Trim();
                settings[key] = val;
            }
            return settings;
        }

        /// <summary>
        /// Builds a GPIB VISA resource string from the given GPIB device number.
        /// </summary>
        public static string FormatGpibAddress(int deviceNumber)
        {
            return $"GPIB0::{deviceNumber}::INSTR";
        }

        /// <summary>
        /// Builds a TCPIP VISA resource string from the given IP address.
        /// </summary>
        public static string FormatTcpipAddress(string ipAddress)
        {
            return $"TCPIP::{ipAddress}::INSTR";
        }

        /// <summary>
        /// Validates that a voltage value is within the legal range for the given channel
        /// (0 to GetChannelMaxVoltage).
        /// </summary>
        public static bool IsValidVoltage(double voltage, int channelNum)
        {
            return voltage >= 0 && voltage <= GetChannelMaxVoltage(channelNum);
        }

        /// <summary>
        /// Validates that a current value is within the legal range for any channel
        /// (0 to GetChannelMaxCurrent).
        /// </summary>
        public static bool IsValidCurrent(double current)
        {
            return current >= 0 && current <= GetChannelMaxCurrent();
        }

        /// <summary>
        /// Validates that an OVP level is within the legal range for the given channel
        /// (0.01 to GetChannelMaxVoltage + 1).
        /// </summary>
        public static bool IsValidOvpLevel(double level, int channelNum)
        {
            return level >= 0.01 && level <= GetChannelMaxVoltage(channelNum) + 1;
        }

        /// <summary>
        /// Validates that an OCP level is within the legal range for any channel
        /// (0.001 to GetChannelMaxCurrent + 1).
        /// </summary>
        public static bool IsValidOcpLevel(double level)
        {
            return level >= 0.001 && level <= GetChannelMaxCurrent() + 1;
        }
    }
}
