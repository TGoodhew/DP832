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
    /// Configures the Over Current Protection (OCP) level and state for the specified channel.
    /// Omit <c>--level</c> to leave the level unchanged; omit <c>--state</c> to leave the
    /// enabled/disabled state unchanged.
    /// Pass <c>--json</c> to receive the result as a JSON object.
    /// </summary>
    public sealed class SetOcpCommand : Command<SetOcpCommand.Settings>
    {
        /// <summary>Settings for the set-ocp command.</summary>
        public sealed class Settings : ChannelSettings
        {
            /// <summary>OCP level in amps (all channels: 0.001–4 A). Omit to leave unchanged.</summary>
            [Description("OCP level in amps (all channels: 0.001\u20134 A). Omit to leave unchanged.")]
            [CommandOption("-l|--level")]
            public double? Level { get; set; }

            /// <summary>Enable or disable OCP: <c>on</c> or <c>off</c>. Omit to leave unchanged.</summary>
            [Description("Enable or disable OCP: on or off. Omit to leave unchanged.")]
            [CommandOption("-s|--state")]
            public string State { get; set; }
        }

        /// <inheritdoc/>
        public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            int ch = settings.Channel;

            if (settings.Level.HasValue && !DeviceHelpers.IsValidOcpLevel(settings.Level.Value))
            {
                double maxA = DeviceHelpers.GetChannelMaxCurrent();
                string msg = string.Format(
                    CultureInfo.InvariantCulture,
                    "OCP level {0} A is outside the valid range 0.001\u2013{1} A.",
                    settings.Level.Value, maxA + 1);
                return Fail(settings, msg);
            }

            bool? turnOn = null;
            if (!string.IsNullOrEmpty(settings.State))
            {
                if (string.Equals(settings.State, "on", StringComparison.OrdinalIgnoreCase))
                    turnOn = true;
                else if (string.Equals(settings.State, "off", StringComparison.OrdinalIgnoreCase))
                    turnOn = false;
                else
                    return Fail(settings, "--state must be 'on' or 'off'.");
            }

            using (var device = new DP832Device(settings.Address))
            {
                try
                {
                    device.Connect();

                    if (settings.Level.HasValue)
                        device.SendCommand(string.Format(CultureInfo.InvariantCulture,
                            ":SOURce{0}:CURRent:PROTection {1:F3}", ch, settings.Level.Value));

                    if (turnOn.HasValue)
                        device.SendCommand(":SOURce" + ch + ":CURRent:PROTection:STATe " + (turnOn.Value ? "ON" : "OFF"));

                    if (settings.Json)
                    {
                        var obj = new Dictionary<string, object> { { "success", true }, { "channel", ch } };
                        if (settings.Level.HasValue) obj["level"] = settings.Level.Value;
                        if (turnOn.HasValue)         obj["enabled"] = turnOn.Value;
                        Console.WriteLine(JsonBuilder.Serialize(obj));
                    }
                    else
                    {
                        if (settings.Level.HasValue)
                            AnsiConsole.MarkupLine(string.Format(CultureInfo.InvariantCulture,
                                "[green]CH{0} OCP level set to {1:F3} A.[/]", ch, settings.Level.Value));
                        if (turnOn.HasValue)
                            AnsiConsole.MarkupLine("[green]CH" + ch + " OCP " + (turnOn.Value ? "enabled" : "disabled") + ".[/]");
                    }
                    return 0;
                }
                catch (Exception ex)
                {
                    return Fail(settings, ex.Message);
                }
            }
        }

        private static int Fail(Settings settings, string message)
        {
            if (settings.Json)
            {
                Console.WriteLine(JsonBuilder.Serialize(new Dictionary<string, object>
                {
                    { "success", false },
                    { "error", message }
                }));
            }
            else
            {
                AnsiConsole.MarkupLine("[red]Error:[/] " + Markup.Escape(message));
                AnsiConsole.MarkupLine("[grey]Usage: dp832 set-ocp [[-c <n>]] [[-l <a>]] [[-s <on|off>]] [[-a <address>]][/]");
            }
            return 1;
        }
    }
}
