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
    /// Displays a comprehensive status for all three DP832 channels (set and measured voltage,
    /// current, power, OVP/OCP configuration and trip state, output state) plus system settings
    /// (tracking, OTP, beeper, brightness, screen saver).
    /// Pass <c>--json</c> to receive the result as a JSON object.
    /// </summary>
    public sealed class StatusCommand : Command<DeviceSettings>
    {
        /// <inheritdoc/>
        public override int Execute(CommandContext context, DeviceSettings settings, CancellationToken cancellationToken)
        {
            using (var device = new DP832Device(settings.Address))
            {
                try
                {
                    device.Connect();

                    // ── Collect per-channel data ──────────────────────────────────────
                    double[] voltSet    = new double[3];
                    double[] voltMeas   = new double[3];
                    double[] currSet    = new double[3];
                    double[] currMeas   = new double[3];
                    double[] power      = new double[3];
                    double[] ovpLevel   = new double[3];
                    bool[]   ovpEnabled = new bool[3];
                    bool[]   ovpTripped = new bool[3];
                    double[] ocpLevel   = new double[3];
                    bool[]   ocpEnabled = new bool[3];
                    bool[]   ocpTripped = new bool[3];
                    bool[]   outEnabled = new bool[3];
                    bool[]   chError    = new bool[3];

                    for (int i = 0; i < 3; i++)
                    {
                        int ch = i + 1;
                        string chName = "CH" + ch;
                        try
                        {
                            voltSet[i]    = ParseDouble(device.SendQuery(":SOURce" + ch + ":VOLTage?"));
                            voltMeas[i]   = ParseDouble(device.SendQuery(":MEASure:VOLTage? " + chName));
                            currSet[i]    = ParseDouble(device.SendQuery(":SOURce" + ch + ":CURRent?"));
                            currMeas[i]   = ParseDouble(device.SendQuery(":MEASure:CURRent? " + chName));
                            power[i]      = ParseDouble(device.SendQuery(":MEASure:POWEr? " + chName));
                            ovpLevel[i]   = ParseDouble(device.SendQuery(":SOURce" + ch + ":VOLTage:PROTection?"));
                            ovpEnabled[i] = DeviceHelpers.ParseProtectionState(device.SendQuery(":SOURce" + ch + ":VOLTage:PROTection:STATe?"));
                            ocpLevel[i]   = ParseDouble(device.SendQuery(":SOURce" + ch + ":CURRent:PROTection?"));
                            ocpEnabled[i] = DeviceHelpers.ParseProtectionState(device.SendQuery(":SOURce" + ch + ":CURRent:PROTection:STATe?"));
                            outEnabled[i] = DeviceHelpers.ParseProtectionState(device.SendQuery(":OUTPut? " + chName));
                        }
                        catch { chError[i] = true; }

                        // Trip queries are separate so a failure here does not mark the whole channel as error.
                        try { ovpTripped[i] = DeviceHelpers.ParseProtectionState(device.SendQuery(":SOURce" + ch + ":VOLTage:PROTection:TRIP?")); }
                        catch { /* default: false */ }
                        try { ocpTripped[i] = DeviceHelpers.ParseProtectionState(device.SendQuery(":SOURce" + ch + ":CURRent:PROTection:TRIP?")); }
                        catch { /* default: false */ }
                    }

                    // ── Collect system data ───────────────────────────────────────────
                    string trackMode     = QuerySafe(device, ":SYSTem:TRACKMode?");
                    bool   trackCh1      = SafeBool(device, ":OUTPut:TRACk? CH1");
                    bool   trackCh2      = SafeBool(device, ":OUTPut:TRACk? CH2");
                    bool?  otpEnabled    = SafeBoolNullable(device, ":SYSTem:OTP?");
                    bool?  beeperEnabled = SafeBoolNullable(device, ":SYSTem:BEEPer?");
                    string brightness    = QuerySafe(device, ":SYSTem:BRIGhtness?");
                    bool?  ssaverEnabled = SafeBoolNullable(device, ":SYSTem:SAVer?");

                    // ── Output ────────────────────────────────────────────────────────
                    if (settings.Json)
                    {
                        var channelList = new List<Dictionary<string, object>>();
                        for (int i = 0; i < 3; i++)
                        {
                            var chObj = new Dictionary<string, object>();
                            chObj["channel"] = "CH" + (i + 1);
                            if (chError[i])
                            {
                                chObj["error"] = true;
                            }
                            else
                            {
                                chObj["voltageSet"]     = voltSet[i];
                                chObj["voltageMeasured"]= voltMeas[i];
                                chObj["currentSet"]     = currSet[i];
                                chObj["currentMeasured"]= currMeas[i];
                                chObj["powerMeasured"]  = power[i];
                                chObj["ovpLevel"]       = ovpLevel[i];
                                chObj["ovpEnabled"]     = ovpEnabled[i];
                                chObj["ovpTripped"]     = ovpTripped[i];
                                chObj["ocpLevel"]       = ocpLevel[i];
                                chObj["ocpEnabled"]     = ocpEnabled[i];
                                chObj["ocpTripped"]     = ocpTripped[i];
                                chObj["outputEnabled"]  = outEnabled[i];
                            }
                            channelList.Add(chObj);
                        }

                        var sysObj = new Dictionary<string, object>();
                        sysObj["trackMode"]          = trackMode;
                        sysObj["trackCH1"]           = trackCh1;
                        sysObj["trackCH2"]           = trackCh2;
                        sysObj["otpEnabled"]         = otpEnabled.HasValue ? (object)otpEnabled.Value : null;
                        sysObj["beeperEnabled"]      = beeperEnabled.HasValue ? (object)beeperEnabled.Value : null;
                        sysObj["brightness"]         = brightness;
                        sysObj["screenSaverEnabled"] = ssaverEnabled.HasValue ? (object)ssaverEnabled.Value : null;

                        var root = new Dictionary<string, object>
                        {
                            { "channels", channelList },
                            { "system",   sysObj }
                        };
                        Console.WriteLine(JsonBuilder.Serialize(root));
                    }
                    else
                    {
                        // Channel table
                        var ct = new Table()
                            .Border(TableBorder.Rounded)
                            .AddColumn("[bold]Parameter[/]")
                            .AddColumn("[bold cyan]CH1[/]")
                            .AddColumn("[bold cyan]CH2[/]")
                            .AddColumn("[bold cyan]CH3[/]");

                        ct.AddRow("Voltage (Set)",
                            chError[0] ? "[red]Error[/]" : string.Format("[yellow]{0:F3}V[/]", voltSet[0]),
                            chError[1] ? "[red]Error[/]" : string.Format("[yellow]{0:F3}V[/]", voltSet[1]),
                            chError[2] ? "[red]Error[/]" : string.Format("[yellow]{0:F3}V[/]", voltSet[2]));
                        ct.AddRow("Voltage (Meas)",
                            chError[0] ? "[red]Error[/]" : string.Format("[cyan]{0:F3}V[/]", voltMeas[0]),
                            chError[1] ? "[red]Error[/]" : string.Format("[cyan]{0:F3}V[/]", voltMeas[1]),
                            chError[2] ? "[red]Error[/]" : string.Format("[cyan]{0:F3}V[/]", voltMeas[2]));
                        ct.AddRow("Current (Set)",
                            chError[0] ? "[red]Error[/]" : string.Format("[yellow]{0:F3}A[/]", currSet[0]),
                            chError[1] ? "[red]Error[/]" : string.Format("[yellow]{0:F3}A[/]", currSet[1]),
                            chError[2] ? "[red]Error[/]" : string.Format("[yellow]{0:F3}A[/]", currSet[2]));
                        ct.AddRow("Current (Meas)",
                            chError[0] ? "[red]Error[/]" : string.Format("[cyan]{0:F3}A[/]", currMeas[0]),
                            chError[1] ? "[red]Error[/]" : string.Format("[cyan]{0:F3}A[/]", currMeas[1]),
                            chError[2] ? "[red]Error[/]" : string.Format("[cyan]{0:F3}A[/]", currMeas[2]));
                        ct.AddRow("Power (Meas)",
                            chError[0] ? "[red]Error[/]" : string.Format("[cyan]{0:F3}W[/]", power[0]),
                            chError[1] ? "[red]Error[/]" : string.Format("[cyan]{0:F3}W[/]", power[1]),
                            chError[2] ? "[red]Error[/]" : string.Format("[cyan]{0:F3}W[/]", power[2]));
                        ct.AddEmptyRow();
                        ct.AddRow("OVP Level",
                            chError[0] ? "[red]Error[/]" : string.Format("[yellow]{0:F3}V[/]", ovpLevel[0]),
                            chError[1] ? "[red]Error[/]" : string.Format("[yellow]{0:F3}V[/]", ovpLevel[1]),
                            chError[2] ? "[red]Error[/]" : string.Format("[yellow]{0:F3}V[/]", ovpLevel[2]));
                        ct.AddRow("OVP State",
                            chError[0] ? "[red]Error[/]" : (ovpEnabled[0] ? "[green]Enabled[/]" : "[red]Disabled[/]"),
                            chError[1] ? "[red]Error[/]" : (ovpEnabled[1] ? "[green]Enabled[/]" : "[red]Disabled[/]"),
                            chError[2] ? "[red]Error[/]" : (ovpEnabled[2] ? "[green]Enabled[/]" : "[red]Disabled[/]"));
                        ct.AddRow("OVP Tripped",
                            chError[0] ? "[red]Error[/]" : (ovpTripped[0] ? "[red]Yes[/]" : "[grey]No[/]"),
                            chError[1] ? "[red]Error[/]" : (ovpTripped[1] ? "[red]Yes[/]" : "[grey]No[/]"),
                            chError[2] ? "[red]Error[/]" : (ovpTripped[2] ? "[red]Yes[/]" : "[grey]No[/]"));
                        ct.AddRow("OCP Level",
                            chError[0] ? "[red]Error[/]" : string.Format("[yellow]{0:F3}A[/]", ocpLevel[0]),
                            chError[1] ? "[red]Error[/]" : string.Format("[yellow]{0:F3}A[/]", ocpLevel[1]),
                            chError[2] ? "[red]Error[/]" : string.Format("[yellow]{0:F3}A[/]", ocpLevel[2]));
                        ct.AddRow("OCP State",
                            chError[0] ? "[red]Error[/]" : (ocpEnabled[0] ? "[green]Enabled[/]" : "[red]Disabled[/]"),
                            chError[1] ? "[red]Error[/]" : (ocpEnabled[1] ? "[green]Enabled[/]" : "[red]Disabled[/]"),
                            chError[2] ? "[red]Error[/]" : (ocpEnabled[2] ? "[green]Enabled[/]" : "[red]Disabled[/]"));
                        ct.AddRow("OCP Tripped",
                            chError[0] ? "[red]Error[/]" : (ocpTripped[0] ? "[red]Yes[/]" : "[grey]No[/]"),
                            chError[1] ? "[red]Error[/]" : (ocpTripped[1] ? "[red]Yes[/]" : "[grey]No[/]"),
                            chError[2] ? "[red]Error[/]" : (ocpTripped[2] ? "[red]Yes[/]" : "[grey]No[/]"));
                        ct.AddEmptyRow();
                        ct.AddRow("Output State",
                            chError[0] ? "[red]Error[/]" : (outEnabled[0] ? "[green]On[/]" : "[grey]Off[/]"),
                            chError[1] ? "[red]Error[/]" : (outEnabled[1] ? "[green]On[/]" : "[grey]Off[/]"),
                            chError[2] ? "[red]Error[/]" : (outEnabled[2] ? "[green]On[/]" : "[grey]Off[/]"));

                        AnsiConsole.Write(ct);

                        // System settings table
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[bold cyan]System Status:[/]");

                        var st = new Table()
                            .Border(TableBorder.Rounded)
                            .AddColumn("[bold]Setting[/]")
                            .AddColumn("[bold]Value[/]");

                        st.AddRow("Track Mode",                Markup.Escape(trackMode));
                        st.AddRow("Track (CH1/CH2)",
                            (trackCh1 ? "[green]CH1 On[/]" : "[grey]CH1 Off[/]") + " / " +
                            (trackCh2 ? "[green]CH2 On[/]" : "[grey]CH2 Off[/]"));
                        st.AddRow("OTP",         otpEnabled.HasValue    ? (otpEnabled.Value    ? "[green]Enabled[/]" : "[red]Disabled[/]") : "[red]Error[/]");
                        st.AddRow("Beeper",      beeperEnabled.HasValue ? (beeperEnabled.Value ? "[green]Enabled[/]" : "[grey]Disabled[/]") : "[red]Error[/]");
                        st.AddRow("Brightness",  string.IsNullOrEmpty(brightness) ? "[red]Error[/]" : Markup.Escape(brightness.Trim()) + "%");
                        st.AddRow("Screen Saver",ssaverEnabled.HasValue ? (ssaverEnabled.Value ? "[green]Enabled[/]" : "[grey]Disabled[/]") : "[red]Error[/]");

                        AnsiConsole.Write(st);
                    }

                    return 0;
                }
                catch (Exception ex)
                {
                    if (settings.Json)
                    {
                        var err = new Dictionary<string, object>
                        {
                            { "success", false },
                            { "error", ex.Message }
                        };
                        Console.WriteLine(JsonBuilder.Serialize(err));
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[red]Error:[/] " + Markup.Escape(ex.Message));
                    }
                    return 1;
                }
            }
        }

        private static string QuerySafe(IDP832Device device, string query)
        {
            try { return device.SendQuery(query); }
            catch { return ""; }
        }

        private static bool SafeBool(IDP832Device device, string query)
        {
            try { return DeviceHelpers.ParseProtectionState(device.SendQuery(query)); }
            catch { return false; }
        }

        private static bool? SafeBoolNullable(IDP832Device device, string query)
        {
            try { return DeviceHelpers.ParseProtectionState(device.SendQuery(query)); }
            catch { return null; }
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
