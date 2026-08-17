---
layout: page
title: Installation
permalink: /installation.html
---

## Requirements

- Microsoft Windows
- [.NET Framework 4.7.2](https://dotnet.microsoft.com/en-us/download/dotnet-framework/net472) or later
- Microsoft Flight Simulator (2020 or 2024), with the SimConnect component available

SimConnect is normally installed automatically with MSFS. If you built the app yourself or run an unusual MSFS install, you may need to install the SimConnect SDK component separately.

## Installing

1. Download the latest installer (`transmitter_<version>.exe`) from the project's [Releases](https://github.com/jonbeckett/vfo-transmitter-client-msfs/releases) page.
2. Run the installer and follow the prompts. You can optionally create a desktop shortcut.
3. Launch **Transmitter** from the Start Menu or desktop shortcut.

## First run

On first launch, a **default** server profile is pre-filled, pointing at the official VirtualFlight.Online transmit endpoint. Before you can connect, you must:

1. Replace the placeholder **Callsign** and **Pilot Name** with your own details.
2. Enter your **Group Name**.

See [Configuration]({% link configuration.md %}) for details on these and other settings, and [Using the Transmitter]({% link using-the-transmitter.md %}) for how to connect.
