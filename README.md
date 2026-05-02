# VirtualFlight Online Transmitter (MSFS)

VirtualFlight.Online Transmitter (MSFS) - Version 1.0.4

A lightweight Windows Forms client that reads live aircraft data from Microsoft Flight Simulator via SimConnect and transmits periodic position updates to the VirtualFlight.Online transmitter service.

## Summary

This application connects to MSFS using the SimConnect API, samples the user's aircraft state once per second, displays the values in a simple UI, and sends the snapshot to a configured HTTP transmit endpoint. It is intended for use with the VirtualFlight.Online network but can be pointed at other compatible endpoints.

## Features

- Connects to MSFS using `SimConnect` and requests a single data snapshot on demand.
- Displays aircraft information (type, lat/lon, altitude, heading, airspeed, groundspeed, touchdown velocity, transponder) in the UI.
- Sends non-blocking HTTP GET transmissions to a configurable server endpoint using a single shared `HttpClient` instance.
- Stores multiple server profiles in application settings (JSON) and supports a default profile.
- Optional auto-connect and reconnect timers.

## Requirements

- Microsoft Windows
- .NET Framework 4.7.2
- Microsoft Flight Simulator (SimConnect SDK component available)

Note: The project references the `Microsoft.FlightSimulator.SimConnect` assembly. Ensure SimConnect is available on the development or target machine (it is normally installed with MSFS or the SimConnect SDK).

## Building

Open the solution or the `VirtualFlightOnlineTransmitter.csproj` in Visual Studio that targets .NET Framework 4.7.2 and build normally.

## Running

1. Launch the application while MSFS is running (or start MSFS after enabling auto-connect).
2. Configure your Callsign, Pilot Name and Group in the main window — these fields are required before connecting.
3. Click Connect (or enable Auto-Connect in the menu).

When connected the app requests a fresh data snapshot once per second and transmits it to the configured server URL.

## Configuration / Settings

Settings are stored via the application's `Properties.Settings`. Key settings and their purposes:

- `servers`: JSON array of server profiles. A default profile is provided by the app.
- `selectedServer`: Index of the active profile in the `servers` array.
- `serverURL`: The full transmit endpoint URL (e.g. `https://transmitter.virtualflight.online/transmit`).
- `pin`: Optional PIN for server authentication.
- `callsign`, `pilotName`, `groupName`, `msfsServer`, `notes`: Profile values displayed and transmitted.
- `AutoConnect`: `true` or `false` to control automatic connection on startup.

The application keeps the `servers` JSON array and mirrors the selected profile into the flat keys above so internal components can read them without parsing JSON.

## Troubleshooting

- If the app reports it cannot connect to the simulator, ensure MSFS is running and that SimConnect is installed and matches the simulator version.
- If HTTP transmissions fail, verify the `serverURL` and network connectivity. The UI shows a short status string for each transmit attempt (round-trip time on success or an error description).

## Robustness improvements (recent)

Recent updates improve stability when the simulator shuts down unexpectedly or when the network is unreliable. Key behaviours:

- The application now guards SimConnect message dispatch so exceptions thrown by SimConnect (for example when MSFS exits) are caught and handled. The app attempts a clean disconnect and updates the UI instead of crashing.
- Disconnect logic has been hardened to safely unsubscribe event handlers and dispose of the SimConnect client even if the underlying connection is already in a bad state.
- HTTP URL construction is defensive against missing settings and null data fields to avoid runtime exceptions when building requests.
- The data handler that updates the UI and triggers transmissions is more defensive: it guards against null data, ensures UI updates occur on the UI thread, and catches unexpected exceptions so they do not propagate into SimConnect's message dispatch.

These changes mean simulator shutdowns and temporary internet outages fail gracefully: the UI shows an error/status message and the app remains running so you can reconnect or investigate.

Recommended next steps (optional): add minimal error logging (file or Event Log) for caught exceptions and consider a retry/backoff policy for transient HTTP failures.

## Changelog

- 1.0.4 — Improve robustness: guard SimConnect dispatch, safer disconnect, defensive HTTP URL building, defensive data handler, README updates (2026-05-02).
- 1.0.3 — Previous release (assembly version reflected in earlier builds).

## Extending

The code is intentionally small and focused. Useful extension points:

- Add server management UI (add/remove/edit profiles) — placeholders exist in the form code.
- Add more SimConnect variables or change sampling frequency in `SimConnectClient` / timers.
- Migrate to an HTTPS POST payload if the server requires a richer or authenticated API.

## Source layout

- `frmMain.cs` — WinForms UI and main application logic.
- `SimConnectClient.cs` — wrapper around SimConnect; requests and decodes simulator data.
- `HttpTransmitter.cs` — builds query URL and performs HTTP GET transmissions.
- `PlaneData.cs` — POCO carrying a single snapshot.
- `Server.cs` — serialisable server profile class.

## License
No license information is included in this repository. Check the project root for a `LICENSE` file or consult the repository owner.