using System;
using System.Globalization;
using System.Threading;
using DP832.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DP832.CLI.Commands
{
    /// <summary>
    /// Displays the measured voltage, current, and power for all three DP832 channels.
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

                    var table = new Table()
                        .Border(TableBorder.Rounded)
                        .AddColumn("[bold]Channel[/]")
                        .AddColumn("[bold]Voltage (V)[/]")
                        .AddColumn("[bold]Current (A)[/]")
                        .AddColumn("[bold]Power (W)[/]")
                        .AddColumn("[bold]Output[/]");

                    for (int ch = 1; ch <= 3; ch++)
                    {
                        string chName = "CH" + ch;
                        string voltage = QuerySafe(device, ":MEASure:VOLTage? " + chName);
                        string current = QuerySafe(device, ":MEASure:CURRent? " + chName);
                        string power   = QuerySafe(device, ":MEASure:POWEr? " + chName);
                        string output  = QuerySafe(device, ":OUTPut? " + chName);

                        string outputDisplay = string.Equals(output, "ON", StringComparison.OrdinalIgnoreCase)
                            ? "[green]ON[/]"
                            : "[red]OFF[/]";

                        table.AddRow(
                            chName,
                            FormatMeasurement(voltage),
                            FormatMeasurement(current),
                            FormatMeasurement(power),
                            outputDisplay);
                    }

                    AnsiConsole.Write(table);
                    return 0;
                }
                catch (Exception ex)
                {
                    AnsiConsole.MarkupLine("[red]Error:[/] " + ex.Message);
                    return 1;
                }
            }
        }

        private static string QuerySafe(IDP832Device device, string query)
        {
            try { return device.SendQuery(query); }
            catch { return "N/A"; }
        }

        private static string FormatMeasurement(string raw)
        {
            double value;
            if (double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                return value.ToString("F3", CultureInfo.InvariantCulture);
            return raw;
        }
    }
}
