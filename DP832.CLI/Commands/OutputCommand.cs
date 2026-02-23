using System;
using System.ComponentModel;
using DP832.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DP832.CLI.Commands
{
    /// <summary>
    /// Enables or disables the output for the specified DP832 channel.
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
        public override int Execute(CommandContext context, Settings settings)
        {
            bool turnOn;
            if (string.Equals(settings.State, "on", StringComparison.OrdinalIgnoreCase))
                turnOn = true;
            else if (string.Equals(settings.State, "off", StringComparison.OrdinalIgnoreCase))
                turnOn = false;
            else
            {
                AnsiConsole.MarkupLine("[red]Error:[/] --state must be 'on' or 'off'.");
                return 1;
            }

            using (var device = new DP832Device(settings.Address))
            {
                try
                {
                    device.Connect();
                    string stateStr = turnOn ? "ON" : "OFF";
                    device.SendCommand(":OUTPut CH" + settings.Channel + "," + stateStr);
                    AnsiConsole.MarkupLine(
                        "[green]CH" + settings.Channel + " output set to " + stateStr + ".[/]");
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
