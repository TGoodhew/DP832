using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading;
using DP832.Core;
using DP832.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DP832.CLI.Commands
{
    /// <summary>
    /// Displays detailed settings and measurements for a single DP832 channel: set and measured
    /// voltage and current, power, OVP/OCP levels, states and trip status, and output state.
    /// Pass <c>--json</c> to receive the result as a JSON object.
    /// </summary>
    public sealed class ChannelStatusCommand : Command<ChannelSettings>
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

                    double voltSet    = ParseDouble(device.SendQuery(":SOURce" + ch + ":VOLTage?"));
                    double voltMeas   = ParseDouble(device.SendQuery(":MEASure:VOLTage? " + chName));
                    double currSet    = ParseDouble(device.SendQuery(":SOURce" + ch + ":CURRent?"));
                    double currMeas   = ParseDouble(device.SendQuery(":MEASure:CURRent? " + chName));
                    double power      = ParseDouble(device.SendQuery(":MEASure:POWEr? " + chName));
                    double ovpLevel   = ParseDouble(device.SendQuery(":SOURce" + ch + ":VOLTage:PROTection?"));
                    bool   ovpEnabled = DeviceHelpers.ParseProtectionState(device.SendQuery(":SOURce" + ch + ":VOLTage:PROTection:STATe?"));
                    double ocpLevel   = ParseDouble(device.SendQuery(":SOURce" + ch + ":CURRent:PROTection?"));
                    bool   ocpEnabled = DeviceHelpers.ParseProtectionState(device.SendQuery(":SOURce" + ch + ":CURRent:PROTection:STATe?"));
                    bool   outEnabled = DeviceHelpers.ParseProtectionState(device.SendQuery(":OUTPut? " + chName));

                    // Trip queries are separate so a failure does not block the rest of the view.
                    bool ovpTripped = false;
                    bool ocpTripped = false;
                    try { ovpTripped = DeviceHelpers.ParseProtectionState(device.SendQuery(":SOURce" + ch + ":VOLTage:PROTection:TRIP?")); } catch { }
                    try { ocpTripped = DeviceHelpers.ParseProtectionState(device.SendQuery(":SOURce" + ch + ":CURRent:PROTection:TRIP?")); } catch { }

                    if (settings.Json)
                    {
                        var obj = new Dictionary<string, object>
                        {
                            { "channel",          chName },
                            { "voltageSet",       voltSet },
                            { "voltageMeasured",  voltMeas },
                            { "currentSet",       currSet },
                            { "currentMeasured",  currMeas },
                            { "powerMeasured",    power },
                            { "ovpLevel",         ovpLevel },
                            { "ovpEnabled",       ovpEnabled },
                            { "ovpTripped",       ovpTripped },
                            { "ocpLevel",         ocpLevel },
                            { "ocpEnabled",       ocpEnabled },
                            { "ocpTripped",       ocpTripped },
                            { "outputEnabled",    outEnabled }
                        };
                        Console.WriteLine(JsonBuilder.Serialize(obj));
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[bold cyan]Channel Status for " + chName + "[/]");
                        AnsiConsole.WriteLine();

                        var table = new Table()
                            .Border(TableBorder.Rounded)
                            .AddColumn(new TableColumn("[bold]Parameter[/]").Centered())
                            .AddColumn(new TableColumn("[bold]Setting[/]").Centered())
                            .AddColumn(new TableColumn("[bold]Measured[/]").Centered());

                        table.AddRow("Voltage", string.Format("[yellow]{0:F3}V[/]", voltSet), string.Format("[cyan]{0:F3}V[/]", voltMeas));
                        table.AddRow("Current", string.Format("[yellow]{0:F3}A[/]", currSet), string.Format("[cyan]{0:F3}A[/]", currMeas));
                        table.AddRow("Power",   "-",                                           string.Format("[cyan]{0:F3}W[/]", power));
                        table.AddEmptyRow();
                        table.AddRow("OVP Level",   string.Format("[yellow]{0:F3}V[/]", ovpLevel), "-");
                        table.AddRow("OVP State",   ovpEnabled ? "[green]Enabled[/]" : "[red]Disabled[/]", "-");
                        table.AddRow("OVP Tripped", ovpTripped ? "[red]Yes[/]" : "[grey]No[/]", "-");
                        table.AddRow("OCP Level",   string.Format("[yellow]{0:F3}A[/]", ocpLevel), "-");
                        table.AddRow("OCP State",   ocpEnabled ? "[green]Enabled[/]" : "[red]Disabled[/]", "-");
                        table.AddRow("OCP Tripped", ocpTripped ? "[red]Yes[/]" : "[grey]No[/]", "-");
                        table.AddEmptyRow();
                        table.AddRow("Output State", outEnabled ? "[green]On[/]" : "[grey]Off[/]", "-");

                        AnsiConsole.Write(table);

                        if (ovpTripped || ocpTripped)
                        {
                            AnsiConsole.WriteLine();
                            AnsiConsole.MarkupLine("[red]\u26a0 One or more protection trips detected![/] The channel output has been turned off.");
                            AnsiConsole.MarkupLine("[grey]Use 'clear-trip' to clear the trip and re-enable the output.[/]");
                        }
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

        private static double ParseDouble(string raw)
        {
            double value;
            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return value;
            return 0.0;
        }
    }
}
