"""Package the existing validated 0.6.0 binaries with the current committed public source."""
from pathlib import Path
import argparse
import hashlib
import json
import shutil
import subprocess
import zipfile

ROOT = Path(__file__).resolve().parents[1]
VERSION = '0.6.0'
PINNED = {
    'VECTOR-Android-0.6.0.apk': '0bad85688674ac171a77f8e4ab2bba1d7473790bf8758bf7b472c9d3558d741e',
    'VECTOR-Host-Windows-0.6.0-Setup.exe': '6132a71c7f75dcde4c89eda0f3c57c6550c0fa90ea6d763291b8b45bfe705ce5',
}
parser = argparse.ArgumentParser(description=__doc__)
parser.add_argument('--assets', type=Path, required=True, help='Directory containing the validated APK, installer and VALIDATION-0.6.0.json')
args = parser.parse_args()
assets = args.assets.resolve()
out = ROOT / 'release'
out.mkdir(exist_ok=True)
for name, expected in PINNED.items():
    source = assets / name
    if hashlib.sha256(source.read_bytes()).hexdigest() != expected:
        raise SystemExit(f'Binary integrity check failed: {name}')
    target = out / name
    if source != target:
        shutil.copy2(source, target)
subprocess.run(['python', str(ROOT / 'scripts/check-docs.py')], check=True, cwd=ROOT)
commit = subprocess.check_output(['git', 'rev-parse', 'HEAD'], cwd=ROOT, text=True).strip()
source_name = f'VECTOR-Source-{VERSION}.zip'
subprocess.run(['git', 'archive', '--format=zip', '--prefix=VECTOR/', '--output=' + str(out / source_name), commit], cwd=ROOT, check=True)
with zipfile.ZipFile(out / source_name) as archive:
    for name in archive.namelist():
        if any(p in {'signing', '.git', 'node_modules', 'release', 'qa'} for p in Path(name).parts) or name.endswith(('.jks', '.keystore')):
            raise SystemExit(f'Excluded local file in source archive: {name}')
    assert 'VECTOR/docs/assets/hero.png' in archive.namelist()
    assert 'VECTOR/README_UK.md' in archive.namelist()
    assert 'VECTOR/android/src/app/vector/controller/MainActivity.java' in archive.namelist()
report = json.loads((assets / f'VALIDATION-{VERSION}.json').read_text(encoding='utf-8'))
files = [*PINNED, source_name]
report['artifacts'] = {name: (out / name).stat().st_size for name in files}
report['public_repository_packaging'] = {
    'source_commit': commit,
    'runtime_binaries_unchanged': True,
    'source_archive_from_committed_tree': True,
    'private_signing_keys_excluded': True,
    'english_and_ukrainian_guides': True,
    'actual_ui_screenshots': 9,
    'readme_and_social_artwork': True,
    'screenshot_data': 'Fictional example devices and illustrative input/RTT values',
}
report_name = f'VALIDATION-{VERSION}.json'
(out / report_name).write_text(json.dumps(report, ensure_ascii=False, indent=2) + '\n', encoding='utf-8')
files.append(report_name)
manifest = '\n'.join(hashlib.sha256((out / name).read_bytes()).hexdigest() + '  ' + name for name in files) + '\n'
(out / f'SHA256SUMS-{VERSION}.txt').write_text(manifest, encoding='utf-8')
print(json.dumps({'version': VERSION, 'source_commit': commit, 'assets': files + [f'SHA256SUMS-{VERSION}.txt']}, indent=2))
