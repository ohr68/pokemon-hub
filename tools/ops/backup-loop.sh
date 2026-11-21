#!/bin/sh
# Keeps the database backups fresh. It checks every hour instead of sleeping a full interval,
# so a computer that was off or asleep catches up soon after it comes back.
set -u

hours="${BACKUP_INTERVAL_HOURS:-24}"

while true; do
  recent=$(find /backups/db -name 'pokehub-*.dump' -mmin "-$((hours * 60))" 2>/dev/null | head -n 1)
  if [ -z "$recent" ]; then
    until pg_isready -h pokehub.database -U postgres -q; do sleep 5; done
    /bin/sh /ops/backup-db.sh || echo "Database backup FAILED" >&2
  fi
  sleep 3600
done
