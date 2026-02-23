using System;
using System.Collections.Generic;
using System.Threading;
using DP832.Core;
using DP832.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DP832.CLI.Commands
{
    /// <summary>
    /// Clears latched OVP and OCP protection trips for the specified channel using
    /// <c>:OUTPut:OVP:CLEar</c> and <c>:OUTPut:OCP:CLEar</c> SCPI commands.
    /// After clearing, the channel output can be re-enabled with the <c>output</c> command.
    /// Pass <c>--json</c> to receive the result as a JSON object.
    /// </summary>
    public sealed class ClearTripCommand : Command<ChannelSettings>
    {
        /// <inheritdoc/>
        public override int Execute(CommandContext context, ChannelSettings settings, CancellationToken cancellationToken)
        {
            int ch = settings.Channel;
            string chName = "CH" + ch;

            using (var device = new DP832Device(settings.Address))
            {
                try
                {
                    device.Connect();

                    bool ovpTripped = false;
                    bool ocpTripped = false;
                    try { ovpTripped = DeviceHelpers.ParseProtectionState(device.SendQuery(":SOURce" + ch + ":VOLTage:PROTection:TRIP?")); } catch { }
                    try { ocpTripped = DeviceHelpers.ParseProtectionState(device.SendQuery(":SOURce" + ch + ":CURRent:PROTection:TRIP?")); } catch { }

                    bool ovpCleared = true;
                    bool ocpCleared = true;

                    if (ovpTripped)
                    {
                        try
                        {
                            device.SendCommand(":OUTPut:OVP:CLEar " + chName);
                        }
                        catch { ovpCleared = false; }
                    }

                    if (ocpTripped)
                    {
                        try
                        {
                            device.SendCommand(":OUTPut:OCP:CLEar " + chName);
                        }
                        catch { ocpCleared = false; }
                    }

                    bool success = ovpCleared && ocpCleared;

                    if (settings.Json)
                    {
                        Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object>
                        {
                            { "success",     success },
                            { "channel",     ch },
                            { "ovpWasTripped", ovpTripped },
                            { "ovpCleared",  ovpCleared },
                            { "ocpWasTripped", ocpTripped },
                            { "ocpCleared",  ocpCleared }
                        }));
                    }
                    else
                    {
                        if (!ovpTripped && !ocpTripped)
                        {
                            AnsiConsole.MarkupLine("[grey]No protection trips detected on " + chName + ".[/]");
                        }
                        else
                        {
                            if (ovpTripped)
                                AnsiConsole.MarkupLine(ovpCleared
                                    ? "[green]OVP trip cleared for " + chName + ".[/]"
                                    : "[red]Failed to clear OVP trip for " + chName + ".[/]");
                            if (ocpTripped)
                                AnsiConsole.MarkupLine(ocpCleared
                                    ? "[green]OCP trip cleared for " + chName + ".[/]"
                                    : "[red]Failed to clear OCP trip for " + chName + ".[/]");
                        }
                    }
                    return success ? 0 : 1;
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
