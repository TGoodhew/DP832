# DP832 Power Supply Controller

A C# console application for controlling the Rigol DP832 Programmable DC Power Supply using NI-VISA and SCPI commands.

## About the DP832

The Rigol DP832 is a triple-output programmable DC power supply featuring:
- Three independent output channels (2×30V/3A + 5V/3A)
- High precision and low noise
- SCPI command compatibility
- Multiple interface options: USB, LAN, GPIB (optional)
- Comprehensive measurement and control capabilities

## Features

This application currently provides:
- ✨ **Beautiful Console Interface** - Powered by Spectre.Console with interactive menus
- 🔌 **Flexible Connectivity** - Support for GPIB and TCPIP (LAN) connections
- ⚙️ **Easy Configuration** - Simple device address setup with guided prompts
- 📡 **Device Identification** - Query device information via *IDN? SCPI command
- 🎯 **User-Friendly** - Interactive menu-driven interface
- ⚡ **Channel Control** - Full voltage and current control for all three channels
- 🛡️ **Protection Controls** - Configure OVP and OCP for safe operation

### Current Capabilities
- Configure and change VISA resource address (GPIB/TCPIP/Custom)
- Connect to and disconnect from the power supply
- Query device identification (*IDN?)
- Display connection status and settings
- **Channel voltage and current control** for all three channels
- **OVP (Over Voltage Protection)** configuration and control
- **OCP (Over Current Protection)** configuration and control
- Real-time monitoring of voltage, current, and power readings per channel
- View detailed channel status with protection settings

### Planned Features
The following capabilities are planned for future releases:
- General SCPI command sending interface
- Output enable/disable controls
- Logging and data capture

## Prerequisites

### Required Software
- [.NET Framework 4.7.2 Developer Pack](https://dotnet.microsoft.com/download/dotnet-framework/net472) or later
- [Visual Studio 2017](https://visualstudio.microsoft.com/) or later (or MSBuild/dotnet CLI)
- [NI-VISA Runtime](https://www.ni.com/en-us/support/downloads/drivers/download.ni-visa.html) - Required for GPIB/TCPIP communication

### Hardware
- Rigol DP832 Power Supply
- Appropriate connection interface:
  - GPIB interface card (for GPIB connection)
  - Network connection (for TCPIP/LAN connection)
  - USB connection (using VISA USB support)

## Installation

1. Clone the repository:
   ```bash
   git clone https://github.com/TGoodhew/DP832.git
   cd DP832
   ```

2. Open the solution in Visual Studio:
   ```bash
   # Open in Visual Studio
   DP832.sln
   ```

3. Build the solution:
   - In Visual Studio: Press Ctrl+Shift+B
   - Or using command line:
   ```bash
   dotnet build DP832.sln
   ```

## Usage

### Running the Application

From Visual Studio:
- Press F5 to run with debugging
- Or Ctrl+F5 to run without debugging

From command line:
```bash
DP832PowerSupply\bin\Debug\net472\DP832PowerSupply.exe
```

Or using dotnet CLI:
```bash
dotnet run --project DP832PowerSupply
```

### Configuring Device Address

The application supports multiple connection types:

#### GPIB Connection
Default format: `GPIB0::1::INSTR`
- GPIB0: Primary GPIB interface
- 1: Device address (configurable on the power supply)
- INSTR: VISA resource class

Example addresses:
- `GPIB0::1::INSTR` - Device at GPIB address 1
- `GPIB0::5::INSTR` - Device at GPIB address 5

#### TCPIP/LAN Connection
Format: `TCPIP::192.168.1.100::INSTR`
- Replace `192.168.1.100` with your device's IP address

Example addresses:
- `TCPIP::192.168.1.100::INSTR` - Device at IP 192.168.1.100
- `TCPIP::10.0.0.50::INSTR` - Device at IP 10.0.0.50

#### USB Connection
Format: `USB0::0x1AB1::0x0E11::DP8XXXXXXXXX::INSTR`
- USB devices are typically auto-discovered by NI-VISA

### Main Menu Options

1. **Configure Device Address** - Set or change the GPIB/TCPIP address
2. **Connect to Device** - Establish connection to the power supply
3. **Disconnect from Device** - Close the connection
4. **Channel Controls** - Access voltage, current, OVP, and OCP controls for each channel
5. **Show Current Settings** - Display current configuration and device info
6. **Exit** - Close the application

### Channel Controls

The Channel Controls menu provides access to all three channels (CH1, CH2, CH3) with the following capabilities for each:
- **Set Voltage** - Configure output voltage (CH1/CH2: 0-30V, CH3: 0-5V)
- **Set Current** - Configure current limit (all channels: 0-3A)
- **Configure OVP** - Set Over Voltage Protection level and enable/disable protection
- **Configure OCP** - Set Over Current Protection level and enable/disable protection
- **View Channel Status** - Display real-time voltage, current, power readings and protection settings

## SCPI Commands Reference

The DP832 supports standard SCPI commands for control and measurement:

### Common Commands
- `*IDN?` - Query device identification
- `*RST` - Reset to default state
- `*CLS` - Clear status registers

### Output Control
- `:OUTPut:STATe <CH>,<ON|OFF>` - Enable/disable channel output
- `:OUTPut:STATe? <CH>` - Query channel output state

### Voltage Control
- `:SOURce<CH>:VOLTage <value>` - Set voltage
- `:SOURce<CH>:VOLTage?` - Query voltage setting
- `:MEASure:VOLTage? <CH>` - Measure actual voltage

### Current Control
- `:SOURce<CH>:CURRent <value>` - Set current limit
- `:SOURce<CH>:CURRent?` - Query current setting
- `:MEASure:CURRent? <CH>` - Measure actual current

### Protection Control
- `:SOURce<CH>:VOLTage:PROTection <value>` - Set OVP level
- `:SOURce<CH>:VOLTage:PROTection?` - Query OVP level
- `:SOURce<CH>:VOLTage:PROTection:STATe <ON|OFF>` - Enable/disable OVP
- `:SOURce<CH>:VOLTage:PROTection:STATe?` - Query OVP state
- `:SOURce<CH>:CURRent:PROTection <value>` - Set OCP level
- `:SOURce<CH>:CURRent:PROTection?` - Query OCP level
- `:SOURce<CH>:CURRent:PROTection:STATe <ON|OFF>` - Enable/disable OCP
- `:SOURce<CH>:CURRent:PROTection:STATe?` - Query OCP state

### Power Measurement
- `:MEASure:POWEr? <CH>` - Measure output power

Where `<CH>` can be CH1, CH2, or CH3 for the three channels.

## Project Structure

```
DP832/
├── DP832.sln                      # Visual Studio solution file
├── DP832PowerSupply/              # Console application project
│   ├── Program.cs                 # Main application code
│   ├── DP832PowerSupply.csproj   # Project file (.NET Framework 4.7.2, C# 7.3)
│   └── README.md                  # Project-specific documentation
├── README.md                      # This file
├── LICENSE                        # License information
└── DP832.pdf                      # Device manual
```

## Dependencies

- **Spectre.Console** (v0.49.1) - Rich console UI framework (compatible with .NET Framework 4.7.2)
- **NI-VISA .NET Framework Libraries** - NI-VISA runtime assemblies for instrument communication
  - NationalInstruments.Common.dll
  - NationalInstruments.VisaNS.dll
  - Installed via NI-VISA Runtime (not available as NuGet package for .NET Framework 4.7.2)

## Technical Details

- **Target Framework:** .NET Framework 4.7.2
- **C# Version:** 7.3
- **Build System:** Visual Studio solution with MSBuild
- **UI Framework:** Spectre.Console for rich terminal interfaces
- **Instrument Control:** NI-VISA for GPIB/TCPIP/USB communication

## Troubleshooting

### Connection Issues

**Error: "Connection failed"**
- Verify NI-VISA Runtime is installed
- Check that the device is powered on
- Verify the device address is correct
- For GPIB: Ensure the GPIB interface card is properly installed
- For TCPIP: Ping the device IP address to verify network connectivity
- Check that the device's GPIB address or IP is correctly configured

**Error: "VISA not found"**
- Install the NI-VISA Runtime from National Instruments website
- Restart your computer after installation

### Device-Specific Issues

**Cannot find device**
- Use NI MAX (Measurement & Automation Explorer) to scan for devices
- Verify the device appears in NI MAX before using this application

## Development

### Building from Source

```bash
# Clone the repository
git clone https://github.com/TGoodhew/DP832.git
cd DP832

# Build
dotnet build

# Run
dotnet run
```

### Code Structure

The application is structured as a single-file console application with the following main components:
- **ShowTitle()** - Displays the application title banner
- **ShowDescription()** - Shows the application description and features
- **ShowMainMenu()** - Interactive menu system
- **ConfigureDeviceAddress()** - Device address configuration
- **ConnectToDevice()** - VISA connection handling
- **DisconnectFromDevice()** - Connection cleanup
- **ShowCurrentSettings()** - Display current configuration

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.

## License

This project is licensed under the terms specified in the LICENSE file.

## Resources

- [Rigol DP800 Series Manual](https://www.rigolna.com/products/dc-power-supplies/dp800/)
- [NI-VISA Documentation](https://www.ni.com/en-us/support/documentation/supplemental/06/ni-visa-overview.html)
- [SCPI Standard](https://www.ivifoundation.org/scpi/)
- [Spectre.Console Documentation](https://spectreconsole.net/)

## Author

Created for controlling Rigol DP832 power supplies via SCPI commands.

## Version History

- **v1.0.0** - Initial release with basic connectivity and device configuration
