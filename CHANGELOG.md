# Changelog

## 0.6.0 — First public GitHub release

### Fixed

- Duplicate legacy drone-backup IDs no longer block profile persistence after upgrading.
- Known legacy duplicates are recovered without discarding their settings; migration creates a pre-change backup.
- The editor confirms a successful PC write before reporting saved settings. A failed write retains the draft and shows its cause.
- Stale Host snapshots no longer replace a newly saved profile.
- Editing pauses input while preserving the connection. Applying parameter changes to the same profile keeps Host armed, so the phone can resume without re-pairing or recalibration.

### Included from earlier local versions

- Android motion controller and Windows Host with a shared dark UI.
- Car, universal and Brick Rigs drone presets; mouse/keyboard, Xbox RT/LT and optional vJoy output.
- Desktop drag/resize editor with up to 12 configurable touch buttons.
- One-degree steering activation, independent X/Y mouse scaling, 64,000 counts/s velocity and 1,280 counts/degree displacement limits.
- PIN approval and encrypted remembered-device credentials for later authenticated reconnects.
- Local Wi-Fi and Android USB-tethering transport, input watchdogs, live profile sync and import/export.

### Public repository packaging

- English/Ukrainian README and installation guides.
- Actual interface screenshots, README hero, control reference and troubleshooting.
- Portable development paths/test imports, Node dependency metadata and CI checks.
- Downloadable APK, Windows installer, source ZIP, SHA-256 manifest and validation report.

Earlier 0.1–0.5 builds were local development releases. This entry describes the first public repository package; it does not imply the historical local builds were previously published here.
