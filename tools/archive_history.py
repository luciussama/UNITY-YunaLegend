"""Resolve preserved historical paths and verify archived bytes (stdlib only)."""
import argparse
import hashlib
import json
from pathlib import Path

ROOT = Path(__file__).resolve().parents[1]
MANIFEST = ROOT / 'Archive/pre-25d-2026-11-12/manifest.json'


def main():
    parser = argparse.ArgumentParser(description=__doc__)
    commands = parser.add_subparsers(dest='command', required=True)
    resolve = commands.add_parser('resolve')
    resolve.add_argument('path')
    commands.add_parser('verify')
    args = parser.parse_args()
    rows = json.loads(MANIFEST.read_text(encoding='utf-8'))['files']
    if args.command == 'resolve':
        supplied = Path(args.path.replace('\\', '/'))
        candidate = (ROOT / supplied).resolve()
        try:
            name = candidate.relative_to(ROOT).as_posix()
        except ValueError:
            parser.error('Path must stay inside the repository.')
        if candidate.is_file():
            print(candidate)
            return 0
        for row in rows:
            if row['original_path'] == name:
                print((ROOT / row['archived_path']).resolve())
                return 0
        parser.error('No current file or archived mapping found.')
    failed = []
    for row in rows:
        target = (ROOT / row['archived_path']).resolve()
        if not target.is_relative_to(ROOT) or not target.is_file():
            failed.append(row['archived_path'])
        elif hashlib.sha256(target.read_bytes()).hexdigest() != row['sha256']:
            failed.append(row['archived_path'])
    print(json.dumps({'verified': len(rows) - len(failed), 'total': len(rows), 'failed': failed}))
    return 1 if failed else 0


if __name__ == '__main__':
    raise SystemExit(main())
