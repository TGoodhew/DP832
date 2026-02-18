using Spectre.Console;
using NationalInstruments.Visa;
using System;

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
                case "Show Current Settings":
                    ShowCurrentSettings();
                    break;
                case "Exit":
                    exit = true;
                    DisconnectFromDevice();
                    break;
            }
            
            if (!exit)
            {
                AnsiConsole.WriteLine();
                AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
                Console.ReadKey(true);
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
                    "Show Current Settings",
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
                var ipAddress = AnsiConsole.Prompt(
                    new TextPrompt<string>("Enter [green]IP address[/] (e.g., 192.168.1.100):")
                        .DefaultValue("192.168.1.100"));
                newAddress = $"TCPIP::{ipAddress}::INSTR";
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
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine($"[red]✗ Connection failed:[/] {Markup.Escape(ex.Message)}");
                    AnsiConsole.MarkupLine("[yellow]Note:[/] Make sure the device is powered on and the address is correct.");
                    AnsiConsole.MarkupLine("[yellow]Note:[/] NI-VISA runtime must be installed on your system.");
                }
            });
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
    }

    static bool ParseProtectionState(string stateStr)
    {
        string trimmedState = stateStr.Trim();
        return (trimmedState == "ON" || trimmedState == "1");
    }

    static void ChannelControlsMenu()
    {
        if (visaSession == null)
        {
            AnsiConsole.MarkupLine("[red]✗[/] Not connected to device. Please connect first.");
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
                AnsiConsole.MarkupLine("[grey]Press any key to continue...[/]");
                Console.ReadKey(true);
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
            double currentVolt = double.Parse(currentVoltStr);
            
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
            double newVolt = double.Parse(newVoltStr);
            
            AnsiConsole.MarkupLine($"[green]✓[/] Voltage set to: [yellow]{newVolt:F3}V[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error setting voltage:[/] {Markup.Escape(ex.Message)}");
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
            double currentAmp = double.Parse(currentAmpStr);
            
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
            double newCurrent = double.Parse(newCurrentStr);
            
            AnsiConsole.MarkupLine($"[green]✓[/] Current limit set to: [yellow]{newCurrent:F3}A[/]");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error setting current:[/] {Markup.Escape(ex.Message)}");
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
            double currentOvp = double.Parse(currentOvpStr);
            
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
            double currentOcp = double.Parse(currentOcpStr);
            
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
            double voltSetting = double.Parse(voltSettingStr);

            visaSession.FormattedIO.WriteLine($":MEAS:VOLT? CH{channelNum}");
            string voltMeasStr = visaSession.FormattedIO.ReadLine();
            double voltMeas = double.Parse(voltMeasStr);

            table.AddRow("Voltage", $"[yellow]{voltSetting:F3}V[/]", $"[cyan]{voltMeas:F3}V[/]");

            // Query current settings and measurements
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:CURR?");
            string currSettingStr = visaSession.FormattedIO.ReadLine();
            double currSetting = double.Parse(currSettingStr);

            visaSession.FormattedIO.WriteLine($":MEAS:CURR? CH{channelNum}");
            string currMeasStr = visaSession.FormattedIO.ReadLine();
            double currMeas = double.Parse(currMeasStr);

            table.AddRow("Current", $"[yellow]{currSetting:F3}A[/]", $"[cyan]{currMeas:F3}A[/]");

            // Query power measurement
            visaSession.FormattedIO.WriteLine($":MEAS:POWEr? CH{channelNum}");
            string powerStr = visaSession.FormattedIO.ReadLine();
            double power = double.Parse(powerStr);

            table.AddRow("Power", "-", $"[cyan]{power:F3}W[/]");

            // Add separator
            table.AddEmptyRow();

            // Query OVP settings
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:VOLT:PROT?");
            string ovpLevelStr = visaSession.FormattedIO.ReadLine();
            double ovpLevel = double.Parse(ovpLevelStr);

            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:VOLT:PROT:STAT?");
            string ovpStateStr = visaSession.FormattedIO.ReadLine();
            bool ovpEnabled = ParseProtectionState(ovpStateStr);

            table.AddRow("OVP Level", $"[yellow]{ovpLevel:F3}V[/]", "-");
            table.AddRow("OVP State", ovpEnabled ? "[green]Enabled[/]" : "[red]Disabled[/]", "-");

            // Query OCP settings
            visaSession.FormattedIO.WriteLine($":SOUR{channelNum}:CURR:PROT?");
            string ocpLevelStr = visaSession.FormattedIO.ReadLine();
            double ocpLevel = double.Parse(ocpLevelStr);

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
        }
    }
}
}
