using Spectre.Console;
using NationalInstruments.Visa;

namespace DP832PowerSupply;

class Program
{
    private static string deviceAddress = "GPIB0::1::INSTR"; // Default GPIB address
    private static MessageBasedSession? visaSession;

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

    static void ShowTitle()
    {
        var rule = new Rule("[bold blue]DP832 Power Supply Controller[/]");
        rule.Style = Style.Parse("blue");
        AnsiConsole.Write(rule);
        AnsiConsole.WriteLine();
    }

    static void ShowDescription()
    {
        var panel = new Panel(
            "[bold]Welcome to the DP832 Power Supply Control Application[/]\n\n" +
            "This application allows you to control the Rigol DP832 programmable DC power supply\n" +
            "using SCPI commands over GPIB or TCPIP interfaces via NI-VISA.\n\n" +
            "[yellow]Features:[/]\n" +
            "  • Configure GPIB/TCPIP device address\n" +
            "  • Connect and communicate with the power supply\n" +
            "  • Send SCPI commands for device control\n" +
            "  • Monitor device status and readings\n\n" +
            "[dim]Powered by NI-VISA and Spectre.Console[/]"
        )
        {
            Header = new PanelHeader("[ About ]", Justify.Center),
            Border = BoxBorder.Rounded,
            BorderStyle = new Style(Color.Cyan1)
        };
        
        AnsiConsole.Write(panel);
        AnsiConsole.WriteLine();
    }

    static string ShowMainMenu()
    {
        var connectionStatus = visaSession != null ? "[green]Connected[/]" : "[red]Disconnected[/]";
        
        AnsiConsole.MarkupLine($"[bold]Current Device Address:[/] [yellow]{deviceAddress}[/]");
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
            AnsiConsole.MarkupLine($"[green]✓[/] Device address updated to: [yellow]{deviceAddress}[/]");
            
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
            .Start($"Connecting to [yellow]{deviceAddress}[/]...", ctx =>
            {
                try
                {
                    ctx.Spinner(Spinner.Known.Dots);
                    ctx.SpinnerStyle(Style.Parse("green"));
                    
                    using (var rmSession = new ResourceManager())
                    {
                        visaSession = (MessageBasedSession)rmSession.Open(deviceAddress);
                        visaSession.TimeoutMilliseconds = 5000;
                        
                        // Try to query device identification
                        ctx.Status("Querying device identification...");
                        visaSession.FormattedIO.WriteLine("*IDN?");
                        string idn = visaSession.FormattedIO.ReadLine();
                        
                        AnsiConsole.WriteLine();
                        AnsiConsole.MarkupLine("[green]✓ Successfully connected![/]");
                        AnsiConsole.MarkupLine($"[bold]Device:[/] [cyan]{idn}[/]");
                    }
                }
                catch (Exception ex)
                {
                    visaSession = null;
                    AnsiConsole.WriteLine();
                    AnsiConsole.MarkupLine($"[red]✗ Connection failed:[/] {ex.Message}");
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
            AnsiConsole.MarkupLine("[green]✓[/] Disconnected from device.");
        }
        catch (Exception ex)
        {
            AnsiConsole.MarkupLine($"[red]✗ Error during disconnect:[/] {ex.Message}");
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
        
        table.AddRow("Device Address", $"[yellow]{deviceAddress}[/]");
        table.AddRow("Connection Status", visaSession != null ? "[green]Connected[/]" : "[red]Disconnected[/]");
        
        if (visaSession != null)
        {
            try
            {
                visaSession.FormattedIO.WriteLine("*IDN?");
                string idn = visaSession.FormattedIO.ReadLine();
                table.AddRow("Device ID", $"[cyan]{idn}[/]");
            }
            catch (Exception ex)
            {
                table.AddRow("Device ID", $"[red]Error: {ex.Message}[/]");
            }
        }
        
        AnsiConsole.Write(table);
    }
}
