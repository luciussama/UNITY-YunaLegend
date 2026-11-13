"""Contract and process-concurrency tests, using disposable SQLite files."""
import json
from pathlib import Path
import sqlite3
import subprocess
import sys
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[1]
sys.path.insert(0, str(ROOT / 'tools'))
import agent_registry as registry
import plan_25d


class RegistryTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.path = Path(self.temp.name) / 'registry.sqlite3'
        with self.connect() as db:
            registry.initialize(db)

    def connect(self):
        db = sqlite3.connect(self.path, timeout=10)
        db.row_factory = sqlite3.Row
        db.execute('PRAGMA foreign_keys=ON')
        self.addCleanup(db.close)
        return db

    def claim(self, subject='test/topic', session='owner', resources=None):
        with self.connect() as db:
            return registry.claim(db, subject, session, 'test', resources or ['test/file'], 'test')

    def change(self, identifier, state, session='owner', outcome=''):
        with self.connect() as db:
            return registry.transition(db, identifier, session, state, 'evidence', outcome)

    def test_idempotency_ownership_and_terminal_subject(self):
        first = self.claim()
        self.assertEqual(first['id'], self.claim()['id'])
        with self.assertRaises(ValueError):
            self.claim(session='other')
        with self.assertRaises(ValueError):
            self.change(first['id'], 'em_execucao', session='other')
        with self.assertRaises(sqlite3.IntegrityError):
            self.change(first['id'], 'finalizado', outcome='concluido')
        self.change(first['id'], 'em_execucao')
        self.assertTrue(self.change(first['id'], 'em_execucao')['reused'])
        self.change(first['id'], 'finalizado', outcome='concluido')
        self.assertTrue(self.change(first['id'], 'finalizado', outcome='concluido')['reused'])
        self.assertTrue(self.claim()['reused'])
        with self.assertRaises(ValueError):
            self.claim(session='other')
        self.claim(subject='test/new-request', session='other')  # Resource released.

    def test_resource_aliases_hierarchy_and_immutability(self):
        first = self.claim(resources=['Test\\Folder/'])
        with self.assertRaises(ValueError):
            self.claim(subject='test/different', resources=['test/folder/child'])
        with self.connect() as db:
            for table in ('agent_jobs', 'agent_resources', 'agent_events'):
                with self.assertRaises(sqlite3.IntegrityError):
                    db.execute(f'DELETE FROM {table}')
            with self.assertRaises(sqlite3.IntegrityError):
                db.execute('UPDATE agent_jobs SET session=? WHERE id=?', ('other', first['id']))

    def test_legacy_reservation_blocks_registry(self):
        # Independent of current real task statuses: use a temporary coordinator state.
        from unittest.mock import patch
        state_path = Path(self.temp.name) / 'state.json'
        state_path.write_text(json.dumps({'tasks': [{'id': 'T99', 'status': 'active',
            'owner': {'session': 'legacy'}, 'resources': ['test/file']}]}))
        with patch.object(plan_25d, 'STATE', state_path):
            with self.assertRaisesRegex(ValueError, 'Coordenador ocupado'):
                self.claim()

    def test_registry_blocks_coordinator(self):
        self.claim()
        root = Path(self.temp.name)
        destination = root / 'docs/ia'
        destination.mkdir(parents=True)
        import shutil
        shutil.copy2(self.path, destination / 'diario.sqlite3')
        with self.assertRaisesRegex(ValueError, 'Registro de agentes ocupado'):
            plan_25d.check_agent_registry(root, {'id': 'T99', 'resources': ['test/file']}, 'other')
        plan_25d.check_agent_registry(root, {'id': 'T99', 'resources': ['test/file']}, 'owner')

    def test_different_subjects_can_run_together(self):
        first = self.claim(subject='test/art', resources=['test/art'])
        second = self.claim(subject='test/audio', session='other', resources=['test/audio'])
        self.change(first['id'], 'em_execucao')
        self.change(second['id'], 'em_execucao', session='other')
        with self.connect() as db:
            self.assertEqual([j['state'] for j in registry.jobs(db)], ['em_execucao', 'em_execucao'])

    def test_simultaneous_processes_only_one_wins(self):
        def command(index):
            return [sys.executable, str(ROOT / 'tools/agent_registry.py'), '--db', str(self.path),
                    'claim', '--subject', 'race/subject', '--session', f'worker-{index}',
                    '--agent', 'test', '--resource', 'race/file', '--details', 'race']
        processes = [subprocess.Popen(command(i), stdout=subprocess.PIPE, stderr=subprocess.PIPE) for i in range(6)]
        results = [p.communicate(timeout=30) for p in processes]
        self.assertEqual(sum(p.returncode == 0 for p in processes), 1, results)
        with self.connect() as db:
            self.assertEqual(len(registry.jobs(db)), 1)
            self.assertEqual(db.execute('SELECT count(*) FROM agent_events').fetchone()[0], 1)


if __name__ == '__main__':
    unittest.main()
