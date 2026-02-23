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
                    .WithExample(new[] { "identify", "--address", "GPIB0::1::INSTR" });

                config.AddCommand<StatusCommand>("status")
                    .WithDescription("Display current voltage, current, and power readings for all channels.")
                    .WithExample(new[] { "status", "--address", "GPIB0::1::INSTR" });

                config.AddCommand<SetVoltageCommand>("set-voltage")
                    .WithDescription("Set the output voltage for a channel.")
                    .WithExample(new[] { "set-voltage", "--address", "GPIB0::1::INSTR", "--channel", "1", "--voltage", "5.0" });

                config.AddCommand<SetCurrentCommand>("set-current")
                    .WithDescription("Set the current limit for a channel.")
                    .WithExample(new[] { "set-current", "--address", "GPIB0::1::INSTR", "--channel", "1", "--current", "1.5" });

                config.AddCommand<OutputCommand>("output")
                    .WithDescription("Enable or disable the output for a channel.")
                    .WithExample(new[] { "output", "--address", "GPIB0::1::INSTR", "--channel", "1", "--state", "on" });

                config.AddCommand<ResetCommand>("reset")
                    .WithDescription("Reset the device to factory defaults (*RST).")
                    .WithExample(new[] { "reset", "--address", "GPIB0::1::INSTR" });
            });

            return app.Run(args);
        }
    }
}
