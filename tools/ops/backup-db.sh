#!/bin/sh
# Dumps the PokeHub database into /backups/db. Runs inside the pokehub.backup container.
# A dump only gets its final name after pg_restore has read it back.
set -eu
umask 077

keep="${BACKUP_KEEP:-14}"
dir=/backups/db
mkdir -p "$dir"

final="$dir/pokehub-$(date -u +%Y%m%d-%H%M%S).dump"
tmp="$final.tmp"
trap 'rm -f "$tmp"' EXIT

pg_dump -h pokehub.database -U postgres -d pokehub -Fc -f "$tmp"
pg_restore --list "$tmp" > /dev/null
mv "$tmp" "$final"

ls -1t "$dir"/pokehub-*.dump | tail -n +"$((keep + 1))" | while read -r old; do rm -f "$old"; done

echo "Database backup: $(basename "$final") ($(du -h "$final" | cut -f1))"
