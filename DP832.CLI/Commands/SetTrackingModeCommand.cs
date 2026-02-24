using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using DP832.Core;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DP832.CLI.Commands
{
    /// <summary>
    /// Sets the channel tracking mode to <c>SYNC</c> (synchronised) or <c>INDE</c> (independent)
    /// via <c>:SYSTem:TRACKMode</c>. Tracking links CH1 and CH2 so voltage changes on one channel
    /// are mirrored on the other when in SYNC mode.
    /// Pass <c>--json</c> to receive the result as a JSON object.
    /// </summary>
    public sealed class SetTrackingModeCommand : Command<SetTrackingModeCommand.Settings>
    {
        /// <summary>Settings for the set-tracking-mode command.</summary>
        public sealed class Settings : DeviceSettings
        {
            /// <summary>Tracking mode: SYNC (synchronised) or INDE (independent).</summary>
            [Description("Tracking mode: SYNC (synchronised) or INDE (independent).")]
            [CommandArgument(0, "<mode>")]
            public string Mode { get; set; }
        }

        /// <inheritdoc/>
        public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            string mode;
            if (string.Equals(settings.Mode, "SYNC", StringComparison.OrdinalIgnoreCase))
                mode = "SYNC";
            else if (string.Equals(settings.Mode, "INDE", StringComparison.OrdinalIgnoreCase))
                mode = "INDE";
            else
            {
                string msg = "--mode must be 'SYNC' or 'INDE'.";
                if (settings.Json)
                    Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object> { { "success", false }, { "error", msg } }));
                else
                {
                    AnsiConsole.MarkupLine("[red]Error:[/] " + msg);
                    AnsiConsole.MarkupLine("[grey]Usage: dp832 set-tracking-mode <SYNC|INDE> [[--address <address>]][/]");
                }
                return 1;
            }

            using (var device = new DP832Device(settings.Address))
            {
                try
                {
                    device.Connect();
                    device.SendCommand(":SYSTem:TRACKMode " + mode);

                    if (settings.Json)
                    {
                        Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object>
                        {
                            { "success", true },
                            { "mode",    mode }
                        }));
                    }
                    else
                    {
                        AnsiConsole.MarkupLine("[green]Track mode set to " + mode + ".[/]");
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
