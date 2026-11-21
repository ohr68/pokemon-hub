#!/usr/bin/env bash
# Backs up the database and Keycloak now. Usage: tools/ops/backup.sh [--db-only]
#
# Keycloak's dev database (H2) is locked while it runs, so Keycloak stops for a few seconds
# while its files are copied. --db-only leaves it running.
set -euo pipefail
umask 077

cd "$(dirname "$0")/../.."
backups="${POKEHUB_BACKUP_DIR:-$HOME/PokeHubBackups}"
keep="${BACKUP_KEEP:-14}"
mkdir -p "$backups/db" "$backups/identity"

docker compose up -d --wait pokehub.database > /dev/null 2>&1 || docker compose up -d pokehub.database
until docker exec PokeHub.Database pg_isready -U postgres -q; do sleep 1; done
docker compose run --rm --no-deps -T --entrypoint sh pokehub.backup /ops/backup-db.sh

[[ "${1:-}" == "--db-only" ]] && exit 0

was_running=$(docker inspect -f '{{.State.Running}}' PokeHub.Identity 2>/dev/null || echo false)
final="$backups/identity/keycloak-$(date -u +%Y%m%d-%H%M%S).tar.gz"
tmp="$final.tmp"
cleanup() {
  rm -f "$tmp"
  [[ "$was_running" == "true" ]] && docker start PokeHub.Identity > /dev/null
}
trap cleanup EXIT

[[ "$was_running" == "true" ]] && docker stop PokeHub.Identity > /dev/null
tar -czf "$tmp" -C .containers/identity --exclude='*.trace.db' h2
tar -tzf "$tmp" | grep -q 'keycloakdb.mv.db'
mv "$tmp" "$final"

ls -1t "$backups"/identity/keycloak-*.tar.gz | tail -n +"$((keep + 1))" | while read -r old; do rm -f "$old"; done
docker compose run --rm --no-deps -T --entrypoint sh pokehub.backup \
  -c 'umask 022; date -u +%Y-%m-%dT%H:%M:%SZ > /status/identity' > /dev/null 2>&1
echo "Keycloak backup: $(basename "$final") ($(du -h "$final" | cut -f1))"
echo "Saved in $backups"
