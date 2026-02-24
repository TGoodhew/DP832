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
    /// Enables or disables the display screen saver via <c>:SYSTem:SAVer</c>.
    /// When enabled, the screen saver activates after 25 minutes of standby.
    /// Pass <c>--json</c> to receive the result as a JSON object.
    /// </summary>
    public sealed class SetScreenSaverCommand : Command<SetScreenSaverCommand.Settings>
    {
        /// <summary>Settings for the set-screensaver command.</summary>
        public sealed class Settings : DeviceSettings
        {
            /// <summary>Desired screen saver state: on or off.</summary>
            [Description("Desired screen saver state: on or off.")]
            [CommandArgument(0, "<state>")]
            public string State { get; set; }
        }

        /// <inheritdoc/>
        public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
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
                    AnsiConsole.MarkupLine("[grey]Usage: dp832 set-screensaver <on|off> [[--address <address>]][/]");
                }
                return 1;
            }

            using (var device = new DP832Device(settings.Address))
            {
                try
                {
                    device.Connect();
                    device.SendCommand(":SYSTem:SAVer " + (turnOn ? "ON" : "OFF"));

                    if (settings.Json)
                    {
                        Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object>
                        {
                            { "success", true },
                            { "enabled", turnOn }
                        }));
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[green]Screen saver " + (turnOn ? "enabled" : "disabled") + ".[/]");
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
