# Backups

Backups go to `~/PokeHubBackups`, outside Docker and outside the repo, so
`docker compose down -v` can't remove them. Set `POKEHUB_BACKUP_DIR` to use another folder.

They hold password hashes, client secrets, saves and the ROM: keep them private.

| What | When | Where |
|---|---|---|
| Database | Daily, by the `pokehub.backup` service, while the stack is up | `db/pokehub-*.dump` |
| Database and Keycloak | When you run `backup.sh` | `db/`, `identity/keycloak-*.tar.gz` |

The last 14 of each are kept. `BACKUP_INTERVAL_HOURS` and `BACKUP_KEEP` change that.

Keycloak isn't in the daily backup: its dev database is locked while it runs, so copying it
means stopping Keycloak for a few seconds. Run `backup.sh` after creating accounts or
changing the realm.

## Back up now

```bash
tools/ops/backup.sh
```

`--db-only` skips Keycloak and leaves it running. A full backup restarts Keycloak, which can
sign you out of the app and the FireRed connection, so sign in again afterwards.

## Check a backup

Restores into a scratch database, compares its rows with the live one and drops it.

```bash
tools/ops/restore.sh check ~/PokeHubBackups/db/pokehub-YYYYMMDD-HHMMSS.dump
```

## Restore

Both ask you to type `RESTORE`, and both keep what they replace.

```bash
tools/ops/restore.sh db ~/PokeHubBackups/db/pokehub-YYYYMMDD-HHMMSS.dump
```

```bash
tools/ops/restore.sh identity ~/PokeHubBackups/identity/keycloak-YYYYMMDD-HHMMSS.tar.gz
```

- **Database:** takes a fresh backup, loads the dump on the side, then swaps it in. The old
  database stays as `pokehub_before_<timestamp>` until you drop it.
- **Keycloak:** the old files stay in `.containers/identity/h2.before_<timestamp>`.

Restore both from around the same time, since users in the database point at Keycloak
accounts.
