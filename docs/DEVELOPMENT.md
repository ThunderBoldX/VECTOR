# Development

[← Overview](../README.md) · [Install the released apps](INSTALL.md)

## Layout

| Directory | Contents |
| --- | --- |
| android/ | Java Android app, motion engine, UDP client, Keystore-backed device trust |
| windows/ | Electron Host, authenticated UDP receiver, profile storage, C# native input worker |
| ui/ | Shared local HTML/CSS/JS UI and licensed fonts/assets |
| tests/ | Node unit/UDP tests, Java motion/crypto checks and browser/Host integration checks |
| scripts/ | Local preview, screenshot capture, documentation and packaging tools |
| docs/ | Installation, controls, screenshots and release notes |

At runtime index.html loads core.js, vector.js and vector.css. The older app.js, design.css and templates.js files are retained historical UI assets and are not the current entry point.

The phone sends authenticated input to UDP 26760. Host checks session, endpoint, sequence and HMAC before output. Profile revisions must be acknowledged before game input is accepted. Editing requests a pause; applying the same profile preserves the enabled connection. A native worker emits Windows input and independently releases it on a timeout.

## Local preview

Node.js 22+ is sufficient for the UI preview and unit tests:

~~~sh
npm test
npm run preview
~~~

Open http://127.0.0.1:8769 for the phone UI, or add ?desktop=1 for the desktop preview. This is a UI preview only; native sensors and Windows game input require the Android/Host apps.

## Build Android

On Windows, install JDK 17+, Android SDK platform 35 and build-tools 35.0.0. Then:

~~~powershell
./android/Build-Android.ps1 -SdkRoot C:/Android/Sdk -JdkRoot C:/Java/jdk-17
~~~

Alternatively set ANDROID_SDK_ROOT (or ANDROID_HOME) and JAVA_HOME. The script uses aapt2, javac, D8, zipalign and apksigner directly; Gradle is not required. Output is release/VECTOR-Android-0.6.0.apk.

A missing android/signing/vector-development.jks is generated for your local build. Signing keys are intentionally excluded from this repository and source archive. Locally signed builds cannot update an APK signed with a different key; keep your own signing identity consistent.

## Build Windows

Windows x64 with the .NET Framework C# compiler is required. Install the pinned Electron development dependency and fetch the third-party SDK/driver:

~~~powershell
npm --prefix windows install
./windows/Fetch-XboxSDK.ps1
./windows/Build-Windows.ps1
./windows/Build-Installer.ps1 -MakeNsis C:/NSIS/makensis.exe
~~~

Or provide an existing runtime with -ElectronRoot C:/Electron/dist and, if needed, -Csc to Build-Windows.ps1. NSIS 3 is needed for the installer. The native worker's self-test checks report ABI and SDK loading without installing the driver.

Outputs are release/windows-0.6/ and release/VECTOR-Host-Windows-0.6.0-Setup.exe. Fetch-XboxSDK.ps1 downloads fixed SDK and ViGEmBus versions, checks SHA-256 values and verifies the driver's Windows publisher signature. It does not install the driver.

The released binaries predate the public-repository packaging. Runtime Android Java, Windows C#/CJS and shared UI files are unchanged; public packaging adds documentation, portable build paths, dependency metadata and portable test imports.

## Testing

### Unit and UDP checks

~~~sh
npm test
~~~

The current suite has 27 checks covering mappings, one-degree steering, motion scaling, authenticated input, replay rejection, pairing retry/expiry, remembered devices, migration and profile persistence. These tests use isolated mock/native interfaces and local UDP sockets; they do not send real game input or install drivers. GitHub Actions runs this suite and documentation checks.

### Browser and packaged Host

~~~powershell
npm ci
npx playwright install chromium
npm run test:ui
# Build the Windows app first, then on Windows:
npm run test:host
~~~

To use an installed Chrome instead of Playwright Chromium, set CHROME_PATH to its executable. Host tests use an isolated temporary user-data directory, random UDP ports and VECTOR_SMOKE=1, which prevents OS input. Run packaged Host tests serially: they share the dedicated test directory.

Browser checks exercise layout editing, save success/failure, stale state, multi-touch and phone pause/resume. Packaged Host checks exercise real IPC, UDP, protected Windows storage, profile transfer on the same session and persistence after restart.

### Java checks

~~~powershell
New-Item -ItemType Directory -Force qa/java
javac -encoding UTF-8 -d qa/java android/src/app/vector/controller/MotionEngine.java tests/MotionEngineTest.java
java -cp qa/java MotionEngineTest
~~~

SessionCryptoTest.java separately compares Java derivation against the Host HMAC implementation using a shared known vector.

### What automated checks do not prove

They do not validate every physical phone, Android Keystore implementation, installed ViGEmBus/vJoy driver or game session. Published UI screenshots use fictional example data. Game-specific bindings and actual end-to-end latency should be tested on the target hardware.

## Reproduce the screenshots

~~~powershell
npm ci
npx playwright install chromium
npm run docs:screenshots
node scripts/render-hero.cjs
~~~

This renders the actual shared UI with sample state into docs/screenshots, then composes the README banner from those captures. No game footage or measured performance is implied.

## Release packaging

Run python scripts/package-release.py after the documentation and screenshots are final. It copies the validated APK and installer from a supplied asset directory, checks the pinned binary hashes, creates a clean source ZIP, writes SHA256SUMS and records packaging checks. See the script's --help output.

Binaries, downloaded vendor SDKs, temporary device state, private signing keys and build folders are excluded from Git. Release assets belong in GitHub Releases, not in the source tree.
