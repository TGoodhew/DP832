using System;
using DP832.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DP832.CLI.Commands
{
    /// <summary>
    /// Resets the DP832 to factory defaults using the *RST IEEE 488.2 command.
    /// </summary>
    public sealed class ResetCommand : Command<DeviceSettings>
    {
        /// <inheritdoc/>
        public override int Execute(CommandContext context, DeviceSettings settings)
        {
            using (var device = new DP832Device(settings.Address))
            {
                try
                {
                    device.Connect();
                    device.SendCommand("*RST");
                    AnsiConsole.MarkupLine("[green]Device reset to factory defaults.[/]");
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
