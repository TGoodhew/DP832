using System;
using DP832.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DP832.CLI.Commands
{
    /// <summary>
    /// Queries the instrument identification string via *IDN? and writes it to the console.
    /// </summary>
    public sealed class IdentifyCommand : Command<DeviceSettings>
    {
        /// <inheritdoc/>
        public override int Execute(CommandContext context, DeviceSettings settings)
        {
            using (var device = new DP832Device(settings.Address))
            {
                try
                {
                    device.Connect();
                    string idn = device.GetIdentification();
                    AnsiConsole.MarkupLine("[green]Device:[/] " + idn);
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
