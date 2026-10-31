# Contributing

Thanks for helping improve VECTOR.

For a bug, include the Android model/version, Windows version, VECTOR version, connection type, active output/profile, reproduction steps, expected behavior and a screenshot if useful. Hide personal device names, addresses and live pairing codes. Never upload trusted-device keys or signing keystores.

Run npm test before a pull request. For UI changes, run the browser checks and include a screenshot from the affected phone orientation or desktop page. Keep motion mappings and signed-session validation covered when changing the protocol.

The native worker can send OS input. Prefer the test suite and VECTOR_SMOKE=1 for automated work, with isolated test data and no real game input.

Changes should keep phone usage simple: connect, calibrate, play. Profiles, button layout editing and detailed tuning belong on the PC.
