"""Validate local documentation links, artwork and public-tree hygiene with stdlib only."""
from pathlib import Path
import re
import struct
import urllib.parse

root = Path(__file__).resolve().parents[1]
failures = []
links = 0
for file in list(root.glob('*.md')) + list((root / 'docs').rglob('*.md')):
    text = file.read_text(encoding='utf-8')
    urls = re.findall(r'!?\[[^\]]*\]\(([^\s)]+)(?:\s+"[^"]*")?\)', text)
    urls += re.findall(r'(?:src|href)="([^"]+)"', text)
    for url in urls:
        url = url.strip('<>')
        if url.startswith(('https://', 'http://', '#', 'mailto:')):
            continue
        target = (file.parent / urllib.parse.unquote(url.split('#')[0].split('?')[0])).resolve()
        links += 1
        if not target.is_relative_to(root) or not target.exists():
            failures.append(f'{file.relative_to(root)}: missing local target {url}')
for name in ['hero.png', 'social-preview.png']:
    image = root / 'docs/assets' / name
    if not image.is_file():
        failures.append(f'Missing {name}')
        continue
    data = image.read_bytes()
    if not data.startswith(b'\x89PNG\r\n\x1a\n'):
        failures.append(f'Invalid PNG: {name}')
    else:
        width, height = struct.unpack('>II', data[16:24])
        expected = (1920, 1080) if name == 'hero.png' else (1280, 640)
        if (width, height) != expected:
            failures.append(f'{name}: unexpected dimensions {(width, height)}')
for name in ['desktop-editor', 'desktop-motion', 'desktop-profiles', 'desktop-connection', 'phone-connect', 'phone-calibration', 'phone-driving', 'phone-drone', 'phone-paused']:
    file = root / 'docs/screenshots' / (name + '.png')
    if not file.is_file() or file.stat().st_size < 1000:
        failures.append(f'Missing screenshot: {name}')
# Source distribution must not include local signing material or author workspace paths.
for folder in ['android', 'windows', 'ui', 'tests', 'scripts']:
    for file in (root / folder).rglob('*'):
        if not file.is_file() or any(p in {'node_modules', 'build', 'signing', '__pycache__'} for p in file.relative_to(root).parts):
            continue
        if file.suffix in {'.cjs', '.js', '.java', '.cs', '.ps1', '.py', '.json', '.html'}:
            text = file.read_text(encoding='utf-8')
            if re.search(r'[A-Z]:[/\\]Users[/\\]', text, re.I):
                failures.append(f'Author-specific path: {file.relative_to(root)}')
            if re.search(r'(?:gh[pousr]_|github_pat_)[A-Za-z0-9_]{20,}|-----BEGIN (?:RSA |EC |OPENSSH )?PRIVATE KEY-----', text):
                failures.append(f'Potential credential: {file.relative_to(root)}')
if failures:
    raise SystemExit('\n'.join(failures))
print(f'PASS {links} local documentation links; 9 screenshots; README/social artwork; portable source paths.')
