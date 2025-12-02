# VirtualFlight.Online Transmitter

![Version](https://img.shields.io/badge/version-1.0.2.3-blue)
![Platform](https://img.shields.io/badge/platform-Windows-lightgrey)
![Framework](https://img.shields.io/badge/.NET%20Framework-4.7.2-purple)
![License](https://img.shields.io/badge/license-GPL--3.0-green)

**Transmitter** is a Windows desktop client application that connects to Microsoft Flight Simulator 2020/2024 and broadcasts your flight data to [VirtualFlight.Online](https://virtualflight.online) servers in real-time. This allows other pilots and enthusiasts to track your flights on radar maps, see who's currently flying, and share your virtual aviation adventures with the community.

## 🎯 Features

- **Real-Time Flight Tracking** - Continuously transmits your aircraft position, altitude, heading, and speed to VirtualFlight.Online servers
- **SimConnect Integration** - Uses the official Microsoft Flight Simulator SimConnect API via the CTrue.FsConnect library
- **Multiple Server Support** - Configure and switch between multiple tracking servers
- **Auto-Connect** - Optionally automatically connect when the simulator is running
- **Low Latency** - Displays communication response times to monitor connection quality
- **Session Timer** - Tracks how long you've been connected to the simulator
- **Detailed Aircraft Data** - Displays comprehensive flight information including:
  - Aircraft Type
  - Latitude/Longitude (in degrees, minutes, seconds format)
  - Indicated Altitude
  - True Heading
  - Indicated Airspeed
  - Ground Speed
  - Touchdown Velocity
  - Transponder Code

## 📋 Requirements

- **Operating System:** Windows 10/11 (64-bit)
- **Flight Simulator:** Microsoft Flight Simulator 2020 or Microsoft Flight Simulator 2024
- **.NET Framework:** 4.7.2 or later
- **Internet Connection:** Required for transmitting data to tracking servers

## 🚀 Installation

### Option 1: Download the Installer
1. Download the latest `transmitter_installer.exe` from the [Releases](https://github.com/jonbeckett/vfo-transmitter-client-msfs/releases) page
2. Run the installer and follow the on-screen instructions
3. Launch **Transmitter** from the Start Menu or Desktop shortcut

### Option 2: Build from Source
1. Clone this repository:
   ```bash
   git clone https://github.com/jonbeckett/vfo-transmitter-client-msfs.git
   ```
2. Open `VirtualFlightOnlineTransmitter.sln` in Visual Studio 2019 or later
3. Restore NuGet packages
4. Build the solution in Release configuration
5. Run `Transmitter.exe` from the `bin\Release` folder

## 🎮 Usage

### First-Time Setup
1. Launch **Transmitter**
2. Configure your settings:
   - **Callsign** - Your unique identifier (e.g., "N12345" or "BA123")
   - **Pilot Name** - Your name as you want it displayed
   - **Group Name** - Your virtual airline or flying group (default: "VirtualFlight.Online")
   - **MSFS Server** - The multiplayer server region you're connected to
   - **Notes** - Optional notes about your flight
3. The default server URL is pre-configured to `https://transmitter.virtualflight.online/transmit`

### Connecting to the Simulator
1. Start Microsoft Flight Simulator and load into a flight
2. Click the **Connect** button in Transmitter
3. Once connected, your flight data will be transmitted every few seconds
4. The status bar shows:
   - Connection duration (elapsed time)
   - Latest position data
   - Server response time in milliseconds

### Auto-Connect Feature
Enable **Options → Auto Connect** to have Transmitter automatically attempt to connect when the simulator is detected. If connection fails, it will retry every 5 seconds.

### Managing Multiple Servers
You can configure multiple tracking servers for different communities:
1. Click **Add Server** to create a new server profile
2. Configure the server settings (Name, URL, credentials)
3. Switch between servers using the server list
4. Remove unused servers with the **Remove Server** button

### Disconnecting
- Click **Disconnect** to stop transmitting data
- Closing the application while connected will prompt for confirmation

## 📊 Transmitted Data

The following flight parameters are transmitted to the server:

| Parameter | Description |
|-----------|-------------|
| `Callsign` | Your configured callsign |
| `PilotName` | Your pilot name |
| `GroupName` | Your virtual airline/group |
| `MSFSServer` | MSFS multiplayer server region |
| `AircraftType` | Full aircraft title from the simulator |
| `Latitude` | Current latitude in decimal degrees |
| `Longitude` | Current longitude in decimal degrees |
| `Altitude` | Indicated altitude in feet |
| `Heading` | True heading in degrees |
| `Airspeed` | Indicated airspeed in knots |
| `Groundspeed` | GPS ground speed in knots |
| `TouchdownVelocity` | Last touchdown velocity (feet/second) |
| `TransponderCode` | Current transponder squawk code |
| `Version` | Transmitter client version |
| `Notes` | Optional flight notes |

## 🌐 VirtualFlight.Online Resources

- **Website:** [https://virtualflight.online](https://virtualflight.online)
- **Live Radar:** [https://transmitter.virtualflight.online/radar](https://transmitter.virtualflight.online/radar)
- **Who's Online:** [https://transmitter.virtualflight.online/status](https://transmitter.virtualflight.online/status)
- **Virtual Airline:** [https://airline.virtualflight.online](https://airline.virtualflight.online)
- **Forums:** [https://forums.virtualflight.online](https://forums.virtualflight.online)
- **Discord:** [https://bit.ly/virtualflightonlinediscordserver](https://bit.ly/virtualflightonlinediscordserver)
- **Facebook Group:** [https://www.facebook.com/groups/virtualflight.online](https://www.facebook.com/groups/virtualflight.online)
- **Newsletter:** [https://virtualflightonline.substack.com](https://virtualflightonline.substack.com)
- **Patreon:** [https://patreon.com/virtualflightonline](https://patreon.com/virtualflightonline)

## 🔧 Technical Details

### Architecture
- **Language:** C# (.NET Framework 4.7.2)
- **UI Framework:** Windows Forms
- **SimConnect Library:** [CTrue.FsConnect](https://github.com/c-true/FsConnect) v1.4.0
- **Platform Target:** x64

### Key Dependencies
- `CTrue.FsConnect` - SimConnect wrapper for .NET
- `CTrue.FsConnect.Managers` - Additional SimConnect management utilities
- `Microsoft.FlightSimulator.SimConnect` - Official SimConnect SDK
- `System.Text.Json` - JSON serialization for server configuration
- `Newtonsoft.Json` - Additional JSON support
- `Serilog` - Logging framework

### SimConnect Variables Read
```csharp
- Title (Aircraft Name)
- PlaneLatitude
- PlaneLongitude
- IndicatedAltitude
- PlaneHeadingDegreesTrue
- AirspeedIndicated
- GpsGroundSpeed
- PlaneTouchdownNormalVelocity
- TransponderCode
```

## 🛠️ Building the Installer

The installer is built using [Inno Setup](https://jrsoftware.org/isinfo.php). To create a new installer:

1. Build the project in Release configuration
2. Open `installer/installer.iss` in Inno Setup Compiler
3. Update the version number in the script if needed
4. Compile the script to generate `transmitter_installer.exe`

## 🤝 Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the GNU General Public License v3.0 - see the [LICENSE](LICENSE) file for details.

## 👤 Author

**Jonathan Beckett**
- Website: [VirtualFlight.Online](https://virtualflight.online)
- GitHub: [@jonbeckett](https://github.com/jonbeckett)

## 🙏 Acknowledgments

- [CTrue](https://github.com/c-true) for the excellent FsConnect library
- The Microsoft Flight Simulator development team for SimConnect
- The VirtualFlight.Online community for feedback and support

---

<p align="center">
  <i>Happy Flying! ✈️</i>
</p>
