using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Threading;
using DP832.Core;
using DP832.Helpers;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DP832.CLI.Commands
{
    /// <summary>
    /// Sets the display brightness (1–100%) via <c>:SYSTem:BRIGhtness</c>.
    /// Pass <c>--json</c> to receive the result as a JSON object.
    /// </summary>
    public sealed class SetBrightnessCommand : Command<SetBrightnessCommand.Settings>
    {
        /// <summary>Settings for the set-brightness command.</summary>
        public sealed class Settings : DeviceSettings
        {
            /// <summary>Brightness percentage: 1–100.</summary>
            [Description("Display brightness percentage (1\u2013100).")]
            [CommandArgument(0, "<brightness>")]
            public int Brightness { get; set; }
        }

        /// <inheritdoc/>
        public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            if (!DeviceHelpers.IsValidBrightness(settings.Brightness))
            {
                string msg = string.Format(
                    CultureInfo.InvariantCulture,
                    "Brightness {0} is outside the valid range 1\u2013100.", settings.Brightness);
                if (settings.Json)
                    Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object> { { "success", false }, { "error", msg } }));
                else
                {
                    AnsiConsole.MarkupLine("[red]Error:[/] " + msg);
                    AnsiConsole.MarkupLine("[grey]Usage: dp832 set-brightness <brightness> [[--address <address>]][/]");
                }
                return 1;
            }

            using (var device = new DP832Device(settings.Address))
            {
                try
                {
                    device.Connect();
                    device.SendCommand(":SYSTem:BRIGhtness " + settings.Brightness.ToString(CultureInfo.InvariantCulture));

                    if (settings.Json)
                    {
                        Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object>
                        {
                            { "success",    true },
                            { "brightness", settings.Brightness }
                        }));
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[green]Display brightness set to " + settings.Brightness + "%.[/]");
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    if (settings.Json)
                        Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object> { { "success", false }, { "error", ex.Message } }));
                    else
                        AnsiConsole.MarkupLine("[red]Error:[/] " + Markup.Escape(ex.Message));
                    return 1;
                }
            }
        }
    }
}
