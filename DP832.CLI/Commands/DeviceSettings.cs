using System.ComponentModel;
using Spectre.Console.Cli;

namespace DP832.CLI.Commands
{
    /// <summary>
    /// Common settings shared by all DP832 CLI commands.
    /// Provides the VISA device address used to open the instrument session.
    /// </summary>
    public class DeviceSettings : CommandSettings
    {
        /// <summary>
        /// VISA resource address of the DP832 (e.g. <c>GPIB0::1::INSTR</c> or
        /// <c>TCPIP::192.168.1.100::INSTR</c>). Defaults to <c>GPIB0::1::INSTR</c>.
        /// </summary>
        [Description("VISA resource address of the DP832 (e.g. GPIB0::1::INSTR or TCPIP::192.168.1.100::INSTR).")]
        [CommandOption("-a|--address")]
        [DefaultValue("GPIB0::1::INSTR")]
        public string Address { get; set; }
    }
}
