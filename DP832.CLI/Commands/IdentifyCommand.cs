using System;
using System.Collections.Generic;
using System.Threading;
using DP832.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DP832.CLI.Commands
{
    /// <summary>
    /// Queries the instrument identification string via *IDN? and writes it to the console.
    /// Pass <c>--json</c> to receive the result as a JSON object.
    /// </summary>
    public sealed class IdentifyCommand : Command<DeviceSettings>
    {
        /// <inheritdoc/>
        public override int Execute(CommandContext context, DeviceSettings settings, CancellationToken cancellationToken)
        {
            using (var device = new DP832Device(settings.Address))
            {
                try
                {
                    device.Connect();
                    string idn = device.GetIdentification();

                    if (settings.Json)
                    {
                        var obj = new Dictionary<string, object>
                        {
                            { "identification", idn }
                        };
                        Console.WriteLine(JsonBuilder.Serialize(obj));
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[green]Device:[/] " + Markup.Escape(idn));
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
    }
}
