using System;
using System.Collections.Generic;
using System.Threading;
using DP832.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DP832.CLI.Commands
{
    /// <summary>
    /// Resets the DP832 to factory defaults using the *RST IEEE 488.2 command.
    /// Pass <c>--json</c> to receive the result as a JSON object.
    /// </summary>
    public sealed class ResetCommand : Command<DeviceSettings>
    {
        /// <inheritdoc/>
        public override int Execute(CommandContext context, DeviceSettings settings, CancellationToken cancellationToken)
        {
            using (var device = new DP832Device(settings.Address))
            {
                try
                {
                    device.Connect();
                    device.SendCommand("*RST");

                    if (settings.Json)
                    {
                        Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object>
                        {
                            { "success", true },
                            { "message", "Device reset to factory defaults." }
                        }));
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[green]Device reset to factory defaults.[/]");
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
