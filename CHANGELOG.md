# Changelog

All notable changes to this project are documented in this file.

## [1.0.4] - 2026-05-02
### Changed
- Improve robustness when the simulator shuts down or the network is unreliable:
  - Guard SimConnect message dispatch to prevent application crashes when MSFS exits unexpectedly.
  - Harden `SimConnectClient.Disconnect()` to safely unsubscribe handlers and dispose the client.
  - Make `HttpTransmitter.BuildUrl()` defensive against missing settings and null data.
  - Make `frmMain.HandleDataReceived()` defensive (null checks, UI-thread marshaling, catch unexpected exceptions).
- Update `README.md` with robustness notes and changelog entry.
- Bump assembly version to `1.0.4.0`.

## [1.0.3]
- Previous release (assembly version `1.0.3.0`).

## Unreleased
- Consider adding minimal error logging (file or Event Log).
- Consider a retry/backoff policy for transient HTTP failures.
