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
    /// Enables or disables the output for the specified DP832 channel.
    /// Pass <c>--json</c> to receive the result as a JSON object.
    /// </summary>
    public sealed class OutputCommand : Command<OutputCommand.Settings>
    {
        /// <summary>Settings for the output command.</summary>
        public sealed class Settings : ChannelSettings
        {
            /// <summary>Desired output state: <c>on</c> or <c>off</c>.</summary>
            [Description("Desired output state: on or off.")]
            [CommandOption("-s|--state")]
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
                    AnsiConsole.MarkupLine("[grey]Usage: dp832 output -s <on|off> [[-c <n>]] [[-a <address>]][/]");
                }
                return 1;
            }

            using (var device = new DP832Device(settings.Address))
            {
                try
                {
                    device.Connect();
                    string stateStr = turnOn ? "ON" : "OFF";
                    device.SendCommand(":OUTPut CH" + settings.Channel + "," + stateStr);

                    if (settings.Json)
                    {
                        Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object>
                        {
                            { "success", true },
                            { "channel", settings.Channel },
                            { "state", stateStr }
                        }));
                    }
                    else
                    {
                        AnsiConsole.MarkupLine(
                            "[green]CH" + settings.Channel + " output set to " + stateStr + ".[/]");
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
