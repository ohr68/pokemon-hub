#!/usr/bin/env bash
# Usage:
#   tools/ops/restore.sh check <file.dump>      restores into a scratch database and compares rows
#   tools/ops/restore.sh db <file.dump>         replaces the database (the old one is kept, renamed)
#   tools/ops/restore.sh identity <file.tar.gz> replaces Keycloak's data (the old one is kept, renamed)
set -euo pipefail

cd "$(dirname "$0")/../.."
command="${1:-}"
file="${2:-}"
target="${RESTORE_TARGET_DB:-pokehub}"

usage() { sed -n '2,5p' "$0" | sed 's/^# \{0,1\}//'; exit 1; }
[[ -n "$command" && -f "$file" ]] || usage

psql_on() { local db="$1"; shift; docker exec -i PokeHub.Database psql -U postgres -d "$db" -v ON_ERROR_STOP=1 -At "$@"; }

load_dump() {
  psql_on postgres -c "DROP DATABASE IF EXISTS \"$1\" WITH (FORCE)" -c "CREATE DATABASE \"$1\"" > /dev/null
  docker exec -i PokeHub.Database pg_restore -U postgres -d "$1" --no-owner --exit-on-error < "$file"
}

rows_per_schema() {
  psql_on "$1" -F ' ' <<'SQL'
SELECT schemaname, count(*),
       sum((xpath('/row/c/text()', query_to_xml(format('select count(*) c from %I.%I', schemaname, tablename), false, true, '')))[1]::text::bigint)
FROM pg_tables
WHERE schemaname NOT IN ('pg_catalog', 'information_schema')
GROUP BY 1 ORDER BY 1;
SQL
}

confirm() {
  echo "$1"
  read -r -p "Type RESTORE to continue: " answer
  [[ "$answer" == "RESTORE" ]] || { echo "Cancelled."; exit 1; }
}

stamp=$(date -u +%Y%m%d_%H%M%S)

case "$command" in
  check)
    scratch=pokehub_restore_check
    trap 'psql_on postgres -c "DROP DATABASE IF EXISTS $scratch WITH (FORCE)" > /dev/null' EXIT
    load_dump "$scratch"
    echo "schema tables rows (restored)"
    rows_per_schema "$scratch"
    echo "schema tables rows (live now)"
    rows_per_schema "$target"
    ;;

  db)
    confirm "This replaces the '$target' database with $(basename "$file")."
    restoring="${target}_restoring"
    previous="${target}_before_$stamp"

    # Loaded on the side first: a bad dump fails here, with the live database untouched.
    [[ "$target" == "pokehub" ]] && tools/ops/backup.sh --db-only
    load_dump "$restoring"

    if [[ "$target" == "pokehub" ]]; then
      trap 'docker compose start pokehub.api pokehub.backup > /dev/null 2>&1 || true' EXIT
      docker compose stop pokehub.api pokehub.backup > /dev/null 2>&1 || true
    fi

    psql_on postgres \
      -c "SELECT pg_terminate_backend(pid) FROM pg_stat_activity WHERE datname = '$target'" \
      -c "ALTER DATABASE \"$target\" RENAME TO \"$previous\"" \
      -c "ALTER DATABASE \"$restoring\" RENAME TO \"$target\"" > /dev/null

    echo "Restored. The previous database is kept as '$previous'."
    ;;

  identity)
    tar -tzf "$file" | grep -q 'keycloakdb.mv.db' || { echo "Not a Keycloak backup."; exit 1; }
    confirm "This replaces Keycloak's users and realm with $(basename "$file")."

    docker stop PokeHub.Identity > /dev/null
    trap 'docker start PokeHub.Identity > /dev/null' EXIT
    mv .containers/identity/h2 ".containers/identity/h2.before_$stamp"
    tar -xzf "$file" -C .containers/identity
    echo "Restored. The previous data is kept in .containers/identity/h2.before_$stamp."
    ;;

  *) usage ;;
esac
