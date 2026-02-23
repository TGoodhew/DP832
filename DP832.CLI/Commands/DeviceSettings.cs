using System.ComponentModel;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using DP832.Helpers;
using Spectre.Console.Cli;

namespace DP832.CLI.Commands
{
    /// <summary>
    /// Common settings shared by all DP832 CLI commands.
    /// Provides the VISA device address used to open the instrument session.
    /// </summary>
    public class DeviceSettings : CommandSettings
    {
        private string _address;

        /// <summary>
        /// VISA resource address of the DP832. Accepts a full VISA resource string
        /// (e.g. <c>GPIB0::1::INSTR</c> or <c>TCPIP::192.168.1.100::INSTR</c>),
        /// a plain GPIB device number (e.g. <c>1</c>), a bare IPv4 address
        /// (e.g. <c>192.168.1.100</c>), or just the last octet of the host's subnet
        /// (e.g. <c>136</c> when the host is on <c>192.168.1.x</c>).
        /// Defaults to <c>GPIB0::1::INSTR</c>.
        /// </summary>
        [Description("VISA resource address of the DP832. Accepts a full VISA string, an IP address, the last IP octet (e.g. 136), or a GPIB number (e.g. 1) when no network is detected.")]
        [CommandOption("-a|--address")]
        [DefaultValue("GPIB0::1::INSTR")]
        public string Address
        {
            get => _address;
            set => _address = DeviceHelpers.ResolveAddress(value, GetHostIpPrefix());
        }

        /// <summary>
        /// Returns the first three octets (e.g. <c>192.168.1</c>) of the host machine's
        /// first active non-loopback IPv4 network interface, or <see langword="null"/> if
        /// no such interface is found.
        /// </summary>
        private static string GetHostIpPrefix()
        {
            foreach (NetworkInterface ni in NetworkInterface.GetAllNetworkInterfaces())
            {
                if (!IsValidNetworkInterface(ni))
                    continue;

                foreach (UnicastIPAddressInformation addr in ni.GetIPProperties().UnicastAddresses)
                {
                    if (addr.Address.AddressFamily != AddressFamily.InterNetwork)
                        continue;

                    string[] octets = addr.Address.ToString().Split('.');
                    if (octets.Length == 4)
                        return $"{octets[0]}.{octets[1]}.{octets[2]}";
                }
            }
            return null;
        }

        /// <summary>
        /// Returns <see langword="true"/> when the network interface is operational
        /// and is not a loopback adapter.
        /// </summary>
        private static bool IsValidNetworkInterface(NetworkInterface ni)
        {
            return ni.OperationalStatus == OperationalStatus.Up &&
                   ni.NetworkInterfaceType != NetworkInterfaceType.Loopback;
        }

        /// <summary>
        /// When set, command output is printed as a JSON object instead of a formatted console table.
        /// Useful for scripting and automation.
        /// </summary>
        [Description("Output the result as JSON instead of formatted console text.")]
        [CommandOption("--json")]
        [DefaultValue(false)]
        public bool Json { get; set; }
    }
}
