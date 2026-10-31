# Third-party notices

VECTOR's own source is MIT-licensed; see [LICENSE](LICENSE). Dependencies and redistributed assets retain their original licenses.

| Component | Use | License / notice |
| --- | --- | --- |
| Electron | Windows desktop runtime | Electron MIT and bundled Chromium/Node notices distributed with the Windows app; [Electron](https://github.com/electron/electron) |
| ViGEmClient | Native virtual Xbox controller client | [MIT notice](windows/vendor/ViGEmClient-LICENSE.txt) |
| Nefarius.ViGEm.Client | SDK package used to obtain the native client | [MIT notice](windows/vendor/ViGEm.NET-LICENSE.txt) |
| ViGEmBus 1.22.0 | Optional explicitly installed Xbox bus driver | [BSD 3-Clause notice](windows/vendor/ViGEmBus-LICENSE.txt) |
| Inter | UI font | [SIL Open Font License](ui/assets/fonts/Inter-OFL.txt) |
| Roboto Mono | UI monospace font | [SIL Open Font License](ui/assets/fonts/RobotoMono-OFL.txt) |
| Playwright | Development-only browser tests/screenshots | [Apache 2.0](https://github.com/microsoft/playwright/blob/main/LICENSE) |
| NSIS | Development-only installer compiler | [NSIS licenses](https://nsis.sourceforge.io/License) |

Third-party executables and SDK DLLs are not committed here. Fetch-XboxSDK.ps1 downloads pinned packages and retains their license notices. The Windows release bundles the Electron runtime and its shipped notices, the native client, and the separately launched driver installer.

ViGEmBus is an archived third-party project. VECTOR is not affiliated with Nefarius, Microsoft/Xbox, Google/Android, Brick Rigs or the makers of games used with it. Product names identify compatibility and preset intent.
