---
layout: page
title: Troubleshooting & FAQ
permalink: /troubleshooting.html
---

## Connection problems

**"Problem connecting to Simulator"**
- Make sure MSFS is running and you're in a flight, not just at the main menu.
- Confirm SimConnect is installed and matches your simulator version.
- With **Auto Connect** on, the app retries every 5 seconds automatically — you don't need to click Connect again.

**The app won't let me click Connect**
- Replace the placeholder Callsign and Pilot Name with your own values, and make sure Group Name isn't blank. See [Configuration]({% link configuration.md %}).

**"Transmitter is still connected to the simulator" when closing**
- This is expected. It's a safety prompt so you don't lose your session by accident. Choose **Yes** to disconnect and close.

## Transmission problems

**The status bar shows an error instead of a time in milliseconds**
- Check the Server URL in [Configuration]({% link configuration.md %}) is correct and reachable.
- Check your internet connection — transmissions time out after 5 seconds.
- If your server requires a PIN, make sure it's entered correctly.

**Transmissions occasionally seem to skip**
- This is normal — if the server is slow to respond, the app skips a tick rather than sending overlapping requests.

## General

**Where are my settings stored?**
- In your Windows user profile, managed automatically by the app. Use **File → Reset Settings to Defaults** if you need to start over.

**Can I use a different server?**
- Yes — enter any compatible server URL in [Configuration]({% link configuration.md %}).

**Still stuck?**

Ask in the [VirtualFlight.Online Discord](https://bit.ly/virtualflightonlinediscordserver), post on the [forums](https://forums.virtualflight.online), or [open an issue](https://github.com/jonbeckett/vfo-transmitter-client-msfs/issues) on GitHub.
