---
layout: page
title: Using the Transmitter
permalink: /using-the-transmitter.html
---

## The main window

- **Server panel** — your Callsign, Pilot Name, Group Name, MSFS Server region and Notes.
- **Aircraft Data panel** — live read-only values from the simulator: Aircraft Type, Latitude, Longitude, Altitude, Heading, Airspeed, Groundspeed and Landing Rate.
- **Status bar** — shows the simulator connection state, a live summary of your position while connected, the result of the most recent transmission (a round-trip time, or an error), and your elapsed connection time.

## Connecting

1. Fill in **Callsign**, **Pilot Name** and **Group Name** (see [Configuration]({% link configuration.md %})).
2. Click **Connect**.
3. While connected, your identity fields are locked to prevent changing them mid-flight.
4. The app requests a fresh position from the simulator every second, updates the display, and sends it to your configured server.

If the app can't reach the simulator, the status bar shows *"Problem connecting to Simulator"*. With **Auto Connect** enabled, it keeps retrying automatically every 5 seconds.

## Disconnecting

Click **Disconnect**, or simply close the window. If you're still connected when you close the app, you'll be asked to confirm first.

## Menus

### File

- **Auto Connect** — connect to the simulator automatically on startup.
- **Reset Settings to Defaults** — restore the built-in default server profile.
- **Exit** — close the application.

### View

- **Aircraft Data** — show or hide the Aircraft Data panel.
- **Who's Online?** — opens the VirtualFlight.Online status page showing who's currently flying.
- **Radar** — opens the VirtualFlight.Online radar map.

### Links

Quick access to the VirtualFlight.Online community — Website, Newsletter, Airline, Discord, Facebook and Patreon — each opens in your default browser.

### Help

- **About** — shows the current application version.
