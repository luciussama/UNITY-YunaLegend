"""Meaningful coordination tests; every mutation uses an isolated state copy."""
import hashlib
import importlib.util
import json
from pathlib import Path
import subprocess
import sys
import tempfile
import unittest
from unittest.mock import patch

ROOT = Path(__file__).resolve().parents[1]
SCRIPT = ROOT / 'tools/plan_25d.py'
spec = importlib.util.spec_from_file_location('plan_25d', SCRIPT)
coordinator = importlib.util.module_from_spec(spec)
spec.loader.exec_module(coordinator)


class CoordinatorTests(unittest.TestCase):
    def setUp(self):
        self.temporary = tempfile.TemporaryDirectory()
        self.root = Path(self.temporary.name)
        self.path = self.root / 'docs/ia/PLANO-25D-ESTADO.json'
        self.path.parent.mkdir(parents=True)
        self.initial = json.loads((ROOT / 'docs/ia/PLANO-25D-ESTADO.json').read_text(encoding='utf-8'))
        self.path.write_text(json.dumps(self.initial), encoding='utf-8')
        self.artifact = self.root / 'Evidence/report.md'
        self.artifact.parent.mkdir()
        self.artifact.write_text('Actual evidence fixture', encoding='utf-8')

    def tearDown(self):
        self.temporary.cleanup()

    def run_cli(self, *args):
        return subprocess.run([sys.executable, str(SCRIPT), '--state', str(self.path), *args], capture_output=True, text=True)

    def ok(self, *args):
        result = self.run_cli(*args)
        self.assertEqual(0, result.returncode, result.stdout + result.stderr)

    def fail(self, *args):
        before = self.path.read_bytes()
        result = self.run_cli(*args)
        self.assertNotEqual(0, result.returncode, result.stdout)
        self.assertEqual(before, self.path.read_bytes(), 'Rejected mutation must not alter canonical state')

    def checkpoint(self, task='T00', session='s1'):
        self.ok('checkpoint', task, '--session', session, '--done', 'done', '--next', 'next',
                '--validation', 'actual result', '--pending', 'none', '--files', 'Evidence/report.md', '--processes', 'none')

    def seed_accepted_through(self, target):
        state = json.loads(self.path.read_text())
        for task in state['tasks']:
            if task['id'] > target:
                break
            task.update(status='accepted', evidence=[{'path':'Evidence/report.md', 'sha256':hashlib.sha256(self.artifact.read_bytes()).hexdigest()}], acceptance={'reference':'fixture'})
        self.path.write_text(json.dumps(state))

    def test_dependency_and_duplicate_claim_rejected(self):
        self.fail('claim', 'T01', '--session', 's1', '--agent', 'test')
        self.ok('claim', 'T00', '--session', 's1', '--agent', 'test')
        self.fail('claim', 'T00', '--session', 's2', '--agent', 'other')

    def test_checkpoint_ownership_and_explicit_handover(self):
        self.ok('claim', 'T00', '--session', 's1', '--agent', 'test')
        self.fail('release', 'T00', '--session', 's2', '--reason', 'wrong owner')
        self.fail('handover', 'T00', '--session', 's2', '--agent', 'other', '--from-session', 'wrong', '--reason', 'checked')
        self.ok('handover', 'T00', '--session', 's2', '--agent', 'other', '--from-session', 's1', '--reason', 'checked')
        self.checkpoint(session='s2')

    def test_missing_checkpoint_and_evidence_rejected(self):
        self.ok('claim', 'T00', '--session', 's1', '--agent', 'test')
        self.fail('submit', 'T00', '--session', 's1', '--evidence', 'Evidence/report.md', '--validation', 'result')
        self.checkpoint()
        self.fail('submit', 'T00', '--session', 's1', '--evidence', 'missing.md', '--validation', 'result')
        self.fail('submit', 'T00', '--session', 's1', '--evidence', str(SCRIPT), '--validation', 'outside checkout')

    def test_edited_evidence_blocks_acceptance(self):
        self.ok('claim', 'T00', '--session', 's1', '--agent', 'test')
        self.checkpoint()
        self.ok('submit', 'T00', '--session', 's1', '--evidence', 'Evidence/report.md', '--validation', 'result')
        self.artifact.write_text('changed')
        self.fail('accept', 'T00', '--session', 'reviewer', '--reference', 'checked')
        self.fail('check')

    def test_human_gate_requires_explicit_declaration(self):
        self.seed_accepted_through('T00')
        self.ok('claim', 'T01', '--session', 's1', '--agent', 'test')
        self.checkpoint('T01')
        self.ok('submit', 'T01', '--session', 's1', '--evidence', 'Evidence/report.md', '--validation', 'result')
        self.fail('accept', 'T01', '--session', 'reviewer', '--reference', 'technical only')
        self.ok('accept', 'T01', '--session', 'reviewer', '--reference', 'synthetic test approval', '--human-approval')

    def test_parallel_disjoint_scopes_and_conflicting_resources(self):
        self.seed_accepted_through('T02')
        self.ok('claim', 'T03', '--session', 's1', '--agent', 'test')
        self.ok('claim', 'T04', '--session', 's2', '--agent', 'test')
        state = json.loads(self.path.read_text())
        state['tasks'][5]['resources'] = ['art-yuna']
        self.path.write_text(json.dumps(state))
        self.fail('claim', 'T05', '--session', 's3', '--agent', 'test')

    def test_two_processes_cannot_claim_same_task(self):
        command = [sys.executable, str(SCRIPT), '--state', str(self.path), 'claim', 'T00', '--agent', 'test', '--session']
        first = subprocess.Popen(command + ['one'], stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        second = subprocess.Popen(command + ['two'], stdout=subprocess.PIPE, stderr=subprocess.PIPE)
        first.communicate(timeout=15)
        second.communicate(timeout=15)
        self.assertEqual([0, 1], sorted([first.returncode, second.returncode]))
        self.ok('check')

    def test_reopen_invalidates_dependents_but_preserves_history(self):
        self.seed_accepted_through('T02')
        self.ok('claim', 'T03', '--session', 's1', '--agent', 'test')
        self.fail('reopen', 'T01', '--session', 'reviewer', '--reason', 'new issue')
        self.ok('release', 'T03', '--session', 's1', '--reason', 'coordinated')
        self.ok('reopen', 'T01', '--session', 'reviewer', '--reason', 'new issue')
        state = json.loads(self.path.read_text())
        self.assertEqual('accepted', state['tasks'][0]['status'])
        self.assertEqual('pending', state['tasks'][1]['status'])
        self.assertEqual('pending', state['tasks'][2]['status'])
        self.assertEqual(3, len(state['history']))

    def test_failed_atomic_replace_preserves_original(self):
        before = self.path.read_bytes()
        with patch.object(coordinator.os, 'replace', side_effect=OSError('interrupted write')):
            with self.assertRaises(OSError):
                coordinator.save(self.path, self.initial)
        self.assertEqual(before, self.path.read_bytes())
        self.assertEqual([], list(self.path.parent.glob('.plano25d-*.tmp')))


if __name__ == '__main__':
    unittest.main()
