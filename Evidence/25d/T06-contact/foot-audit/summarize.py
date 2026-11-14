"""Summarize the read-only Unity foot audit, excluding intentional flight poses."""
import json
import statistics
from collections import defaultdict
from pathlib import Path

root = Path(__file__).resolve().parent
data = json.loads((root / 'run-60/foot-contact.json').read_text(encoding='utf-8'))
groups = defaultdict(list)
for sample in data['samples']:
    if sample['source'] == 'Run' and not sample['expectedContact']:
        continue
    if sample['source'] == 'Jump' and sample['jumpState'] not in ('Land', 'Recover'):
        continue
    state = sample['source']
    if state == 'Jump':
        state += '/' + sample['jumpState']
    groups[(sample['support'], state)].append(sample)

rows = []
for (support, state), samples in sorted(groups.items()):
    valid = [s for s in samples if s['soleSupported']]
    def span(key):
        values = [s[key] for s in valid]
        return None if not values else {'min': min(values), 'median': statistics.median(values), 'max': max(values)}
    row = {'support': support, 'state': state, 'samples': len(samples),
           'soleOutsideSupport': len(samples) - len(valid),
           'soleGapMetres': span('soleGap'), 'screenGapPixels': span('screenGap'),
           'rootGapMetres': span('rootGap'), 'anchorGapMetres': span('anchorGap')}
    rows.append(row)
    print(json.dumps(row, ensure_ascii=False))
(root / 'summary.json').write_text(json.dumps({'totalGroundedSamples': len(data['samples']), 'groups': rows}, indent=2), encoding='utf-8')
