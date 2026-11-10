"""CLI do diario compartilhado de IA; requer somente Python 3."""

import argparse
import json
import sqlite3
from pathlib import Path

DATABASE = Path(__file__).resolve().parents[1] / "docs" / "ia" / "diario.sqlite3"


def nonempty(value):
    if not value.strip():
        raise argparse.ArgumentTypeError("O valor nao pode ser vazio.")
    return value


def connect(path):
    db = sqlite3.connect(path.resolve().as_uri() + "?mode=rw", uri=True, timeout=30)
    db.row_factory = sqlite3.Row
    db.execute("PRAGMA foreign_keys = ON")
    return db


def main(argv=None):
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--db", type=Path, default=DATABASE)
    commands = parser.add_subparsers(dest="command", required=True)
    add = commands.add_parser("add", help="Acrescentar um passo, sem alterar o historico")
    add.add_argument("--session", required=True, type=nonempty)
    add.add_argument("--agent", required=True, type=nonempty)
    add.add_argument("--phase", required=True,
                     choices=["planned", "completed", "failed", "blocked", "info"])
    add.add_argument("--action", required=True, type=nonempty)
    for field in ("details", "files", "verification"):
        add.add_argument("--" + field, default="")
    add.add_argument("--plan-id", type=int)
    listing = commands.add_parser("list", help="Ler historico em JSON, em ordem cronologica")
    listing.add_argument("--session")
    listing.add_argument("--limit", type=int, default=50)
    commands.add_parser("check", help="Verificar integridade e referencias")
    args = parser.parse_args(argv)
    if args.command == "list" and args.limit < 1:
        parser.error("--limit deve ser positivo")
    try:
        with connect(args.db) as db:
            if args.command == "add":
                values = [getattr(args, key) for key in (
                    "session", "agent", "phase", "action", "details", "files",
                    "verification", "plan_id")]
                cursor = db.execute(
                    "INSERT INTO entries (session, agent, phase, action, details, files, "
                    "verification, plan_id) VALUES (?, ?, ?, ?, ?, ?, ?, ?)", values)
                print(json.dumps({"id": cursor.lastrowid}))
            elif args.command == "list":
                condition = "WHERE session = ?" if args.session else ""
                params = [args.session] if args.session else []
                rows = db.execute(
                    "SELECT * FROM (SELECT * FROM entries " + condition
                    + " ORDER BY id DESC LIMIT ?) ORDER BY id", params + [args.limit])
                print(json.dumps([dict(row) for row in rows], ensure_ascii=False, indent=2))
            else:
                integrity = [row[0] for row in db.execute("PRAGMA integrity_check")]
                foreign_keys = [tuple(row) for row in db.execute("PRAGMA foreign_key_check")]
                print(json.dumps({"integrity": integrity, "foreign_key_errors": foreign_keys}))
                if integrity != ["ok"] or foreign_keys:
                    return 1
    except (sqlite3.Error, OSError) as error:
        parser.exit(1, f"Erro no diario: {error}\n")
    finally:
        if "db" in locals():
            db.close()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
