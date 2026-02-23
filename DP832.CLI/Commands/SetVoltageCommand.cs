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
    /// Sets the output voltage for the specified DP832 channel.
    /// </summary>
    public sealed class SetVoltageCommand : Command<SetVoltageCommand.Settings>
    {
        /// <summary>Settings for the set-voltage command.</summary>
        public sealed class Settings : ChannelSettings
        {
            /// <summary>Target output voltage in volts.</summary>
            [Description("Target output voltage in volts (CH1/CH2: 0–30 V, CH3: 0–5 V).")]
            [CommandOption("-v|--voltage")]
            public double Voltage { get; set; }
        }

        /// <inheritdoc/>
        public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            if (!DeviceHelpers.IsValidVoltage(settings.Voltage, settings.Channel))
            {
                double maxV = DeviceHelpers.GetChannelMaxVoltage(settings.Channel);
                AnsiConsole.MarkupLine(string.Format(
                    "[red]Error:[/] Voltage {0} V is outside the valid range 0–{1} V for CH{2}.",
                    settings.Voltage, maxV, settings.Channel));
                return 1;
            }

            using (var device = new DP832Device(settings.Address))
            {
                try
                {
                    device.Connect();
                    string cmd = string.Format(
                        CultureInfo.InvariantCulture,
                        ":SOURce{0}:VOLTage {1:F3}",
                        settings.Channel, settings.Voltage);
                    device.SendCommand(cmd);
                    AnsiConsole.MarkupLine(string.Format(
                        CultureInfo.InvariantCulture,
                        "[green]CH{0} voltage set to {1:F3} V.[/]",
                        settings.Channel, settings.Voltage));
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
