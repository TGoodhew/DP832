using System;
using System.Collections.Generic;
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
    /// Pass <c>--json</c> to receive the result as a JSON object.
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
                string msg = string.Format(
                    CultureInfo.InvariantCulture,
                    "Voltage {0} V is outside the valid range 0\u2013{1} V for CH{2}.",
                    settings.Voltage, maxV, settings.Channel);
                if (settings.Json)
                {
                    Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object>
                    {
                        { "success", false },
                        { "error", msg }
                    }));
                }
                else
                {
                    AnsiConsole.MarkupLine("[red]Error:[/] " + msg);
                }
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

                    if (settings.Json)
                    {
                        Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object>
                        {
                            { "success", true },
                            { "channel", settings.Channel },
                            { "voltage", settings.Voltage }
                        }));
                    }
                    else
                    {
                        AnsiConsole.MarkupLine(string.Format(
                            CultureInfo.InvariantCulture,
                            "[green]CH{0} voltage set to {1:F3} V.[/]",
                            settings.Channel, settings.Voltage));
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    if (settings.Json)
                    {
                        Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object>
                        {
                            { "success", false },
                            { "error", ex.Message }
                        }));
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Error:[/] " + Markup.Escape(ex.Message));
                    }
                    return 1;
                }
            }
        }
    }
}
