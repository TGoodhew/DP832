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
- 🔌 **Flexible Connectivity** - Support for GPIB, TCPIP (LAN), and USB connections
- ⚙️ **Easy Configuration** - Simple device address setup with guided prompts
- 📡 **Device Identification** - Query device information via *IDN? SCPI command
- 🎯 **User-Friendly** - Interactive menu-driven interface with ESC key navigation
- ⚡ **Channel Control** - Full voltage and current control for all three channels
- 🛡️ **Protection Controls** - Configure OVP, OCP, and OTP for safe operation
- 🔔 **Protection Trip Alerts** - Automatic detection and display of OVP/OCP trip events
- 🔁 **Output State Control** - Enable or disable individual channels or all channels at once
- 🔗 **Channel Tracking** - Configure synchronised (SYNC) or independent (INDE) tracking for CH1/CH2
- 💾 **State Save/Load** - Save and restore full device configuration to local files or device memory
- 🔄 **Device Reset** - Reset device to factory defaults with a single menu action

### Current Capabilities
- Configure and change VISA resource address (GPIB/TCPIP/USB/Custom)
- Connect to and disconnect from the power supply
- Query device identification (*IDN?)
- Display connection status and settings
- **Channel voltage and current control** for all three channels
- **OVP (Over Voltage Protection)** configuration and control
- **OCP (Over Current Protection)** configuration and control
- **OTP (Over Temperature Protection)** enable/disable control
- Real-time monitoring of voltage, current, and power readings per channel
- View detailed channel status with protection settings
- **Protection trip detection** - OVP/OCP trip state displayed in channel status and current settings views
- **Protection trip alerts** - Immediate warning when OVP or OCP triggers after enabling protection
- **Clear protection trips** - Dedicated option to clear latched OVP/OCP trips and restore channel output
- **Output state control** - Enable/disable output for individual channels or all channels simultaneously
- **Channel tracking** - Set track mode (SYNC/INDE) and per-channel tracking (CH1/CH2)
- **Beeper** - Enable/disable the instrument beeper
- **Display settings** - Configure display brightness and screen saver
- **Save/Load State** - Save full configuration to a local file or device memory slot (1–10), and restore it later
- **Reset Device** - Reset to factory default state via *RST, with automatic clearing of OVP/OCP trip latches

### Planned Features
The following capabilities are planned for future releases:
- General SCPI command sending interface
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

1. **Configure Device Address** - Set or change the GPIB/TCPIP/USB address
2. **Connect to Device** - Establish connection to the power supply
3. **Disconnect from Device** - Close the connection
4. **Channel Controls** - Access voltage, current, OVP, and OCP controls for each channel
5. **Advanced Options** - Output state, channel tracking, OTP, beeper, and display settings
6. **Show Current Settings** - Display current configuration and device info
7. **Save/Load State** - Save or restore the full device configuration
8. **Reset Device** - Reset the device to factory default state
9. **Exit** - Close the application

> **Note:** Use the arrow keys to navigate menus. Press **ESC** at any sub-menu to return to the previous menu without making changes.

### Channel Controls

The Channel Controls menu provides access to all three channels (CH1, CH2, CH3) with the following capabilities for each:
- **Set Voltage** - Configure output voltage (CH1/CH2: 0–30 V, CH3: 0–5 V)
- **Set Current** - Configure current limit (all channels: 0–3 A)
- **Configure OVP** - Set Over Voltage Protection level and enable/disable protection (CH1/CH2: 0.01–31 V, CH3: 0.01–6 V)
- **Configure OCP** - Set Over Current Protection level and enable/disable protection (all channels: 0.001–4 A)
- **View Channel Status** - Display real-time voltage, current, power readings, protection settings, and trip status
- **Clear Protection Trip** - Clear latched OVP/OCP trip conditions to restore channel output

> **Warning:** If OVP or OCP trips, the channel output is disabled automatically. Use **Clear Protection Trip** to unlatch the protection and restore the output. Resolve the root cause (over-voltage or over-current condition) before clearing.

### Advanced Options

The Advanced Options menu provides system-level controls:
- **Configure Output State** - Enable or disable the output for individual channels (CH1/CH2/CH3) or all channels simultaneously
- **Configure Tracking** - Set the track mode (`SYNC` for synchronised output or `INDE` for independent output) and toggle per-channel tracking for CH1 and CH2
- **Configure OTP (Over Temperature Protection)** - Enable or disable the hardware over-temperature protection feature
- **Configure Beeper** - Enable or disable the instrument's audible beeper
- **Configure Display Settings** - Set the display brightness (1–100%) and enable/disable the screen saver (activates after 25 minutes of standby)

> **Note:** Advanced Options requires an active connection to the device.

### Save/Load State

The Save/Load State menu lets you persist and restore the complete device configuration:
- **Save State to Local File** - Queries all channel and system settings and writes them to a plain-text `key=value` file at a path you specify (default: `Documents\dp832_state.txt`)
- **Load State from Local File** - Reads a previously saved file and sends all settings back to the device, channel by channel
- **Save State to Device Memory** - Saves the current configuration to one of the device's 10 built-in memory slots using the `*SAV <slot>` command
- **Load State from Device Memory** - Recalls a saved configuration from a device memory slot using the `*RCL <slot>` command

> **Warning:** Loading a state from a file or device memory will overwrite the current device settings. A confirmation prompt is shown before any destructive operation.

> **Note:** Local state files use plain `key=value` text format (one setting per line). Comment lines beginning with `#` are ignored. Numeric values are stored using the invariant (`.`-decimal) locale.

### Reset Device

The **Reset Device** option resets the DP832 to its factory default state using the `*RST` command. After the reset, OVP and OCP trip latches on all channels are automatically cleared (as `*RST` does not clear latched protection conditions).

> **Warning:** Resetting the device will discard all channel settings, protection levels, and system settings. A confirmation prompt is shown before the reset is performed.

## SCPI Commands Reference

The DP832 supports standard SCPI commands for control and measurement:

### Common Commands
- `*IDN?` - Query device identification
- `*RST` - Reset to factory default state
- `*CLS` - Clear status registers
- `*SAV <slot>` - Save current state to device memory (slot 1–10)
- `*RCL <slot>` - Recall saved state from device memory (slot 1–10)

### Output Control
- `:OUTPut <CH>,<ON|OFF>` - Enable/disable channel output
- `:OUTPut? <CH>` - Query channel output state

### Voltage Control
- `:SOURce<n>:VOLTage <value>` - Set voltage
- `:SOURce<n>:VOLTage?` - Query voltage setting
- `:MEASure:VOLTage? <CH>` - Measure actual voltage

### Current Control
- `:SOURce<n>:CURRent <value>` - Set current limit
- `:SOURce<n>:CURRent?` - Query current limit setting
- `:MEASure:CURRent? <CH>` - Measure actual current

### Power Measurement
- `:MEASure:POWEr? <CH>` - Measure output power

### Protection Control
- `:SOURce<n>:VOLTage:PROTection <value>` - Set OVP level
- `:SOURce<n>:VOLTage:PROTection?` - Query OVP level
- `:SOURce<n>:VOLTage:PROTection:STATe <ON|OFF>` - Enable/disable OVP
- `:SOURce<n>:VOLTage:PROTection:STATe?` - Query OVP state
- `:SOURce<n>:VOLTage:PROTection:TRIP?` - Query if OVP has tripped (`YES` = tripped, `NO` = not tripped)
- `:OUTPut:OVP:CLEar <CH>` - Clear a latched OVP trip for the specified channel
- `:SOURce<n>:CURRent:PROTection <value>` - Set OCP level
- `:SOURce<n>:CURRent:PROTection?` - Query OCP level
- `:SOURce<n>:CURRent:PROTection:STATe <ON|OFF>` - Enable/disable OCP
- `:SOURce<n>:CURRent:PROTection:STATe?` - Query OCP state
- `:SOURce<n>:CURRent:PROTection:TRIP?` - Query if OCP has tripped (`YES` = tripped, `NO` = not tripped)
- `:OUTPut:OCP:CLEar <CH>` - Clear a latched OCP trip for the specified channel

### System Controls
- `:SYSTem:OTP <ON|OFF>` - Enable/disable Over Temperature Protection
- `:SYSTem:OTP?` - Query OTP state
- `:SYSTem:BEEPer <ON|OFF>` - Enable/disable the beeper
- `:SYSTem:BEEPer?` - Query beeper state
- `:SYSTem:BRIGhtness <1-100>` - Set display brightness (percentage, 1–100%)
- `:SYSTem:BRIGhtness?` - Query display brightness
- `:SYSTem:SAVer <ON|OFF>` - Enable/disable the screen saver (activates after 25 min standby)
- `:SYSTem:SAVer?` - Query screen saver state
- `:SYSTem:ERRor?` - Read one entry from the SCPI error queue
- `:SYSTem:TRACKMode <SYNC|INDE>` - Set channel track mode
- `:SYSTem:TRACKMode?` - Query channel track mode
- `:OUTPut:TRACk <CH>,<ON|OFF>` - Enable/disable tracking for a channel
- `:OUTPut:TRACk? <CH>` - Query tracking state for a channel

Where `<n>` is the channel number (1, 2, or 3) and `<CH>` is the channel name (CH1, CH2, or CH3).

## Unit Tests

The solution includes a hardware-free unit test suite that runs on any platform without NI-VISA or a physical DP832 device.

### Test Architecture

| Project | Framework | Role |
|---------|-----------|------|
| `DP832.Helpers` | `netstandard2.0` | Production library: pure business logic tested by `DP832.Tests` |
| `DP832.Tests` | `net8.0` | Test project: xUnit tests targeting `DP832.Helpers` |

The `DP832.Helpers` library contains all pure business logic (parsing, validation, address formatting) with no dependencies on NI-VISA or Spectre.Console, making it fully testable on Linux/macOS CI runners.

The `DP832.Core` library defines the `IDP832Device` interface, enabling mock implementations for unit-testing higher-level logic without a physical instrument.

### Prerequisites for Running Tests

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (or later) — **NI-VISA is not required**

### Running the Tests

#### Command Line

```bash
# From the repository root — run all tests
dotnet test DP832.Tests/DP832.Tests.csproj

# With detailed output (shows each test name and pass/fail)
dotnet test DP832.Tests/DP832.Tests.csproj -v normal

# With code coverage report
dotnet test DP832.Tests/DP832.Tests.csproj --collect:"XPlat Code Coverage"
```

#### Visual Studio

1. Open `DP832.sln`
2. Open the **Test Explorer** panel (`Test → Test Explorer`)
3. Click **Run All Tests** (or press Ctrl+R, A)

#### Visual Studio Code

1. Install the [.NET Test Explorer extension](https://marketplace.visualstudio.com/items?itemName=formulahendry.dotnet-test-explorer)
2. Open the repository root folder
3. Tests will appear automatically in the Testing panel

### What Is Tested

The test suite covers the following helpers in `DP832.Helpers/DeviceHelpers.cs`:

| Method | What is verified |
|--------|-----------------|
| `ParseProtectionState` | `ON` / `on` / `On` / `1` / ` ON ` → `true`; `OFF` / `0` / `""` / `TRUE` → `false` |
| `GetChannelMaxVoltage` | CH1=30V, CH2=30V, CH3=5V |
| `GetChannelMaxCurrent` | All channels return 3A |
| `IsValidVoltage` | Boundary and out-of-range values per channel |
| `IsValidCurrent` | Boundary and out-of-range values |
| `IsValidOvpLevel` | Valid range is 0.01 V to maxVoltage+1 V per channel |
| `IsValidOcpLevel` | Valid range is 0.001 A to 4.0 A |
| `FormatGpibAddress` | Produces `GPIB0::<n>::INSTR` |
| `FormatTcpipAddress` | Produces `TCPIP::<ip>::INSTR` |
| `ParseStateFile` | Key=value parsing, skips `#` comments and blank lines, case-insensitive keys, values containing `=` |

### Expected Output

A successful test run looks like:

```
Passed!  - Failed: 0, Passed: 60, Skipped: 0, Total: 60, Duration: ~40 ms
```

### Adding New Tests

1. Add a new test class to `DP832.Tests/` (or add methods to `DeviceHelpersTests.cs`)
2. If the logic to test lives in `Program.cs` and is tightly coupled to VISA/Spectre.Console, first extract it into a public static method in `DP832.Helpers/DeviceHelpers.cs`
3. Reference the new helper from `Program.cs` and add corresponding xUnit `[Fact]` or `[Theory]` tests

## Project Structure

```
DP832/
├── DP832.sln                      # Visual Studio solution file
├── DP832PowerSupply/              # Console application (interactive menu-driven UI)
│   ├── Program.cs                 # Main application code
│   ├── DP832PowerSupply.csproj   # Project file (.NET Framework 4.7.2, C# 7.3)
│   └── README.md                  # Project-specific documentation
├── DP832.CLI/                     # CLI application (argument-driven, Spectre.Console.Cli)
│   ├── Program.cs                 # CommandApp entry point
│   ├── Commands/
│   │   ├── DeviceSettings.cs      # Base settings with --address option
│   │   ├── ChannelSettings.cs     # Base settings adding --channel option
│   │   ├── IdentifyCommand.cs     # dp832 identify
│   │   ├── StatusCommand.cs       # dp832 status
│   │   ├── SetVoltageCommand.cs   # dp832 set-voltage
│   │   ├── SetCurrentCommand.cs   # dp832 set-current
│   │   ├── OutputCommand.cs       # dp832 output
│   │   └── ResetCommand.cs        # dp832 reset
│   └── DP832.CLI.csproj          # Project file (.NET Framework 4.7.2, C# 7.3)
├── DP832.Core/                    # Shared device communication library (net472)
│   ├── IDP832Device.cs            # Device abstraction interface (connect, query, command, errors)
│   ├── DP832Device.cs             # NI-VISA implementation of IDP832Device
│   └── DP832.Core.csproj         # Project file — referenced by all front-ends
├── DP832.WPF/                     # WPF graphical application (Windows only, net472)
│   ├── App.xaml / App.xaml.cs    # WPF application entry point
│   ├── MainWindow.xaml            # Main window XAML layout
│   ├── MainWindow.xaml.cs         # Main window code-behind
│   └── DP832.WPF.csproj          # Project file
├── DP832.Helpers/                 # Hardware-free helper library (netstandard2.0)
│   ├── DeviceHelpers.cs           # Pure business logic (parsing, validation, formatting)
│   └── DP832.Helpers.csproj      # Project file
├── DP832.Tests/                   # Unit test project (net8.0, xUnit)
│   ├── DeviceHelpersTests.cs      # 60 unit tests for DeviceHelpers
│   └── DP832.Tests.csproj        # Project file
├── README.md                      # This file
├── LICENSE                        # License information
├── DP832.pdf                      # DP800 Series User's Guide (device manual)
└── DP800Programming.pdf           # DP800 Series Programmable DC Power Supply Programming Guide
```

## Dependencies

- **Spectre.Console** (v0.54.0) - Rich console UI library; used by `DP832PowerSupply` and `DP832.CLI` for rendering tables, markup, and figlet text
- **Spectre.Console.Cli** (v0.53.1) - Separate command-line parsing assembly from the Spectre.Console project; used by `DP832.CLI` to register and dispatch `CommandApp` commands. Must be referenced explicitly as it ships as its own NuGet package (`Spectre.Console.Cli`) rather than being bundled inside `Spectre.Console`
- **NI-VISA .NET Framework Libraries** - NI-VISA runtime assemblies for instrument communication
  - NationalInstruments.Common.dll
  - NationalInstruments.VisaNS.dll
  - Installed via NI-VISA Runtime (not available as NuGet package for .NET Framework 4.7.2)

## Technical Details

- **Target Frameworks:**
  - `DP832PowerSupply` / `DP832.Core` / `DP832.WPF` / `DP832.CLI`: .NET Framework 4.7.2 (Windows)
  - `DP832.Helpers`: .NET Standard 2.0 (cross-platform, no hardware dependencies)
  - `DP832.Tests`: .NET 8.0 (cross-platform, runs on Linux/macOS CI)
- **C# Version:** 7.3
- **Build System:** Visual Studio solution with MSBuild
- **UI Framework:** Spectre.Console (console app), WPF (graphical app)
- **Instrument Control:** NI-VISA via `DP832.Core` (`IDP832Device` / `DP832Device`)

## Warnings

> **⚠ IMPORTANT — Electrical Safety**
> The DP832 outputs real DC voltages and currents that can damage equipment or cause injury. Always:
> - Verify voltage and current settings before enabling a channel output.
> - Set appropriate OVP and OCP protection levels before enabling output to protect connected loads.
> - Ensure all connections are made with the output disabled.
> - Do not exceed the rated voltage or current for connected equipment.
> - Resolve any OVP or OCP trip condition (remove the over-voltage or over-current source) before clearing the trip latch.

> **⚠ Over Temperature Protection (OTP)**
> The DP832 has hardware and software over-temperature protection. Keep the instrument well-ventilated. Do not block the fan or ventilation holes. If OTP trips, power off the instrument, allow it to cool, and investigate the cause before resuming operation.

> **⚠ Device Reset**
> The **Reset Device** function resets all settings to factory defaults. This action cannot be undone. Save the device state first using **Save/Load State → Save State to Local File** if you need to restore it later.

> **⚠ Load State**
> Loading a state from a file or device memory will immediately overwrite all current channel settings and system configuration. The application shows a confirmation prompt before applying the loaded state.

## Troubleshooting

### Connection Issues

**Error: "Connection failed"**
- Verify NI-VISA Runtime is installed
- Check that the device is powered on
- Verify the device address is correct
- For GPIB: Ensure the GPIB interface card is properly installed and the device GPIB address matches the address used in the application
- For TCPIP: Ping the device IP address to verify network connectivity (`ping <ip>`)
- Check that the device's GPIB address or IP is correctly configured in the instrument's Utility menu

**Error: "VISA not found"**
- Install the NI-VISA Runtime from the National Instruments website
- Restart your computer after installation
- Ensure the NI-VISA assemblies (`NationalInstruments.Common.dll`, `NationalInstruments.VisaNS.dll`) are present in the Global Assembly Cache (GAC) or the application directory

### Device-Specific Issues

**Cannot find device**
- Use NI MAX (Measurement & Automation Explorer) to scan for connected instruments
- Verify the device appears in NI MAX before using this application
- For USB connections, check that the USB device driver is correctly installed

**Channel output does not turn on**
- Check whether OVP or OCP has tripped on the channel. The Channel Status view shows trip state
- Use **Clear Protection Trip** from the Channel Controls menu to unlatch the protection condition before re-enabling output
- Ensure the output has been enabled via **Advanced Options → Configure Output State**
- Verify the voltage and current settings are within the channel's rated range (CH1/CH2: 0–30 V / 0–3 A, CH3: 0–5 V / 0–3 A)

**OVP or OCP trips immediately after enabling protection**
- The protection level may be set below the current output level. Check and increase the OVP or OCP level to a value above the normal operating point
- OVP range: CH1/CH2: 0.01–31 V, CH3: 0.01–6 V
- OCP range: all channels: 0.001–4 A

**Constant voltage output is abnormal**
- Check whether the maximum output power of the channel fulfils the load requirement
- Check whether the cable connecting the load is short-circuited and making good contact
- Check whether the load is operating normally
- Check whether the current limit is set too low; if so, increase the current setting appropriately

**Constant current output is abnormal**
- Check whether the maximum output power of the channel meets the load requirement
- Check whether the cable connecting the load is short-circuited and making good contact
- Check whether the load is operating normally
- Check whether the voltage setting is set too low; if so, increase the voltage setting appropriately

**State file cannot be loaded**
- Verify the file path is correct and the file exists
- Ensure the file uses the expected `key=value` format (one setting per line, lines starting with `#` are comments)
- Numeric values in the file must use a period (`.`) as the decimal separator

**Device memory slot cannot be recalled**
- Verify a state has previously been saved to that slot using **Save State to Device Memory**
- Device memory slots (1–10) persist across power cycles but are cleared by a factory reset

**Application does not start / build fails**
- Verify .NET Framework 4.7.2 Developer Pack is installed
- Verify NI-VISA Runtime is installed (provides the NI-VISA assemblies)
- Check that the solution builds without errors in Visual Studio (Ctrl+Shift+B)

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

The solution is divided into four projects following a layered architecture:

**`DP832.Helpers`** (shared, cross-platform, no hardware dependencies):

| Class / Method | Description |
|----------------|-------------|
| `DeviceHelpers.ParseProtectionState` | Parse SCPI ON/OFF/YES/NO/1/0 responses |
| `DeviceHelpers.GetChannelMaxVoltage` | Channel voltage limits (CH1/CH2: 30 V, CH3: 5 V) |
| `DeviceHelpers.GetChannelMaxCurrent` | Channel current limit (all: 3 A) |
| `DeviceHelpers.IsValidVoltage` | Voltage range validation |
| `DeviceHelpers.IsValidCurrent` | Current range validation |
| `DeviceHelpers.IsValidOvpLevel` | OVP level range validation |
| `DeviceHelpers.IsValidOcpLevel` | OCP level range validation |
| `DeviceHelpers.FormatGpibAddress` | Format GPIB VISA resource string |
| `DeviceHelpers.FormatTcpipAddress` | Format TCPIP VISA resource string |
| `DeviceHelpers.ParseStateFile` | Parse key=value state file lines |

**`DP832.Core`** (shared device communication layer, net472):

| Class / Interface | Description |
|-------------------|-------------|
| `IDP832Device` | Device abstraction interface — connect, disconnect, send command/query, get errors |
| `DP832Device` | NI-VISA implementation of `IDP832Device` |

**`DP832PowerSupply`** (interactive console application, net472):

| Method | Description |
|--------|-------------|
| `ShowTitle()` | Displays the application title banner |
| `ShowDescription()` | Shows the application description and features |
| `ShowMainMenu()` | Interactive main menu loop |
| `ShowMenuWithEsc()` | Reusable menu with ESC key support |
| `ConfigureDeviceAddress()` | Device address configuration (GPIB/TCPIP/USB/Custom) |
| `ConnectToDevice()` | VISA connection handling |
| `DisconnectFromDevice()` | Connection cleanup |
| `ShowCurrentSettings()` | Display current configuration and all channel status |
| `ChannelControlsMenu()` | Menu for selecting a channel |
| `ChannelControlSubMenu()` | Per-channel controls (voltage, current, OVP, OCP, status, clear trip) |
| `SetChannelVoltage()` | Set output voltage for a channel |
| `SetChannelCurrent()` | Set current limit for a channel |
| `ConfigureOVP()` | Configure OVP level and state |
| `ConfigureOCP()` | Configure OCP level and state |
| `ViewChannelStatus()` | Display real-time channel measurements and protection state |
| `ClearProtectionTrips()` | Clear latched OVP/OCP trip conditions |
| `CheckAndWarnProtectionTrips()` | Detect and warn about active protection trips |
| `AdvancedOptionsMenu()` | Menu for system-level advanced options |
| `ConfigureOutputState()` | Enable/disable output for individual or all channels |
| `ConfigureTracking()` | Configure channel tracking mode and per-channel tracking |
| `ConfigureOTP()` | Enable/disable Over Temperature Protection |
| `ConfigureBeeper()` | Enable/disable the instrument beeper |
| `ConfigureDisplaySettings()` | Sub-menu for display settings |
| `ConfigureDisplayBrightness()` | Set display brightness level (1–8) |
| `ConfigureScreenSaver()` | Enable/disable the screen saver |
| `SaveLoadStateMenu()` | Menu for saving and loading device state |
| `SaveStateToFile()` | Save full device state to a local text file |
| `LoadStateFromFile()` | Restore device state from a local text file |
| `SaveStateToDevice()` | Save state to device memory slot (*SAV) |
| `LoadStateFromDevice()` | Load state from device memory slot (*RCL) |
| `ResetDevice()` | Reset device to factory defaults (*RST) |
| `SendCommandAndCheckErrors()` | Send a SET command and verify SCPI errors |
| `CheckScpiErrors()` | Read and report SCPI error queue entries |
| `PauseOnError()` | Pause execution after displaying an error |

**`DP832.WPF`** (graphical Windows application skeleton, net472):

| File | Description |
|------|-------------|
| `App.xaml / App.xaml.cs` | WPF application entry point |
| `MainWindow.xaml` | Main window layout with connection panel and status area |
| `MainWindow.xaml.cs` | Connect/disconnect logic using `IDP832Device` |

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
