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
    /// Sets the current limit for the specified DP832 channel.
    /// Pass <c>--json</c> to receive the result as a JSON object.
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
                string msg = string.Format(
                    CultureInfo.InvariantCulture,
                    "Current {0} A is outside the valid range 0\u2013{1} A.",
                    settings.Current, maxA);
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
                    AnsiConsole.MarkupLine("[grey]Usage: dp832 set-current -i <a> [[-c <n>]] [[-a <address>]][/]");
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
                        ":SOURce{0}:CURRent {1:F3}",
                        settings.Channel, settings.Current);
                    device.SendCommand(cmd);

                    if (settings.Json)
                    {
                        Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object>
                        {
                            { "success", true },
                            { "channel", settings.Channel },
                            { "current", settings.Current }
                        }));
                    }
                    else
                    {
                        AnsiConsole.MarkupLine(string.Format(
                            CultureInfo.InvariantCulture,
                            "[green]CH{0} current limit set to {1:F3} A.[/]",
                            settings.Channel, settings.Current));
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
