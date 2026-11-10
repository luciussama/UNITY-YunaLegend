import contextlib
import importlib.util
import io
import json
from pathlib import Path
import shutil
import sqlite3
import tempfile
import unittest

ROOT = Path(__file__).resolve().parents[1]
SPEC = importlib.util.spec_from_file_location("ai_diary", ROOT / "tools" / "ai_diary.py")
diary = importlib.util.module_from_spec(SPEC)
SPEC.loader.exec_module(diary)


class DiaryTests(unittest.TestCase):
    def setUp(self):
        self.temp = tempfile.TemporaryDirectory()
        self.addCleanup(self.temp.cleanup)
        self.db = Path(self.temp.name) / "copy.sqlite3"
        shutil.copy2(diary.DATABASE, self.db)

    def call(self, *args):
        output = io.StringIO()
        with contextlib.redirect_stdout(output):
            result = diary.main(["--db", str(self.db), *args])
        self.assertEqual(result, 0)
        return json.loads(output.getvalue())

    def add(self, phase="planned", **extra):
        args = ["add", "--session", "test-session", "--agent", "unittest",
                "--phase", phase, "--action", "Validar diário — ação"]
        for key, value in extra.items():
            args.extend(["--" + key.replace("_", "-"), str(value)])
        return self.call(*args)["id"]

    def test_plan_result_unicode_and_filter(self):
        plan = self.add()
        result = self.add("completed", plan_id=plan, verification="Teste aprovado")
        rows = self.call("list", "--session", "test-session", "--limit", "2")
        self.assertEqual([row["id"] for row in rows], [plan, result])
        self.assertEqual(rows[1]["plan_id"], plan)
        self.assertEqual(rows[0]["action"], "Validar diário — ação")
        self.assertTrue(rows[0]["created_at"].endswith("Z"))
        self.assertEqual(self.call("list", "--session", "test-session", "--limit", "1")[0]["id"], result)

    def test_reject_invalid_results(self):
        for extra in ({}, {"plan_id": 999999}):
            with self.subTest(extra=extra), contextlib.redirect_stderr(io.StringIO()):
                with self.assertRaises(SystemExit):
                    self.add("completed", **extra)
        plan = self.add()
        with contextlib.closing(diary.connect(self.db)) as db:
            with self.assertRaises(sqlite3.IntegrityError):
                db.execute("INSERT INTO entries(session, agent, phase, action, plan_id) "
                           "VALUES ('other', 'test', 'completed', 'test', ?)", (plan,))
            with self.assertRaises(sqlite3.IntegrityError):
                db.execute("INSERT INTO entries(session, agent, phase, action) "
                           "VALUES ('test', 'test', 'invalid', 'test')")

    def test_append_only(self):
        plan = self.add()
        with contextlib.closing(diary.connect(self.db)) as db:
            for sql in ("UPDATE entries SET action = 'changed' WHERE id = ?",
                        "DELETE FROM entries WHERE id = ?"):
                with self.subTest(sql=sql), self.assertRaises(sqlite3.IntegrityError):
                    db.execute(sql, (plan,))

    def test_integrity(self):
        self.assertEqual(self.call("check"), {"integrity": ["ok"], "foreign_key_errors": []})

    def test_missing_database_not_created(self):
        path = Path(self.temp.name) / "missing.sqlite3"
        with contextlib.redirect_stderr(io.StringIO()), self.assertRaises(SystemExit):
            diary.main(["--db", str(path), "check"])
        self.assertFalse(path.exists())


if __name__ == "__main__":
    unittest.main()
