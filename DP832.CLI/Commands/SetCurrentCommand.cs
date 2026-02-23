using System;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using DP832.Core;
using DP832.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DP832.CLI.Commands
{
    /// <summary>
    /// Sets the current limit for the specified DP832 channel.
    /// </summary>
    public sealed class SetCurrentCommand : Command<SetCurrentCommand.Settings>
    {
        /// <summary>Settings for the set-current command.</summary>
        public sealed class Settings : ChannelSettings
        {
            /// <summary>Target current limit in amps.</summary>
            [Description("Target current limit in amps (all channels: 0–3 A).")]
            [CommandOption("-i|--current")]
            public double Current { get; set; }
        }

        /// <inheritdoc/>
        public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            if (!DeviceHelpers.IsValidCurrent(settings.Current))
            {
                double maxA = DeviceHelpers.GetChannelMaxCurrent();
                AnsiConsole.MarkupLine(string.Format(
                    "[red]Error:[/] Current {0} A is outside the valid range 0–{1} A.",
                    settings.Current, maxA));
                return 1;
            }

            using (var device = new DP832Device(settings.Address))
            {
                try
                {
                    device.Connect();
                    string cmd = string.Format(
                        CultureInfo.InvariantCulture,
                        ":SOURce{0}:CURRent {1:F3}",
                        settings.Channel, settings.Current);
                    device.SendCommand(cmd);
                    AnsiConsole.MarkupLine(string.Format(
                        CultureInfo.InvariantCulture,
                        "[green]CH{0} current limit set to {1:F3} A.[/]",
                        settings.Channel, settings.Current));
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine("[red]Error:[/] " + ex.Message);
                    return 1;
                }
            }
        }
    }
}
