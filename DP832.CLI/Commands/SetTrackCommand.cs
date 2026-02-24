using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using DP832.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DP832.CLI.Commands
{
    /// <summary>
    /// Enables or disables per-channel output tracking for CH1 or CH2 via <c>:OUTPut:TRACk</c>.
    /// Only CH1 and CH2 support tracking; CH3 is independent.
    /// Pass <c>--json</c> to receive the result as a JSON object.
    /// </summary>
    public sealed class SetTrackCommand : Command<SetTrackCommand.Settings>
    {
        /// <summary>Settings for the set-track command.</summary>
        public sealed class Settings : DeviceSettings
        {
            /// <summary>Channel to configure tracking on: 1 or 2.</summary>
            [Description("Channel to configure tracking on: 1 or 2.")]
            [CommandOption("-c|--channel")]
            [DefaultValue(1)]
            public int Channel { get; set; }

            /// <summary>Desired tracking state: <c>on</c> or <c>off</c>.</summary>
            [Description("Desired tracking state: on or off.")]
            [CommandOption("-s|--state")]
            public string State { get; set; }
        }

        /// <inheritdoc/>
        public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            if (settings.Channel != 1 && settings.Channel != 2)
            {
                string msg = "--channel must be 1 or 2 (only CH1 and CH2 support tracking).";
                if (settings.Json)
                    Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object> { { "success", false }, { "error", msg } }));
                else
                {
                    AnsiConsole.MarkupLine("[red]Error:[/] " + msg);
                    AnsiConsole.MarkupLine("[grey]Usage: dp832 set-track -s <on|off> [[-c <n>]] [[-a <address>]][/]");
                }
                return 1;
            }

            bool turnOn;
            if (string.Equals(settings.State, "on", StringComparison.OrdinalIgnoreCase))
                turnOn = true;
            else if (string.Equals(settings.State, "off", StringComparison.OrdinalIgnoreCase))
                turnOn = false;
            else
            {
                string msg = "--state must be 'on' or 'off'.";
                if (settings.Json)
                    Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object> { { "success", false }, { "error", msg } }));
                else
                {
                    AnsiConsole.MarkupLine("[red]Error:[/] " + msg);
                    AnsiConsole.MarkupLine("[grey]Usage: dp832 set-track -s <on|off> [[-c <n>]] [[-a <address>]][/]");
                }
                return 1;
            }

            string chName = "CH" + settings.Channel;

            using (var device = new DP832Device(settings.Address))
            {
                try
                {
                    device.Connect();
                    device.SendCommand(":OUTPut:TRACk " + chName + "," + (turnOn ? "ON" : "OFF"));

                    if (settings.Json)
                    {
                        Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object>
                        {
                            { "success", true },
                            { "channel", settings.Channel },
                            { "enabled", turnOn }
                        }));
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[green]" + chName + " tracking " + (turnOn ? "enabled" : "disabled") + ".[/]");
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    if (settings.Json)
                        Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object> { { "success", false }, { "error", ex.Message } }));
                    else
                        AnsiConsole.MarkupLine("[red]Error:[/] " + Markup.Escape(ex.Message));
                    return 1;
                }
            }
        }
    }
}
