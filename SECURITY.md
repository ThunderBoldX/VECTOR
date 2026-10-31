# Security

VECTOR is a local-network input controller. Run it only on trusted private networks.

## Pairing and stored devices

The first pairing requires the Host's six-digit PIN and explicit approval on the PC. That initial exchange sends a session key over the local network without transport encryption. Do not perform initial pairing on public or untrusted Wi-Fi.

Subsequent input/configuration packets are authenticated with HMAC-SHA256 and checked for session, endpoint and increasing sequence numbers. Remembered-device reconnect uses a signed nonce challenge and derives a fresh session key on both peers. Remembered keys are protected by Windows safeStorage/DPAPI and Android Keystore-backed AES-GCM.

These controls are authentication and integrity measures, not a claim of end-to-end encryption for all traffic. Do not port-forward UDP 26760. Restrict Windows firewall rules to private networks and the local subnet.

## Input lifecycle

Editing, pausing or losing valid input releases controls. Host and its native worker have watchdogs; the phone detects a missing acknowledgment. A saved-device reconnect does not automatically start game input. Forgetting a phone in Host revokes its stored access and disconnects it.

## Reporting a vulnerability

Use [GitHub private vulnerability reporting](https://github.com/ThunderBoldX/VECTOR/security/advisories/new) if it is available for the repository. If unavailable, open an issue asking for a private contact channel without disclosing an exploit, credentials or device tokens. Ordinary bugs belong in Issues.

Never include pairing keys, trusted-device storage files, Android signing keystores or a live PIN in an issue. A UI profile export contains layout/mapping data, but check its labels before sharing.
