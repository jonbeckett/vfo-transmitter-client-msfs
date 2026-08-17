---
layout: home
title: Home
permalink: /
---

The **VirtualFlight.Online Transmitter** is a lightweight Windows application that reads live aircraft data from Microsoft Flight Simulator and transmits your position to the VirtualFlight.Online network, so other pilots and controllers can see you flying.

It connects to MSFS using SimConnect, samples your aircraft's position, altitude, heading, speed and transponder code once per second, and sends each snapshot to the VirtualFlight.Online server (or any compatible server you configure).

## In this guide

| Page | What it covers |
|---|---|
| [Installation]({% link installation.md %}) | System requirements and how to install the app. |
| [Configuration]({% link configuration.md %}) | Setting up your callsign, pilot name, and server profile. |
| [Using the Transmitter]({% link using-the-transmitter.md %}) | Connecting, the main window, and the menus. |
| [Troubleshooting & FAQ]({% link troubleshooting.md %}) | Fixes for common connection and transmission problems. |
| [Changelog]({% link changelog.md %}) | What's new in each release. |

## Quick start

1. [Install the app]({% link installation.md %}).
2. Launch Microsoft Flight Simulator, then start the Transmitter.
3. Fill in your **Callsign**, **Pilot Name** and **Group** — these are required before you can connect. See [Configuration]({% link configuration.md %}).
4. Click **Connect** (or turn on **Auto Connect** in the File menu so it connects automatically every time you start the app).

Once connected, the status bar shows your live position and the result of each transmission to the server.

## Useful links

- Website: [virtualflight.online](https://virtualflight.online)
- Discord: [Join the server](https://bit.ly/virtualflightonlinediscordserver)
- Forums: [forums.virtualflight.online](https://forums.virtualflight.online)
- Source code: [GitHub repository](https://github.com/jonbeckett/vfo-transmitter-client-msfs)
