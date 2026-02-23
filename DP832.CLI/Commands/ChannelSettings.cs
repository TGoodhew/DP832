using System.ComponentModel;
using Spectre.Console.Cli;

namespace DP832.CLI.Commands
{
    /// <summary>
    /// Settings for commands that target a specific DP832 output channel (1, 2, or 3).
    /// </summary>
    public class ChannelSettings : DeviceSettings
    {
        /// <summary>
        /// Output channel number: 1, 2, or 3.
        /// </summary>
        [Description("Output channel number (1, 2, or 3).")]
        [CommandOption("-c|--channel")]
        [DefaultValue(1)]
        public int Channel { get; set; }
    }
}
