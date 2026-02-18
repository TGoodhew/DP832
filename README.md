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

This application provides:
- ✨ **Beautiful Console Interface** - Powered by Spectre.Console
- 🔌 **Flexible Connectivity** - Support for GPIB and TCPIP (LAN) connections
- ⚙️ **Easy Configuration** - Simple device address setup
- 📡 **SCPI Communication** - Direct communication with the power supply via NI-VISA
- 🎯 **User-Friendly** - Interactive menu-driven interface

## Prerequisites

### Required Software
- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
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

2. Restore dependencies:
   ```bash
   dotnet restore
   ```

3. Build the application:
   ```bash
   dotnet build
   ```

## Usage

### Running the Application

```bash
dotnet run
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
4. **Show Current Settings** - Display current configuration and device info
5. **Exit** - Close the application

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

### Power Measurement
- `:MEASure:POWEr? <CH>` - Measure output power

Where `<CH>` can be CH1, CH2, or CH3 for the three channels.

## Project Structure

```
DP832/
├── Program.cs                 # Main application code
├── DP832PowerSupply.csproj   # Project file
├── README.md                  # This file
└── LICENSE                    # License information
```

## Dependencies

- **Spectre.Console** (v0.54.0) - Rich console UI framework
- **NationalInstruments.Visa** (v25.5.0.13) - NI-VISA .NET library for instrument communication

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
