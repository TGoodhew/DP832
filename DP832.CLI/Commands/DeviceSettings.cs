using System.ComponentModel;
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
        /// a plain GPIB device number (e.g. <c>1</c>), or a bare IPv4 address
        /// (e.g. <c>192.168.1.100</c>). Defaults to <c>GPIB0::1::INSTR</c>.
        /// </summary>
        [Description("VISA resource address of the DP832. Accepts a full VISA string, a GPIB number (e.g. 1), or an IP address (e.g. 192.168.1.100).")]
        [CommandOption("-a|--address")]
        [DefaultValue("GPIB0::1::INSTR")]
        public string Address
        {
            get => _address;
            set => _address = DeviceHelpers.ResolveAddress(value);
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
