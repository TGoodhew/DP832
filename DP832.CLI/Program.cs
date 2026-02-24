using DP832.CLI.Commands;
using Spectre.Console.Cli;

namespace DP832.CLI
{
    /// <summary>
    /// Entry point for the DP832 command-line interface.
    /// Built with <see href="https://spectreconsole.net/cli">Spectre.Console.Cli</see>.
    /// </summary>
    class Program
    {
        static int Main(string[] args)
        {
            var app = new CommandApp();

            app.Configure(config =>
            {
                config.SetApplicationName("dp832");
                config.SetApplicationVersion("1.0.0");

                config.AddCommand<IdentifyCommand>("identify")
                    .WithDescription("Query the device identification string (*IDN?).")
                    .WithExample(new[] { "identify", "--address", "GPIB0::1::INSTR" })
                    .WithExample(new[] { "identify", "--address", "GPIB0::1::INSTR", "--json" });

                config.AddCommand<StatusCommand>("status")
                    .WithDescription("Display comprehensive status for all channels (set/measured values, OVP/OCP, output) and system settings.")
                    .WithExample(new[] { "status", "--address", "GPIB0::1::INSTR" })
                    .WithExample(new[] { "status", "--address", "GPIB0::1::INSTR", "--json" });

                config.AddCommand<ChannelStatusCommand>("channel-status")
                    .WithDescription("Display detailed settings and measurements for a single channel.")
                    .WithExample(new[] { "channel-status", "--address", "GPIB0::1::INSTR", "--channel", "1" })
                    .WithExample(new[] { "channel-status", "--address", "GPIB0::1::INSTR", "--channel", "2", "--json" });

                config.AddCommand<SetVoltageCommand>("set-voltage")
                    .WithDescription("Set the output voltage for a channel (CH1/CH2: 0–30 V, CH3: 0–5 V).")
                    .WithExample(new[] { "set-voltage", "--address", "GPIB0::1::INSTR", "--channel", "1", "--voltage", "5.0" });

                config.AddCommand<SetCurrentCommand>("set-current")
                    .WithDescription("Set the current limit for a channel (all channels: 0–3 A).")
                    .WithExample(new[] { "set-current", "--address", "GPIB0::1::INSTR", "--channel", "1", "--current", "1.5" });

                config.AddCommand<SetOvpCommand>("set-ovp")
                    .WithDescription("Configure Over Voltage Protection (OVP) level and/or state for a channel.")
                    .WithExample(new[] { "set-ovp", "--address", "GPIB0::1::INSTR", "--channel", "1", "--level", "31.0", "--state", "on" })
                    .WithExample(new[] { "set-ovp", "--address", "GPIB0::1::INSTR", "--channel", "1", "--state", "off" });

                config.AddCommand<SetOcpCommand>("set-ocp")
                    .WithDescription("Configure Over Current Protection (OCP) level and/or state for a channel.")
                    .WithExample(new[] { "set-ocp", "--address", "GPIB0::1::INSTR", "--channel", "1", "--level", "2.0", "--state", "on" })
                    .WithExample(new[] { "set-ocp", "--address", "GPIB0::1::INSTR", "--channel", "1", "--state", "off" });

                config.AddCommand<ClearTripCommand>("clear-trip")
                    .WithDescription("Clear latched OVP/OCP protection trips for a channel so the output can be re-enabled.")
                    .WithExample(new[] { "clear-trip", "--address", "GPIB0::1::INSTR", "--channel", "1" });

                config.AddCommand<OutputCommand>("output")
                    .WithDescription("Enable or disable the output for a channel.")
                    .WithExample(new[] { "output", "--address", "GPIB0::1::INSTR", "--channel", "1", "--state", "on" });

                config.AddCommand<SetOtpCommand>("set-otp")
                    .WithDescription("Enable or disable the Over Temperature Protection (OTP) feature.")
                    .WithExample(new[] { "set-otp", "on", "--address", "GPIB0::1::INSTR" });

                config.AddCommand<SetBeeperCommand>("set-beeper")
                    .WithDescription("Enable or disable the instrument beeper.")
                    .WithExample(new[] { "set-beeper", "off", "--address", "GPIB0::1::INSTR" });

                config.AddCommand<SetBrightnessCommand>("set-brightness")
                    .WithDescription("Set the display brightness (1–100%).")
                    .WithExample(new[] { "set-brightness", "75", "--address", "GPIB0::1::INSTR" });

                config.AddCommand<SetScreenSaverCommand>("set-screensaver")
                    .WithDescription("Enable or disable the display screen saver (activates after 25 minutes of standby).")
                    .WithExample(new[] { "set-screensaver", "on", "--address", "GPIB0::1::INSTR" });

                config.AddCommand<SetTrackingModeCommand>("set-tracking-mode")
                    .WithDescription("Set the channel tracking mode: SYNC (synchronised) or INDE (independent).")
                    .WithExample(new[] { "set-tracking-mode", "SYNC", "--address", "GPIB0::1::INSTR" });

                config.AddCommand<SetTrackCommand>("set-track")
                    .WithDescription("Enable or disable per-channel output tracking for CH1 or CH2.")
                    .WithExample(new[] { "set-track", "--address", "GPIB0::1::INSTR", "--channel", "1", "--state", "on" });

                config.AddCommand<ResetCommand>("reset")
                    .WithDescription("Reset the device to factory defaults (*RST).")
                    .WithExample(new[] { "reset", "--address", "GPIB0::1::INSTR" });

                config.AddCommand<HelpCommand>("help")
                    .WithDescription("Show a full command reference listing every command, its parameters, and a usage example.")
                    .WithExample(new[] { "help" });
            });

            return app.Run(args);
        }
    }
}
