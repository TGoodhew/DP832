# DP832 Power Supply Controller

A console application for controlling the Rigol DP832 programmable DC power supply using SCPI commands over GPIB or TCPIP interfaces.

## Prerequisites

### .NET Framework 4.7.2
This project targets .NET Framework 4.7.2 with C# 7.3. Ensure you have the .NET Framework 4.7.2 Developer Pack installed.

### NI-VISA Runtime
The application requires National Instruments VISA (NI-VISA) to be installed on your system to communicate with test and measurement instruments.

**Download NI-VISA:**
- Visit: https://www.ni.com/en-us/support/downloads/drivers/download.ni-visa.html
- Download and install the appropriate version for your system
- The installation will add the required assemblies to the Global Assembly Cache (GAC)

**Required Assemblies:**
- `NationalInstruments.Common.dll`
- `NationalInstruments.VisaNS.dll`

These assemblies are typically located in:
- `C:\Program Files (x86)\IVI Foundation\VISA\VisaNS\` (Windows)
- Global Assembly Cache (GAC)

## Building the Project

### Using Visual Studio
1. Open `DP832.sln` in Visual Studio 2017 or later
2. Ensure NI-VISA is installed on your system
3. Build the solution (Ctrl+Shift+B)

### Using Command Line
```bash
# From the repository root
dotnet build DP832.sln

# Or using MSBuild
msbuild DP832.sln
```

## Running the Application

After building, run the executable:
```bash
DP832PowerSupply\bin\Debug\net472\DP832PowerSupply.exe
```

Or run directly from Visual Studio (F5).

## Features

- Configure GPIB/TCPIP device address
- Connect and disconnect from the power supply
- Query device identification (*IDN?)
- View connection status and settings
- Rich console UI powered by Spectre.Console

## Configuration

The default device address is `GPIB0::1::INSTR`. You can change this from the application menu.

### Connection Examples
- **GPIB:** `GPIB0::1::INSTR`
- **TCPIP:** `TCPIP::192.168.1.100::INSTR`

## Dependencies

- **Spectre.Console** (v0.49.1) - Rich console UI library
- **NI-VISA** - National Instruments VISA library for instrument control

## Target Framework

- **.NET Framework:** 4.7.2
- **C# Version:** 7.3

## License

See LICENSE file in the repository root.
