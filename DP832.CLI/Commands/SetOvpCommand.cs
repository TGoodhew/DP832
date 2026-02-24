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
    /// Configures the Over Voltage Protection (OVP) level and state for the specified channel.
    /// Omit <c>--level</c> to leave the level unchanged; omit <c>--state</c> to leave the
    /// enabled/disabled state unchanged.
    /// Pass <c>--json</c> to receive the result as a JSON object.
    /// </summary>
    public sealed class SetOvpCommand : Command<SetOvpCommand.Settings>
    {
        /// <summary>Settings for the set-ovp command.</summary>
        public sealed class Settings : ChannelSettings
        {
            /// <summary>OVP level in volts (CH1/CH2: 0.01–31 V, CH3: 0.01–6 V). Omit to leave unchanged.</summary>
            [Description("OVP level in volts (CH1/CH2: 0.01\u201331 V, CH3: 0.01\u20136 V). Omit to leave unchanged.")]
            [CommandOption("-l|--level")]
            public double? Level { get; set; }

            /// <summary>Enable or disable OVP: <c>on</c> or <c>off</c>. Omit to leave unchanged.</summary>
            [Description("Enable or disable OVP: on or off. Omit to leave unchanged.")]
            [CommandOption("-s|--state")]
            public string State { get; set; }
        }

        /// <inheritdoc/>
        public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            int ch = settings.Channel;

            // Validate level if provided
            if (settings.Level.HasValue && !DeviceHelpers.IsValidOvpLevel(settings.Level.Value, ch))
            {
                double maxV = DeviceHelpers.GetChannelMaxVoltage(ch);
                string msg = string.Format(
                    CultureInfo.InvariantCulture,
                    "OVP level {0} V is outside the valid range 0.01\u2013{1} V for CH{2}.",
                    settings.Level.Value, maxV + 1, ch);
                return Fail(settings, msg);
            }

            // Validate state if provided
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
                            ":SOURce{0}:VOLTage:PROTection {1:F3}", ch, settings.Level.Value));

                    if (turnOn.HasValue)
                        device.SendCommand(":SOURce" + ch + ":VOLTage:PROTection:STATe " + (turnOn.Value ? "ON" : "OFF"));

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
                                "[green]CH{0} OVP level set to {1:F3} V.[/]", ch, settings.Level.Value));
                        if (turnOn.HasValue)
                            AnsiConsole.MarkupLine("[green]CH" + ch + " OVP " + (turnOn.Value ? "enabled" : "disabled") + ".[/]");
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
                AnsiConsole.MarkupLine("[grey]Usage: dp832 set-ovp [[-c <n>]] [[-l <v>]] [[-s <on|off>]] [[-a <address>]][/]");
            }
            return 1;
        }
    }
}
