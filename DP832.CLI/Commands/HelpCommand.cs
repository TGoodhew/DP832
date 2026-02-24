using System.Threading;
using Spectre.Console;
using Spectre.Console.Cli;

namespace DP832.CLI.Commands
{
    /// <summary>
    /// Displays a full command reference listing every dp832 command with its
    /// arguments, options, and a usage example.
    /// </summary>
    public sealed class HelpCommand : Command<HelpCommand.Settings>
    {
        /// <summary>Settings for the help command (no additional parameters).</summary>
        public sealed class Settings : CommandSettings { }

        private const string ParamIndent = "    ";
        private const int ParamColWidth = 30; // total width of parameter column including indent

        /// <inheritdoc/>
        public override int Execute(CommandContext context, Settings settings, CancellationToken cancellationToken)
        {
            AnsiConsole.MarkupLine("[bold cyan]dp832[/] \u2013 Rigol DP832 Power Supply CLI");
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine("[bold]GLOBAL OPTIONS[/]  [grey](apply to every command)[/]");
            Param("-a|--address <addr>", "VISA address (default: GPIB0::1::INSTR). Accepts:");
            AnsiConsole.MarkupLine(new string(' ', ParamColWidth) + "  \u2022 GPIB device number     e.g. [grey]1[/]              \u2192 GPIB0::1::INSTR");
            AnsiConsole.MarkupLine(new string(' ', ParamColWidth) + "  \u2022 Last IP octet          e.g. [grey]136[/]            \u2192 TCPIP::192.168.1.136::INSTR");
            AnsiConsole.MarkupLine(new string(' ', ParamColWidth) + "  \u2022 Full IPv4 address      e.g. [grey]192.168.1.136[/]");
            AnsiConsole.MarkupLine(new string(' ', ParamColWidth) + "  \u2022 Full VISA string        e.g. [grey]GPIB0::1::INSTR[/]");
            Param("--json", "Return output as JSON instead of formatted text");
            AnsiConsole.WriteLine();

            AnsiConsole.MarkupLine("[bold]COMMANDS[/]");

            Cmd("identify", "Query the device identification string (*IDN?)");
            Usage("dp832 identify [[-a <address>]]");

            Cmd("status", "Display full status for all channels and system settings");
            Usage("dp832 status [[-a <address>]]");

            Cmd("channel-status", "Display detailed settings and measurements for a single channel");
            Param("-c|--channel <n>", "Channel number: 1, 2, or 3  [default: 1]");
            Usage("dp832 channel-status [[-c <n>]] [[-a <address>]]");

            Cmd("set-voltage", "Set the output voltage  (CH1/CH2: 0\u201330 V, CH3: 0\u20135 V)");
            Param("-c|--channel <n>", "Channel number: 1, 2, or 3  [default: 1]");
            Param("-v|--voltage <v>", "Target voltage in volts");
            Usage("dp832 set-voltage -v <v> [[-c <n>]] [[-a <address>]]");

            Cmd("set-current", "Set the current limit  (all channels: 0\u20133 A)");
            Param("-c|--channel <n>", "Channel number: 1, 2, or 3  [default: 1]");
            Param("-i|--current <a>", "Current limit in amps");
            Usage("dp832 set-current -i <a> [[-c <n>]] [[-a <address>]]");

            Cmd("set-ovp", "Configure Over Voltage Protection  (CH1/CH2: 0.01\u201331 V, CH3: 0.01\u20136 V)");
            Param("-c|--channel <n>", "Channel number: 1, 2, or 3  [default: 1]");
            Param("-l|--level <v>", "OVP level in volts (optional; omit to leave unchanged)");
            Param("-s|--state <on|off>", "Enable or disable OVP (optional; omit to leave unchanged)");
            Usage("dp832 set-ovp [[-c <n>]] [[-l <v>]] [[-s <on|off>]] [[-a <address>]]");

            Cmd("set-ocp", "Configure Over Current Protection  (all channels: 0.001\u20134 A)");
            Param("-c|--channel <n>", "Channel number: 1, 2, or 3  [default: 1]");
            Param("-l|--level <a>", "OCP level in amps (optional; omit to leave unchanged)");
            Param("-s|--state <on|off>", "Enable or disable OCP (optional; omit to leave unchanged)");
            Usage("dp832 set-ocp [[-c <n>]] [[-l <a>]] [[-s <on|off>]] [[-a <address>]]");

            Cmd("clear-trip", "Clear latched OVP/OCP protection trips so the output can be re-enabled");
            Param("-c|--channel <n>", "Channel number: 1, 2, or 3  [default: 1]");
            Usage("dp832 clear-trip [[-c <n>]] [[-a <address>]]");

            Cmd("output", "Enable or disable the output for a channel");
            Param("-c|--channel <n>", "Channel number: 1, 2, or 3  [default: 1]");
            Param("-s|--state <on|off>", "Desired output state: on or off");
            Usage("dp832 output -s <on|off> [[-c <n>]] [[-a <address>]]");

            Cmd("set-otp", "Enable or disable Over Temperature Protection (OTP)");
            Param("<state>", "on or off");
            Usage("dp832 set-otp <on|off> [[-a <address>]]");

            Cmd("set-beeper", "Enable or disable the instrument beeper");
            Param("<state>", "on or off");
            Usage("dp832 set-beeper <on|off> [[-a <address>]]");

            Cmd("set-brightness", "Set the display brightness");
            Param("<brightness>", "Brightness percentage: 1\u2013100");
            Usage("dp832 set-brightness <1-100> [[-a <address>]]");

            Cmd("set-screensaver", "Enable or disable the display screen saver (activates after 25 minutes)");
            Param("<state>", "on or off");
            Usage("dp832 set-screensaver <on|off> [[-a <address>]]");

            Cmd("set-tracking-mode", "Set the CH1/CH2 tracking mode");
            Param("<mode>", "SYNC (synchronised) or INDE (independent)");
            Usage("dp832 set-tracking-mode <SYNC|INDE> [[-a <address>]]");

            Cmd("set-track", "Enable or disable per-channel output tracking (CH1/CH2 only)");
            Param("-c|--channel <n>", "Channel number: 1 or 2  [default: 1]");
            Param("-s|--state <on|off>", "Desired tracking state: on or off");
            Usage("dp832 set-track -s <on|off> [[-c <n>]] [[-a <address>]]");

            Cmd("reset", "Reset the device to factory defaults (*RST)");
            Usage("dp832 reset [[-a <address>]]");

            Cmd("help", "Show this full command reference");
            Usage("dp832 help");

            AnsiConsole.WriteLine();
            return 0;
        }

        private static void Cmd(string name, string description)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("  [bold cyan]" + name + "[/]");
            AnsiConsole.MarkupLine(ParamIndent + Markup.Escape(description));
        }

        private static void Param(string param, string description)
        {
            string padded = (ParamIndent + param).PadRight(ParamColWidth);
            AnsiConsole.MarkupLine("[yellow]" + padded + "[/]" + Markup.Escape(description));
        }

        private static void Usage(string usage)
        {
            AnsiConsole.MarkupLine(ParamIndent + "[grey]Usage: " + usage + "[/]");
        }
    }
}
