using Spectre.Console;
using NationalInstruments.Visa;
using System;
using System.Globalization;

namespace DP832PowerSupply
{
    class Program
    {
        private static string deviceAddress = "GPIB0::1::INSTR"; // Default GPIB address
        private static ResourceManager resourceManager;
        private static MessageBasedSession visaSession;

    static void Main(string[] args)
    {
        ShowTitle();
        ShowDescription();
        
        bool exit = false;
        while (!exit)
        {
            var choice = ShowMainMenu();
            
            switch (choice)
            {
                case "Configure Device Address":
                    ConfigureDeviceAddress();
                    break;
                case "Connect to Device":
                    ConnectToDevice();
                    break;
                case "Disconnect from Device":
                    DisconnectFromDevice();
                    break;
                case "Channel Controls":
                    ChannelControlsMenu();
                    break;
                case "Advanced Options":
                    AdvancedOptionsMenu();
                    break;
                case "Show Current Settings":
                    ShowCurrentSettings();
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
                    Console.ReadKey(true);
                    break;
                case "Reset Device":
                    ResetDevice();
                    break;
                case "Exit":
                    exit = true;
                    DisconnectFromDevice();
                    break;
            }
            
            if (!exit)
            {
                Console.Clear();
                ShowTitle();
            }
        }
        
        AnsiConsole.MarkupLine("[green]Thank you for using DP832 Power Supply Controller![/]");
    }

    /// <summary>
    /// Displays the DP832 title with figlet ASCII art and rule-based subtitle.
    /// This method can be called independently throughout the program to show the title.
    /// </summary>
    static void ShowTitle()
    {
        // Display figlet ASCII art title
        var figlet = new FigletText("DP832")
            .Centered()
            .Color(Color.Blue);
        
        AnsiConsole.Write(figlet);
        AnsiConsole.WriteLine();
        
        // Display rule-based subtitle
        var rule = new Rule("[bold blue]DP832 Power Supply Controller[/]");
        rule.Style = Style.Parse("blue");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }

    static void ShowDescription()
    {
        var panel = new Panel(
            "[bold]Welcome to the DP832 Power Supply Control Application[/]\n\n" +
            "This application allows you to connect to the Rigol DP832 programmable DC power supply\n" +
            "using SCPI commands over GPIB or TCPIP interfaces via NI-VISA.\n\n" +
            "[yellow]Current Features:[/]\n" +
            " - Configure GPIB/TCPIP device address\n" +
            " - Connect and disconnect from the power supply\n" +
            " - Query device identification (*IDN?)\n" +
            " - View connection status and settings\n" +
            " - Control voltage and current for each channel\n" +
            " - Configure OVP (Over Voltage Protection) and OCP (Over Current Protection)\n\n" +
            "[dim]Powered by NI-VISA and Spectre.Console[/]"
        )
        {
            Header = new PanelHeader("About", Justify.Center),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Cyan1)
        };
        
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    static string ShowMainMenu()
    {
        var connectionStatus = visaSession != null ? "[green]Connected[/]" : "[red]Disconnected[/]";
        
        AnsiConsole.MarkupLine($"[bold]Current Device Address:[/] [yellow]{Markup.Escape(deviceAddress)}[/]");
        AnsiConsole.MarkupLine($"[bold]Connection Status:[/] {connectionStatus}");
        AnsiConsole.WriteLine();
        
        var selection = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("[bold cyan]What would you like to do?[/]")
                .PageSize(10)
                .MoreChoicesText("[grey](Move up and down to reveal more options)[/]")
                .AddChoices(new[] {
                    "Configure Device Address",
                    "Connect to Device",
                    "Disconnect from Device",
                    "Channel Controls",
                    "Advanced Options",
                    "Show Current Settings",
                    "Reset Device",
                    "Exit"
                }));
        
        return selection;
    }

    static void ConfigureDeviceAddress()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Configure Device Address[/]");
        AnsiConsole.MarkupLine("[grey]Examples:[/]");
        AnsiConsole.MarkupLine("[grey]  GPIB: GPIB0::1::INSTR[/]");
        AnsiConsole.MarkupLine("[grey]  TCPIP: TCPIP::192.168.1.100::INSTR[/]");
        AnsiConsole.WriteLine();
        
        var addressType = AnsiConsole.Prompt(
            new SelectionPrompt<string>()
                .Title("Select [green]connection type[/]:")
                .AddChoices(new[] { "GPIB", "TCPIP", "Custom" }));
        
        string newAddress = "";
        
        switch (addressType)
        {
            case "GPIB":
                var gpibNumber = AnsiConsole.Prompt(
                    new TextPrompt<int>("Enter [green]GPIB device number[/] (e.g., 1):")
                        .DefaultValue(1)
                        .ValidationErrorMessage("[red]Please enter a valid number[/]"));
                newAddress = $"GPIB0::{gpibNumber}::INSTR";
                break;
                
            case "TCPIP":
                string ipPrefix = "192.168.1";
                string lastOctetDefault = "100";
                if (deviceAddress.StartsWith("TCPIP::"))
                {
                    string currentIp = deviceAddress.Substring("TCPIP::".Length);
                    int colonIdx = currentIp.IndexOf("::");
                    if (colonIdx >= 0)
                        currentIp = currentIp.Substring(0, colonIdx);
                    string[] octets = currentIp.Split('.');
                    if (octets.Length == 4 &&
                        int.TryParse(octets[0], out int o0) && o0 >= 0 && o0 <= 255 &&
                        int.TryParse(octets[1], out int o1) && o1 >= 0 && o1 <= 255 &&
                        int.TryParse(octets[2], out int o2) && o2 >= 0 && o2 <= 255 &&
                        int.TryParse(octets[3], out int o3) && o3 >= 0 && o3 <= 255)
                    {
                        ipPrefix = $"{o0}.{o1}.{o2}";
                        lastOctetDefault = octets[3];
                    }
                }
                var ipInput = AnsiConsole.Prompt(
                    new TextPrompt<string>($"Enter [green]IP address[/] or last octet ([yellow]{Markup.Escape(ipPrefix)}.[/]):")
                        .DefaultValue(lastOctetDefault)
                        .Validate(input =>
                        {
                            if (int.TryParse(input, out int octet) && octet >= 1 && octet <= 254)
                                return ValidationResult.Success();
                            string[] parts = input.Split('.');
                            if (parts.Length == 4)
                            {
                                bool valid = true;
                                foreach (string part in parts)
                                {
                                    if (!int.TryParse(part, out int b) || b < 0 || b > 255)
                                    {
                                        valid = false;
                                        break;
                                    }
                                }
                                if (valid)
                                    return ValidationResult.Success();
                            }
                            return ValidationResult.Error("[red]Enter a valid last octet (1-254) or full IP address[/]");
                        }));
                if (int.TryParse(ipInput, out int finalLastOctet))
                    newAddress = $"TCPIP::{ipPrefix}.{finalLastOctet}::INSTR";
                else
                    newAddress = $"TCPIP::{ipInput}::INSTR";
                break;
                
            case "Custom":
                newAddress = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter [green]custom VISA resource string[/]:")
                        .DefaultValue(deviceAddress));
                break;
        }
        
        if (!string.IsNullOrWhiteSpace(newAddress))
        {
            deviceAddress = newAddress;
            AnsiConsole.MarkupLine($"[green]✓[/] Device address updated to: [yellow]{Markup.Escape(deviceAddress)}[/]");
            
            // If already connected, suggest reconnection
            if (visaSession != null)
            {
                AnsiConsole.MarkupLine("[yellow]⚠[/] [italic]You are currently connected. Please disconnect and reconnect to use the new address.[/]");
            }
        }
    }

    static void ConnectToDevice()
    {
        if (visaSession != null)
        {
            AnsiConsole.MarkupLine("[yellow]⚠[/] Already connected to device.");
            return;
        }
        
        AnsiConsole.WriteLine();
        Exception connectionError = null;
        AnsiConsole.Status()
            .Start($"Connecting to [yellow]{Markup.Escape(deviceAddress)}[/]...", ctx =>
            {
                try
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(Style.Parse("green"));
                    
                    resourceManager = new ResourceManager();
                    visaSession = (MessageBasedSession)resourceManager.Open(deviceAddress);
                    visaSession.TimeoutMilliseconds = 5000;
                    
                    // Try to query device identification
                    ctx.Status("Querying device identification...");
                    visaSession.FormattedIO.WriteLine("*IDN?");
                    string idn = visaSession.FormattedIO.ReadLine();
                    
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine("[green]✓ Successfully connected![/]");
                    AnsiConsole.MarkupLine($"[bold]Device:[/] [cyan]{Markup.Escape(idn)}[/]");
                }
                catch (Exception ex)
                {
                    // Dispose any resources that were allocated
                    if (visaSession != null)
                    {
                        visaSession.Dispose();
                        visaSession = null;
                    }
                    resourceManager?.Dispose();
                    resourceManager = null;
                    connectionError = ex;
                }
            });

        if (connectionError != null)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[red]✗ Connection failed:[/] {Markup.Escape(connectionError.Message)}");
            AnsiConsole.MarkupLine("[yellow]Note:[/] Make sure the device is powered on and the address is correct.");
            AnsiConsole.MarkupLine("[yellow]Note:[/] NI-VISA runtime must be installed on your system.");
            PauseOnError();
        }
    }

    static void DisconnectFromDevice()
    {
        if (visaSession == null)
        {
            AnsiConsole.MarkupLine("[grey]Not connected to any device.[/]");
            return;
        }
        
        try
        {
            visaSession.Dispose();
            visaSession = null;
            resourceManager?.Dispose();
            resourceManager = null;
            AnsiConsole.MarkupLine("[green]✓[/] Disconnected from device.");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error during disconnect:[/] {Markup.Escape(ex.Message)}");
            PauseOnError();
        }
    }

    static void ShowCurrentSettings()
    {
        AnsiConsole.WriteLine();
        
        var table = new Table();
        table.Border(TableBorder.Rounded);
        table.BorderStyle(new Style(Color.Cyan1));
        table.AddColumn(new TableColumn("[bold]Setting[/]").Centered());
        table.AddColumn(new TableColumn("[bold]Value[/]").Centered());
        
        table.AddRow("Device Address", $"[yellow]{Markup.Escape(deviceAddress)}[/]");
        table.AddRow("Connection Status", visaSession != null ? "[green]Connected[/]" : "[red]Disconnected[/]");
        
        if (visaSession != null)
        {
            try
            {
                visaSession.FormattedIO.WriteLine("*IDN?");
                string idn = visaSession.FormattedIO.ReadLine();
                table.AddRow("Device ID", $"[cyan]{Markup.Escape(idn)}[/]");
            }
            catch (Exception ex)
            {
                table.AddRow("Device ID", $"[red]Error: {Markup.Escape(ex.Message)}[/]");
            }
        }
        
        AnsiConsole.Write(table);
        
        // Display channel status for all 3 channels if connected
        if (visaSession != null)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine("[bold cyan]Channel Status:[/]");
            
            // Create a single horizontal table with all channels
            var channelTable = new Table();
            channelTable.Border(TableBorder.Rounded);
            channelTable.BorderStyle(new Style(Color.Cyan1));
            channelTable.AddColumn(new TableColumn("[bold]Parameter[/]").LeftAligned());
            channelTable.AddColumn(new TableColumn("[bold cyan]CH1[/]").Centered());
            channelTable.AddColumn(new TableColumn("[bold cyan]CH2[/]").Centered());
            channelTable.AddColumn(new TableColumn("[bold cyan]CH3[/]").Centered());
            
            try
            {
                // Arrays to store data for all channels
                double[] voltSettings = new double[3];
                double[] voltMeasured = new double[3];
                double[] currSettings = new double[3];
                double[] currMeasured = new double[3];
                double[] power = new double[3];
                double[] ovpLevels = new double[3];
                bool[] ovpEnabled = new bool[3];
                double[] ocpLevels = new double[3];
                bool[] ocpEnabled = new bool[3];
                bool[] outputEnabled = new bool[3];
                bool[] channelErrors = new bool[3];
                
                // Query all channels
                for (int i = 0; i < 3; i++)
                {
                    int channelNum = i + 1;
                    try
                    {
                        // Query voltage settings and measurements
                        visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:VOLT?");
                        string voltSettingStr = visaSession.FormattedIO.ReadLine();
                        voltSettings[i] = double.Parse(voltSettingStr, CultureInfo.InvariantCulture);
                        
                        visaSession.FormattedIO.WriteLine($":MEAS:VOLT? CH{channelNum}");
                        string voltMeasStr = visaSession.FormattedIO.ReadLine();
                        voltMeasured[i] = double.Parse(voltMeasStr, CultureInfo.InvariantCulture);
                        
                        // Query current settings and measurements
                        visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:CURR?");
                        string currSettingStr = visaSession.FormattedIO.ReadLine();
                        currSettings[i] = double.Parse(currSettingStr, CultureInfo.InvariantCulture);
                        
                        visaSession.FormattedIO.WriteLine($":MEAS:CURR? CH{channelNum}");
                        string currMeasStr = visaSession.FormattedIO.ReadLine();
                        currMeasured[i] = double.Parse(currMeasStr, CultureInfo.InvariantCulture);
                        
                        // Query power measurement
                        visaSession.FormattedIO.WriteLine($":MEAS:POWEr? CH{channelNum}");
                        string powerStr = visaSession.FormattedIO.ReadLine();
                        power[i] = double.Parse(powerStr, CultureInfo.InvariantCulture);
                        
                        // Query OVP settings
                        visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:VOLT:PROT?");
                        string ovpLevelStr = visaSession.FormattedIO.ReadLine();
                        ovpLevels[i] = double.Parse(ovpLevelStr, CultureInfo.InvariantCulture);
                        
                        visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:VOLT:PROT:STAT?");
                        string ovpStateStr = visaSession.FormattedIO.ReadLine();
                        ovpEnabled[i] = ParseProtectionState(ovpStateStr);
                        
                        // Query OCP settings
                        visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:CURR:PROT?");
                        string ocpLevelStr = visaSession.FormattedIO.ReadLine();
                        ocpLevels[i] = double.Parse(ocpLevelStr, CultureInfo.InvariantCulture);
                        
                        visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:CURR:PROT:STAT?");
                        string ocpStateStr = visaSession.FormattedIO.ReadLine();
                        ocpEnabled[i] = ParseProtectionState(ocpStateStr);
                        
                        // Query output state
                        visaSession.FormattedIO.WriteLine($":OUTPut? CH{channelNum}");
                        string outputStateStr = visaSession.FormattedIO.ReadLine();
                        outputEnabled[i] = ParseProtectionState(outputStateStr);
                    }
                    catch
                    {
                        channelErrors[i] = true;
                    }
                }
                
                // Build the table rows
                channelTable.AddRow(
                    "Voltage (Set)",
                    channelErrors[0] ? "[red]Error[/]" : $"[yellow]{voltSettings[0]:F3}V[/]",
                    channelErrors[1] ? "[red]Error[/]" : $"[yellow]{voltSettings[1]:F3}V[/]",
                    channelErrors[2] ? "[red]Error[/]" : $"[yellow]{voltSettings[2]:F3}V[/]"
                );
                
                channelTable.AddRow(
                    "Voltage (Meas)",
                    channelErrors[0] ? "[red]Error[/]" : $"[cyan]{voltMeasured[0]:F3}V[/]",
                    channelErrors[1] ? "[red]Error[/]" : $"[cyan]{voltMeasured[1]:F3}V[/]",
                    channelErrors[2] ? "[red]Error[/]" : $"[cyan]{voltMeasured[2]:F3}V[/]"
                );
                
                channelTable.AddRow(
                    "Current (Set)",
                    channelErrors[0] ? "[red]Error[/]" : $"[yellow]{currSettings[0]:F3}A[/]",
                    channelErrors[1] ? "[red]Error[/]" : $"[yellow]{currSettings[1]:F3}A[/]",
                    channelErrors[2] ? "[red]Error[/]" : $"[yellow]{currSettings[2]:F3}A[/]"
                );
                
                channelTable.AddRow(
                    "Current (Meas)",
                    channelErrors[0] ? "[red]Error[/]" : $"[cyan]{currMeasured[0]:F3}A[/]",
                    channelErrors[1] ? "[red]Error[/]" : $"[cyan]{currMeasured[1]:F3}A[/]",
                    channelErrors[2] ? "[red]Error[/]" : $"[cyan]{currMeasured[2]:F3}A[/]"
                );
                
                channelTable.AddRow(
                    "Power (Meas)",
                    channelErrors[0] ? "[red]Error[/]" : $"[cyan]{power[0]:F3}W[/]",
                    channelErrors[1] ? "[red]Error[/]" : $"[cyan]{power[1]:F3}W[/]",
                    channelErrors[2] ? "[red]Error[/]" : $"[cyan]{power[2]:F3}W[/]"
                );
                
                channelTable.AddEmptyRow();
                
                channelTable.AddRow(
                    "OVP Level",
                    channelErrors[0] ? "[red]Error[/]" : $"[yellow]{ovpLevels[0]:F3}V[/]",
                    channelErrors[1] ? "[red]Error[/]" : $"[yellow]{ovpLevels[1]:F3}V[/]",
                    channelErrors[2] ? "[red]Error[/]" : $"[yellow]{ovpLevels[2]:F3}V[/]"
                );
                
                channelTable.AddRow(
                    "OVP State",
                    channelErrors[0] ? "[red]Error[/]" : (ovpEnabled[0] ? "[green]Enabled[/]" : "[red]Disabled[/]"),
                    channelErrors[1] ? "[red]Error[/]" : (ovpEnabled[1] ? "[green]Enabled[/]" : "[red]Disabled[/]"),
                    channelErrors[2] ? "[red]Error[/]" : (ovpEnabled[2] ? "[green]Enabled[/]" : "[red]Disabled[/]")
                );
                
                channelTable.AddRow(
                    "OCP Level",
                    channelErrors[0] ? "[red]Error[/]" : $"[yellow]{ocpLevels[0]:F3}A[/]",
                    channelErrors[1] ? "[red]Error[/]" : $"[yellow]{ocpLevels[1]:F3}A[/]",
                    channelErrors[2] ? "[red]Error[/]" : $"[yellow]{ocpLevels[2]:F3}A[/]"
                );
                
                channelTable.AddRow(
                    "OCP State",
                    channelErrors[0] ? "[red]Error[/]" : (ocpEnabled[0] ? "[green]Enabled[/]" : "[red]Disabled[/]"),
                    channelErrors[1] ? "[red]Error[/]" : (ocpEnabled[1] ? "[green]Enabled[/]" : "[red]Disabled[/]"),
                    channelErrors[2] ? "[red]Error[/]" : (ocpEnabled[2] ? "[green]Enabled[/]" : "[red]Disabled[/]")
                );
                
                channelTable.AddEmptyRow();
                
                channelTable.AddRow(
                    "Output State",
                    channelErrors[0] ? "[red]Error[/]" : (outputEnabled[0] ? "[green]On[/]" : "[grey]Off[/]"),
                    channelErrors[1] ? "[red]Error[/]" : (outputEnabled[1] ? "[green]On[/]" : "[grey]Off[/]"),
                    channelErrors[2] ? "[red]Error[/]" : (outputEnabled[2] ? "[green]On[/]" : "[grey]Off[/]")
                );
                
                AnsiConsole.Write(channelTable);
                
                // Display system/advanced status
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[bold cyan]System Status:[/]");
                
                var sysTable = new Table();
                sysTable.Border(TableBorder.Rounded);
                sysTable.BorderStyle(new Style(Color.Cyan1));
                sysTable.AddColumn(new TableColumn("[bold]Setting[/]").LeftAligned());
                sysTable.AddColumn(new TableColumn("[bold]Value[/]").Centered());
                
                try
                {
                    visaSession.FormattedIO.WriteLine(":SYSTem:TRACKMode?");
                    string trackStr = visaSession.FormattedIO.ReadLine().Trim();
                    sysTable.AddRow("Track Mode", $"[yellow]{Markup.Escape(trackStr)}[/]");
                }
                catch
                {
                    sysTable.AddRow("Track Mode", "[red]Error[/]");
                }
                
                try
                {
                    visaSession.FormattedIO.WriteLine(":OUTPut:TRACk? CH1");
                    string trackCh1Str = visaSession.FormattedIO.ReadLine();
                    bool trackCh1On = ParseProtectionState(trackCh1Str);
                    visaSession.FormattedIO.WriteLine(":OUTPut:TRACk? CH2");
                    string trackCh2Str = visaSession.FormattedIO.ReadLine();
                    bool trackCh2On = ParseProtectionState(trackCh2Str);
                    sysTable.AddRow("Track (CH1/CH2)",
                        $"{(trackCh1On ? "[green]CH1 On[/]" : "[grey]CH1 Off[/]")} / {(trackCh2On ? "[green]CH2 On[/]" : "[grey]CH2 Off[/]")}");
                }
                catch
                {
                    sysTable.AddRow("Track (CH1/CH2)", "[red]Error[/]");
                }
                
                try
                {
                    visaSession.FormattedIO.WriteLine(":SYSTem:OTP?");
                    string otpStr = visaSession.FormattedIO.ReadLine();
                    bool otpOn = ParseProtectionState(otpStr);
                    sysTable.AddRow("OTP (Over Temp Protection)", otpOn ? "[green]Enabled[/]" : "[red]Disabled[/]");
                }
                catch
                {
                    sysTable.AddRow("OTP (Over Temp Protection)", "[red]Error[/]");
                }
                
                try
                {
                    visaSession.FormattedIO.WriteLine(":SYSTem:BEEPer?");
                    string beeperStr = visaSession.FormattedIO.ReadLine();
                    bool beeperOn = ParseProtectionState(beeperStr);
                    sysTable.AddRow("Beeper", beeperOn ? "[green]Enabled[/]" : "[grey]Disabled[/]");
                }
                catch
                {
                    sysTable.AddRow("Beeper", "[red]Error[/]");
                }
                
                try
                {
                    visaSession.FormattedIO.WriteLine(":SYSTem:BRIGhtness?");
                    string brightnessStr = visaSession.FormattedIO.ReadLine();
                    sysTable.AddRow("Display Brightness", $"[yellow]{Markup.Escape(brightnessStr.Trim())}%[/]");
                }
                catch
                {
                    sysTable.AddRow("Display Brightness", "[red]Error[/]");
                }
                
                try
                {
                    visaSession.FormattedIO.WriteLine(":SYSTem:SAVer?");
                    string ssaverStr = visaSession.FormattedIO.ReadLine();
                    bool ssaverOn = ParseProtectionState(ssaverStr);
                    sysTable.AddRow("Screen Saver", ssaverOn ? "[green]Enabled[/]" : "[grey]Disabled[/]");
                }
                catch
                {
                    sysTable.AddRow("Screen Saver", "[red]Error[/]");
                }
                
                AnsiConsole.Write(sysTable);
            }
            catch (Exception ex)
            {
                AnsiConsole.MarkupLine($"[red]✗ Error reading channel status:[/] {Markup.Escape(ex.Message)}");
            }
        }
    }

    static bool ParseProtectionState(string stateStr)
    {
        string trimmedState = stateStr.Trim();
        return trimmedState.Equals("ON", StringComparison.OrdinalIgnoreCase) || trimmedState == "1";
    }

    /// <summary>
    /// Checks the Standard Event Status Register (*ESR?) for SCPI errors.
    /// If error bits are set, reads and displays each error from :SYSTem:ERRor?.
    /// Returns true if no errors were detected.
    /// </summary>
    static bool CheckScpiErrors()
    {
        try
        {
            visaSession.FormattedIO.WriteLine("*ESR?");
            string esrStr = visaSession.FormattedIO.ReadLine();
            int esr = int.Parse(esrStr.Trim(), CultureInfo.InvariantCulture);
            
            // Bits 2 (QYE=4), 3 (DDE=8), 4 (EXE=16), 5 (CME=32) indicate errors
            const int QYE = 4;   // Query Error
            const int DDE = 8;   // Device Dependent Error
            const int EXE = 16;  // Execution Error
            const int CME = 32;  // Command Error
            const int errorMask = QYE | DDE | EXE | CME;
            if ((esr & errorMask) == 0)
                return true;
            
            bool hasErrors = false;
            const int maxErrorReads = 10;
            int maxErrors = maxErrorReads;
            while (maxErrors-- > 0)
            {
                visaSession.FormattedIO.WriteLine(":SYSTem:ERRor?");
                string errorStr = visaSession.FormattedIO.ReadLine().Trim();
                if (errorStr.StartsWith("0,", StringComparison.Ordinal))
                    break;
                AnsiConsole.MarkupLine($"[red]✗ Device error:[/] {Markup.Escape(errorStr)}");
                hasErrors = true;
            }
            return !hasErrors;
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error checking device status:[/] {Markup.Escape(ex.Message)}");
            return false;
        }
    }

    /// <summary>
    /// Clears the Standard Event Register, sends a SCPI command, then checks
    /// *ESR? and :SYSTem:ERRor? to surface any device errors to the user.
    /// Returns true if the command succeeded without errors.
    /// </summary>
    static bool SendCommandAndCheckErrors(string command)
    {
        try
        {
            visaSession.FormattedIO.WriteLine("*CLS");
            visaSession.FormattedIO.WriteLine(command);
            return CheckScpiErrors();
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error sending command:[/] {Markup.Escape(ex.Message)}");
            return false;
        }
    }

    static void PauseOnError()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
        Console.ReadKey(true);
    }

    static void ChannelControlsMenu()
    {
        if (visaSession == null)
        {
            AnsiConsole.MarkupLine("[red]✗[/] Not connected to device. Please connect first.");
            PauseOnError();
            return;
        }

        bool exitChannelMenu = false;
        while (!exitChannelMenu)
        {
            AnsiConsole.WriteLine();
            var channelChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold cyan]Select a Channel to Control:[/]")
                    .PageSize(10)
                    .AddChoices(new[] {
                        "Channel 1 (CH1) - 30V/3A",
                        "Channel 2 (CH2) - 30V/3A",
                        "Channel 3 (CH3) - 5V/3A",
                        "Back to Main Menu"
                    }));

            if (channelChoice == "Back to Main Menu")
            {
                exitChannelMenu = true;
            }
            else
            {
                // Extract channel number more robustly
                int channelNum;
                if (channelChoice.Contains("Channel 1"))
                    channelNum = 1;
                else if (channelChoice.Contains("Channel 2"))
                    channelNum = 2;
                else if (channelChoice.Contains("Channel 3"))
                    channelNum = 3;
                else
                    continue; // Skip invalid selections
                
                ChannelControlSubMenu(channelNum);
            }
        }
    }

    static void ChannelControlSubMenu(int channelNum)
    {
        string channelName = $"CH{channelNum}";
        
        // Get max voltage based on channel
        double maxVoltage = (channelNum == 3) ? 5.0 : 30.0;
        double maxCurrent = 3.0;

        bool exitSubMenu = false;
        while (!exitSubMenu)
        {
            AnsiConsole.WriteLine();
            AnsiConsole.MarkupLine($"[bold cyan]Controls for {channelName}[/]");
            AnsiConsole.WriteLine();
            
            var controlChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title($"What would you like to control for [yellow]{channelName}[/]?")
                    .PageSize(10)
                    .AddChoices(new[] {
                        "Set Voltage",
                        "Set Current",
                        "Configure OVP (Over Voltage Protection)",
                        "Configure OCP (Over Current Protection)",
                        "View Channel Status",
                        "Back to Channel Selection"
                    }));

            switch (controlChoice)
            {
                case "Set Voltage":
                    SetChannelVoltage(channelNum, maxVoltage);
                    break;
                case "Set Current":
                    SetChannelCurrent(channelNum, maxCurrent);
                    break;
                case "Configure OVP (Over Voltage Protection)":
                    ConfigureOVP(channelNum, maxVoltage);
                    break;
                case "Configure OCP (Over Current Protection)":
                    ConfigureOCP(channelNum, maxCurrent);
                    break;
                case "View Channel Status":
                    ViewChannelStatus(channelNum);
                    break;
                case "Back to Channel Selection":
                    exitSubMenu = true;
                    break;
            }

            if (!exitSubMenu)
            {
                AnsiConsole.WriteLine();
            }
        }
    }

    static void SetChannelVoltage(int channelNum, double maxVoltage)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold cyan]Set Voltage for CH{channelNum}[/]");
        AnsiConsole.MarkupLine($"[grey]Maximum voltage for CH{channelNum}: {maxVoltage}V[/]");
        AnsiConsole.WriteLine();

        try
        {
            // Query current voltage setting
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:VOLT?");
            string currentVoltStr = visaSession.FormattedIO.ReadLine();
            double currentVolt = double.Parse(currentVoltStr, CultureInfo.InvariantCulture);
            
            AnsiConsole.MarkupLine($"[yellow]Current voltage setting:[/] {currentVolt:F3}V");
            AnsiConsole.WriteLine();

            var voltage = AnsiConsole.Prompt(
                new TextPrompt<double>($"Enter [green]voltage[/] (0 to {maxVoltage}V):")
                    .DefaultValue(currentVolt)
                    .ValidationErrorMessage($"[red]Please enter a valid voltage between 0 and {maxVoltage}V[/]")
                    .Validate(v =>
                    {
                        if (v < 0 || v > maxVoltage)
                            return ValidationResult.Error($"[red]Voltage must be between 0 and {maxVoltage}V[/]");
                        return ValidationResult.Success();
                    }));

            // Set the voltage
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:VOLT {voltage:F3}");
            
            // Verify by reading back
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:VOLT?");
            string newVoltStr = visaSession.FormattedIO.ReadLine();
            double newVolt = double.Parse(newVoltStr, CultureInfo.InvariantCulture);
            
            AnsiConsole.MarkupLine($"[green]✓[/] Voltage set to: [yellow]{newVolt:F3}V[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error setting voltage:[/] {Markup.Escape(ex.Message)}");
            PauseOnError();
        }
    }

    static void SetChannelCurrent(int channelNum, double maxCurrent)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold cyan]Set Current for CH{channelNum}[/]");
        AnsiConsole.MarkupLine($"[grey]Maximum current for CH{channelNum}: {maxCurrent}A[/]");
        AnsiConsole.WriteLine();

        try
        {
            // Query current setting
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:CURR?");
            string currentAmpStr = visaSession.FormattedIO.ReadLine();
            double currentAmp = double.Parse(currentAmpStr, CultureInfo.InvariantCulture);
            
            AnsiConsole.MarkupLine($"[yellow]Current setting:[/] {currentAmp:F3}A");
            AnsiConsole.WriteLine();

            var current = AnsiConsole.Prompt(
                new TextPrompt<double>($"Enter [green]current limit[/] (0 to {maxCurrent}A):")
                    .DefaultValue(currentAmp)
                    .ValidationErrorMessage($"[red]Please enter a valid current between 0 and {maxCurrent}A[/]")
                    .Validate(c =>
                    {
                        if (c < 0 || c > maxCurrent)
                            return ValidationResult.Error($"[red]Current must be between 0 and {maxCurrent}A[/]");
                        return ValidationResult.Success();
                    }));

            // Set the current
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:CURR {current:F3}");
            
            // Verify by reading back
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:CURR?");
            string newCurrentStr = visaSession.FormattedIO.ReadLine();
            double newCurrent = double.Parse(newCurrentStr, CultureInfo.InvariantCulture);
            
            AnsiConsole.MarkupLine($"[green]✓[/] Current limit set to: [yellow]{newCurrent:F3}A[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error setting current:[/] {Markup.Escape(ex.Message)}");
            PauseOnError();
        }
    }

    static void ConfigureOVP(int channelNum, double maxVoltage)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold cyan]Configure OVP (Over Voltage Protection) for CH{channelNum}[/]");
        AnsiConsole.WriteLine();

        try
        {
            // Query current OVP value
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:VOLT:PROT?");
            string currentOvpStr = visaSession.FormattedIO.ReadLine();
            double currentOvp = double.Parse(currentOvpStr, CultureInfo.InvariantCulture);
            
            // Query current OVP state
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:VOLT:PROT:STAT?");
            string stateStr = visaSession.FormattedIO.ReadLine();
            bool ovpEnabled = ParseProtectionState(stateStr);
            
            AnsiConsole.MarkupLine($"[yellow]Current OVP level:[/] {currentOvp:F3}V");
            AnsiConsole.MarkupLine($"[yellow]Current OVP state:[/] {(ovpEnabled ? "[green]Enabled[/]" : "[red]Disabled[/]")}");
            AnsiConsole.WriteLine();

            // Ask if user wants to change OVP level
            if (AnsiConsole.Confirm($"Change OVP level? (Current: {currentOvp:F3}V)", false))
            {
                // OVP can be set slightly above max voltage to provide protection margin
                var ovpLevel = AnsiConsole.Prompt(
                    new TextPrompt<double>($"Enter [green]OVP level[/] (0.01 to {maxVoltage + 1}V):")
                        .DefaultValue(currentOvp)
                        .ValidationErrorMessage($"[red]Please enter a valid OVP level[/]")
                        .Validate(v =>
                        {
                            if (v < 0.01 || v > maxVoltage + 1)
                                return ValidationResult.Error($"[red]OVP must be between 0.01 and {maxVoltage + 1}V[/]");
                            return ValidationResult.Success();
                        }));

                visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:VOLT:PROT {ovpLevel:F3}");
                AnsiConsole.MarkupLine($"[green]✓[/] OVP level set to: [yellow]{ovpLevel:F3}V[/]");
            }

            AnsiConsole.WriteLine();
            
            // Ask if user wants to enable/disable OVP
            var enableOvp = AnsiConsole.Confirm("Enable OVP?", ovpEnabled);
            
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:VOLT:PROT:STAT {(enableOvp ? "ON" : "OFF")}");
            AnsiConsole.MarkupLine($"[green]✓[/] OVP {(enableOvp ? "[green]enabled[/]" : "[red]disabled[/]")}");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error configuring OVP:[/] {Markup.Escape(ex.Message)}");
            PauseOnError();
        }
    }

    static void ConfigureOCP(int channelNum, double maxCurrent)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold cyan]Configure OCP (Over Current Protection) for CH{channelNum}[/]");
        AnsiConsole.WriteLine();

        try
        {
            // Query current OCP value
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:CURR:PROT?");
            string currentOcpStr = visaSession.FormattedIO.ReadLine();
            double currentOcp = double.Parse(currentOcpStr, CultureInfo.InvariantCulture);
            
            // Query current OCP state
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:CURR:PROT:STAT?");
            string ocpStateStr = visaSession.FormattedIO.ReadLine();
            bool ocpEnabled = ParseProtectionState(ocpStateStr);
            
            AnsiConsole.MarkupLine($"[yellow]Current OCP level:[/] {currentOcp:F3}A");
            AnsiConsole.MarkupLine($"[yellow]Current OCP state:[/] {(ocpEnabled ? "[green]Enabled[/]" : "[red]Disabled[/]")}");
            AnsiConsole.WriteLine();

            // Ask if user wants to change OCP level
            if (AnsiConsole.Confirm($"Change OCP level? (Current: {currentOcp:F3}A)", false))
            {
                // OCP can be set slightly above max current to provide protection margin
                var ocpLevel = AnsiConsole.Prompt(
                    new TextPrompt<double>($"Enter [green]OCP level[/] (0.001 to {maxCurrent + 1}A):")
                        .DefaultValue(currentOcp)
                        .ValidationErrorMessage($"[red]Please enter a valid OCP level[/]")
                        .Validate(c =>
                        {
                            if (c < 0.001 || c > maxCurrent + 1)
                                return ValidationResult.Error($"[red]OCP must be between 0.001 and {maxCurrent + 1}A[/]");
                            return ValidationResult.Success();
                        }));

                visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:CURR:PROT {ocpLevel:F3}");
                AnsiConsole.MarkupLine($"[green]✓[/] OCP level set to: [yellow]{ocpLevel:F3}A[/]");
            }

            AnsiConsole.WriteLine();
            
            // Ask if user wants to enable/disable OCP
            var enableOcp = AnsiConsole.Confirm("Enable OCP?", ocpEnabled);
            
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:CURR:PROT:STAT {(enableOcp ? "ON" : "OFF")}");
            AnsiConsole.MarkupLine($"[green]✓[/] OCP {(enableOcp ? "[green]enabled[/]" : "[red]disabled[/]")}");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error configuring OCP:[/] {Markup.Escape(ex.Message)}");
            PauseOnError();
        }
    }

    static void ViewChannelStatus(int channelNum)
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine($"[bold cyan]Channel Status for CH{channelNum}[/]");
        AnsiConsole.WriteLine();

        try
        {
            var table = new Table();
            table.Border(TableBorder.Rounded);
            table.BorderStyle(new Style(Color.Cyan1));
            table.AddColumn(new TableColumn("[bold]Parameter[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Setting[/]").Centered());
            table.AddColumn(new TableColumn("[bold]Measured[/]").Centered());

            // Query voltage settings and measurements
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:VOLT?");
            string voltSettingStr = visaSession.FormattedIO.ReadLine();
            double voltSetting = double.Parse(voltSettingStr, CultureInfo.InvariantCulture);

            visaSession.FormattedIO.WriteLine($":MEAS:VOLT? CH{channelNum}");
            string voltMeasStr = visaSession.FormattedIO.ReadLine();
            double voltMeas = double.Parse(voltMeasStr, CultureInfo.InvariantCulture);

            table.AddRow("Voltage", $"[yellow]{voltSetting:F3}V[/]", $"[cyan]{voltMeas:F3}V[/]");

            // Query current settings and measurements
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:CURR?");
            string currSettingStr = visaSession.FormattedIO.ReadLine();
            double currSetting = double.Parse(currSettingStr, CultureInfo.InvariantCulture);

            visaSession.FormattedIO.WriteLine($":MEAS:CURR? CH{channelNum}");
            string currMeasStr = visaSession.FormattedIO.ReadLine();
            double currMeas = double.Parse(currMeasStr, CultureInfo.InvariantCulture);

            table.AddRow("Current", $"[yellow]{currSetting:F3}A[/]", $"[cyan]{currMeas:F3}A[/]");

            // Query power measurement
            visaSession.FormattedIO.WriteLine($":MEAS:POWEr? CH{channelNum}");
            string powerStr = visaSession.FormattedIO.ReadLine();
            double power = double.Parse(powerStr, CultureInfo.InvariantCulture);

            table.AddRow("Power", "-", $"[cyan]{power:F3}W[/]");

            // Add separator
            table.AddEmptyRow();

            // Query OVP settings
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:VOLT:PROT?");
            string ovpLevelStr = visaSession.FormattedIO.ReadLine();
            double ovpLevel = double.Parse(ovpLevelStr, CultureInfo.InvariantCulture);

            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:VOLT:PROT:STAT?");
            string ovpStateStr = visaSession.FormattedIO.ReadLine();
            bool ovpEnabled = ParseProtectionState(ovpStateStr);

            table.AddRow("OVP Level", $"[yellow]{ovpLevel:F3}V[/]", "-");
            table.AddRow("OVP State", ovpEnabled ? "[green]Enabled[/]" : "[red]Disabled[/]", "-");

            // Query OCP settings
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:CURR:PROT?");
            string ocpLevelStr = visaSession.FormattedIO.ReadLine();
            double ocpLevel = double.Parse(ocpLevelStr, CultureInfo.InvariantCulture);

            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:CURR:PROT:STAT?");
            string ocpStateStr = visaSession.FormattedIO.ReadLine();
            bool ocpEnabled = ParseProtectionState(ocpStateStr);

            table.AddRow("OCP Level", $"[yellow]{ocpLevel:F3}A[/]", "-");
            table.AddRow("OCP State", ocpEnabled ? "[green]Enabled[/]" : "[red]Disabled[/]", "-");

            AnsiConsole.Write(table);
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error reading channel status:[/] {Markup.Escape(ex.Message)}");
            PauseOnError();
        }
    }

    static void AdvancedOptionsMenu()
    {
        if (visaSession == null)
        {
            AnsiConsole.MarkupLine("[red]✗[/] Not connected to device. Please connect first.");
            PauseOnError();
            return;
        }

        bool exitMenu = false;
        while (!exitMenu)
        {
            AnsiConsole.WriteLine();
            var choice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold cyan]Advanced Options[/]")
                    .PageSize(10)
                    .AddChoices(new[] {
                        "Configure Output State",
                        "Configure Tracking",
                        "Configure OTP (Over Temperature Protection)",
                        "Configure Beeper",
                        "Configure Display Settings",
                        "Back to Main Menu"
                    }));

            switch (choice)
            {
                case "Configure Output State":
                    ConfigureOutputState();
                    break;
                case "Configure Tracking":
                    ConfigureTracking();
                    break;
                case "Configure OTP (Over Temperature Protection)":
                    ConfigureOTP();
                    break;
                case "Configure Beeper":
                    ConfigureBeeper();
                    break;
                case "Configure Display Settings":
                    ConfigureDisplaySettings();
                    break;
                case "Back to Main Menu":
                    exitMenu = true;
                    break;
            }

            if (!exitMenu)
            {
                AnsiConsole.WriteLine();
            }
        }
    }

    static void ConfigureOutputState()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Configure Output State[/]");
        AnsiConsole.WriteLine();

        try
        {
            // Display current output states
            var stateTable = new Table();
            stateTable.Border(TableBorder.Rounded);
            stateTable.BorderStyle(new Style(Color.Cyan1));
            stateTable.AddColumn(new TableColumn("[bold]Channel[/]").Centered());
            stateTable.AddColumn(new TableColumn("[bold]Output State[/]").Centered());

            bool[] currentStates = new bool[3];
            for (int i = 0; i < 3; i++)
            {
                int channelNum = i + 1;
                try
                {
                    visaSession.FormattedIO.WriteLine($":OUTPut? CH{channelNum}");
                    string stateStr = visaSession.FormattedIO.ReadLine();
                    currentStates[i] = ParseProtectionState(stateStr);
                    stateTable.AddRow($"CH{channelNum}", currentStates[i] ? "[green]On[/]" : "[grey]Off[/]");
                }
                catch
                {
                    stateTable.AddRow($"CH{channelNum}", "[red]Error[/]");
                }
            }

            AnsiConsole.Write(stateTable);
            AnsiConsole.WriteLine();

            // Select channel to configure
            var channelChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("Select [green]channel[/] to configure:")
                    .AddChoices(new[] { "CH1", "CH2", "CH3", "All Channels On", "All Channels Off", "Cancel" }));

            if (channelChoice == "Cancel")
                return;

            if (channelChoice == "All Channels On" || channelChoice == "All Channels Off")
            {
                string state = channelChoice == "All Channels On" ? "ON" : "OFF";
                bool allOk = true;
                for (int i = 1; i <= 3; i++)
                {
                    if (!SendCommandAndCheckErrors($":OUTPut CH{i},{state}"))
                        allOk = false;
                }
                if (allOk)
                    AnsiConsole.MarkupLine($"[green]✓[/] All channels turned [yellow]{state}[/]");
                return;
            }

            int ch = int.Parse(channelChoice.Replace("CH", ""), CultureInfo.InvariantCulture);
            if (ch < 1 || ch > 3)
            {
                AnsiConsole.MarkupLine("[red]✗ Invalid channel selection.[/]");
                return;
            }
            bool enable = AnsiConsole.Confirm($"Enable output for {channelChoice}?", currentStates[ch - 1]);
            if (SendCommandAndCheckErrors($":OUTPut CH{ch},{(enable ? "ON" : "OFF")}"))
                AnsiConsole.MarkupLine($"[green]✓[/] {channelChoice} output {(enable ? "[green]enabled[/]" : "[grey]disabled[/]")}");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error configuring output state:[/] {Markup.Escape(ex.Message)}");
            PauseOnError();
        }
    }

    static void ConfigureTracking()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Configure Tracking[/]");
        AnsiConsole.MarkupLine("[grey]Tracking links CH1 and CH2 to mirror voltage settings.[/]");
        AnsiConsole.WriteLine();

        try
        {
            // Show current track mode and per-channel track state
            visaSession.FormattedIO.WriteLine(":SYSTem:TRACKMode?");
            string currentMode = visaSession.FormattedIO.ReadLine().Trim();
            
            visaSession.FormattedIO.WriteLine(":OUTPut:TRACk? CH1");
            string trackCh1Str = visaSession.FormattedIO.ReadLine();
            bool trackCh1On = ParseProtectionState(trackCh1Str);
            
            visaSession.FormattedIO.WriteLine(":OUTPut:TRACk? CH2");
            string trackCh2Str = visaSession.FormattedIO.ReadLine();
            bool trackCh2On = ParseProtectionState(trackCh2Str);

            AnsiConsole.MarkupLine($"[yellow]Track mode:[/] {Markup.Escape(currentMode)}  " +
                $"[yellow]CH1:[/] {(trackCh1On ? "[green]On[/]" : "[grey]Off[/]")}  " +
                $"[yellow]CH2:[/] {(trackCh2On ? "[green]On[/]" : "[grey]Off[/]")}");
            AnsiConsole.WriteLine();

            var trackChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("What would you like to configure?")
                    .AddChoices(new[] {
                        "Set Track Mode (SYNC/INDE)",
                        "Enable/Disable CH1 Track",
                        "Enable/Disable CH2 Track",
                        "Cancel"
                    }));

            if (trackChoice == "Cancel")
                return;

            if (trackChoice == "Set Track Mode (SYNC/INDE)")
            {
                var modeChoice = AnsiConsole.Prompt(
                    new SelectionPrompt<string>()
                        .Title("Select [green]track mode[/]:")
                        .AddChoices(new[] { "SYNC (Synchronous)", "INDE (Independent)" }));

                string modeCmd = modeChoice.StartsWith("SYNC") ? "SYNC" : "INDE";
                if (SendCommandAndCheckErrors($":SYSTem:TRACKMode {modeCmd}"))
                    AnsiConsole.MarkupLine($"[green]✓[/] Track mode set to: [yellow]{modeCmd}[/]");
            }
            else if (trackChoice == "Enable/Disable CH1 Track")
            {
                bool enable = AnsiConsole.Confirm("Enable track for CH1?", trackCh1On);
                if (SendCommandAndCheckErrors($":OUTPut:TRACk CH1,{(enable ? "ON" : "OFF")}"))
                    AnsiConsole.MarkupLine($"[green]✓[/] CH1 track {(enable ? "[green]enabled[/]" : "[grey]disabled[/]")}");
            }
            else if (trackChoice == "Enable/Disable CH2 Track")
            {
                bool enable = AnsiConsole.Confirm("Enable track for CH2?", trackCh2On);
                if (SendCommandAndCheckErrors($":OUTPut:TRACk CH2,{(enable ? "ON" : "OFF")}"))
                    AnsiConsole.MarkupLine($"[green]✓[/] CH2 track {(enable ? "[green]enabled[/]" : "[grey]disabled[/]")}");
            }
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error configuring tracking:[/] {Markup.Escape(ex.Message)}");
            PauseOnError();
        }
    }

    static void ConfigureOTP()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Configure OTP (Over Temperature Protection)[/]");
        AnsiConsole.WriteLine();

        try
        {
            visaSession.FormattedIO.WriteLine(":SYSTem:OTP?");
            string otpStateStr = visaSession.FormattedIO.ReadLine();
            bool otpEnabled = ParseProtectionState(otpStateStr);

            AnsiConsole.MarkupLine($"[yellow]Current OTP state:[/] {(otpEnabled ? "[green]Enabled[/]" : "[red]Disabled[/]")}");
            AnsiConsole.WriteLine();

            bool enable = AnsiConsole.Confirm("Enable OTP?", otpEnabled);
            if (SendCommandAndCheckErrors($":SYSTem:OTP {(enable ? "ON" : "OFF")}"))
                AnsiConsole.MarkupLine($"[green]✓[/] OTP {(enable ? "[green]enabled[/]" : "[red]disabled[/]")}");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error configuring OTP:[/] {Markup.Escape(ex.Message)}");
            PauseOnError();
        }
    }

    static void ConfigureBeeper()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Configure Beeper[/]");
        AnsiConsole.WriteLine();

        try
        {
            visaSession.FormattedIO.WriteLine(":SYSTem:BEEPer?");
            string beeperStateStr = visaSession.FormattedIO.ReadLine();
            bool beeperEnabled = ParseProtectionState(beeperStateStr);

            AnsiConsole.MarkupLine($"[yellow]Current beeper state:[/] {(beeperEnabled ? "[green]Enabled[/]" : "[grey]Disabled[/]")}");
            AnsiConsole.WriteLine();

            bool enable = AnsiConsole.Confirm("Enable beeper?", beeperEnabled);
            if (SendCommandAndCheckErrors($":SYSTem:BEEPer {(enable ? "ON" : "OFF")}"))
                AnsiConsole.MarkupLine($"[green]✓[/] Beeper {(enable ? "[green]enabled[/]" : "[grey]disabled[/]")}");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error configuring beeper:[/] {Markup.Escape(ex.Message)}");
            PauseOnError();
        }
    }

    static void ConfigureDisplaySettings()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Configure Display Settings[/]");
        AnsiConsole.WriteLine();

        bool exitDisplay = false;
        while (!exitDisplay)
        {
            var displayChoice = AnsiConsole.Prompt(
                new SelectionPrompt<string>()
                    .Title("[bold]Display Settings[/]")
                    .AddChoices(new[] { "Set Brightness", "Configure Screen Saver", "Back" }));

            switch (displayChoice)
            {
                case "Set Brightness":
                    ConfigureDisplayBrightness();
                    break;
                case "Configure Screen Saver":
                    ConfigureScreenSaver();
                    break;
                case "Back":
                    exitDisplay = true;
                    break;
            }

            if (!exitDisplay)
            {
                AnsiConsole.WriteLine();
            }
        }
    }

    static void ConfigureDisplayBrightness()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Set Display Brightness[/]");
        AnsiConsole.WriteLine();

        try
        {
            visaSession.FormattedIO.WriteLine(":SYSTem:BRIGhtness?");
            string brightnessStr = visaSession.FormattedIO.ReadLine();
            int currentBrightness = 50; // default fallback
            if (!int.TryParse(brightnessStr.Trim(), NumberStyles.Integer, CultureInfo.InvariantCulture, out currentBrightness))
                currentBrightness = 50;
            currentBrightness = Math.Max(1, Math.Min(100, currentBrightness));

            AnsiConsole.MarkupLine($"[yellow]Current brightness:[/] {currentBrightness}%");
            AnsiConsole.WriteLine();

            var brightness = AnsiConsole.Prompt(
                new TextPrompt<int>("Enter [green]brightness[/] (1-100%):")
                    .DefaultValue(currentBrightness)
                    .ValidationErrorMessage("[red]Please enter a value between 1 and 100[/]")
                    .Validate(v =>
                    {
                        if (v < 1 || v > 100)
                            return ValidationResult.Error("[red]Brightness must be between 1 and 100[/]");
                        return ValidationResult.Success();
                    }));

            if (SendCommandAndCheckErrors($":SYSTem:BRIGhtness {brightness}"))
                AnsiConsole.MarkupLine($"[green]✓[/] Display brightness set to: [yellow]{brightness}%[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error configuring brightness:[/] {Markup.Escape(ex.Message)}");
            PauseOnError();
        }
    }

    static void ConfigureScreenSaver()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Configure Screen Saver[/]");
        AnsiConsole.WriteLine();

        try
        {
            visaSession.FormattedIO.WriteLine(":SYSTem:SAVer?");
            string ssaverStateStr = visaSession.FormattedIO.ReadLine();
            bool ssaverEnabled = ParseProtectionState(ssaverStateStr);

            AnsiConsole.MarkupLine($"[yellow]Current screen saver state:[/] {(ssaverEnabled ? "[green]Enabled[/]" : "[grey]Disabled[/]")}");
            AnsiConsole.MarkupLine("[grey]When enabled, screen saver activates after 25 minutes standby.[/]");
            AnsiConsole.WriteLine();

            bool enable = AnsiConsole.Confirm("Enable screen saver?", ssaverEnabled);
            if (SendCommandAndCheckErrors($":SYSTem:SAVer {(enable ? "ON" : "OFF")}"))
                AnsiConsole.MarkupLine($"[green]✓[/] Screen saver {(enable ? "[green]enabled[/]" : "[grey]disabled[/]")}");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error configuring screen saver:[/] {Markup.Escape(ex.Message)}");
            PauseOnError();
        }
    }

    static void ResetDevice()
    {
        AnsiConsole.WriteLine();
        AnsiConsole.MarkupLine("[bold cyan]Reset Device[/]");
        AnsiConsole.WriteLine();

        if (visaSession == null)
        {
            AnsiConsole.MarkupLine("[red]✗[/] Not connected to device. Please connect first.");
            return;
        }

        AnsiConsole.MarkupLine("[yellow]⚠ WARNING:[/] This will reset the DP832 to its factory default state.");
        AnsiConsole.MarkupLine("[grey]All channel settings, protection levels, and system settings will be restored to defaults.[/]");
        AnsiConsole.WriteLine();

        if (!AnsiConsole.Confirm("Are you sure you want to reset the device?", false))
        {
            AnsiConsole.MarkupLine("[grey]Reset cancelled.[/]");
            return;
        }

        try
        {
            if (SendCommandAndCheckErrors("*RST"))
                AnsiConsole.MarkupLine("[green]✓[/] Device has been reset to factory default state.");
            else
                AnsiConsole.MarkupLine("[red]✗[/] Reset command reported errors. Check device state.");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error resetting device:[/] {Markup.Escape(ex.Message)}");
        }
    }
}
}
