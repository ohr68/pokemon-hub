#!/usr/bin/env bash
# Runs every local check on both repos and prints one summary. Changes no source files.
# Usage: tools/ops/verify.sh [--backend | --frontend]
#
# POKEMON_WEB_DIR points at the frontend (default: ~/FrontendProjects/pokemon-web).
set -uo pipefail

cd "$(dirname "$0")/../.."
web="${POKEMON_WEB_DIR:-$HOME/FrontendProjects/pokemon-web}"
scope="${1:-all}"

case "$scope" in
  all | --backend | --frontend) ;;
  *) sed -n '3p' "$0" | sed 's/^# //'; exit 2 ;;
esac

dotnet_bin=$(command -v dotnet || true)
[[ -x "$HOME/.dotnet/dotnet" ]] && dotnet_bin="$HOME/.dotnet/dotnet"

logs=$(mktemp -d)
trap 'rm -rf "$logs"' EXIT
results=()
failed=0

check() {
  local name="$1" dir="$2" log="$logs/${#results[@]}.log" start=$SECONDS
  shift 2
  printf '%-22s' "$name"

  if (cd "$dir" && "$@") > "$log" 2>&1; then
    echo "ok ($((SECONDS - start))s)"
    results+=("ok")
  else
    echo "FAILED ($((SECONDS - start))s)"
    results+=("failed")
    failed=$((failed + 1))
    echo "--- last lines of the output:"
    tail -n 15 "$log" | cut -c1-200
    echo "---"
  fi
}

if [[ "$scope" != "--frontend" ]]; then
  check "Backend build" . "$dotnet_bin" build PokeHub.slnx -c Release -v q
  check "Architecture tests" . "$dotnet_bin" test test/PokeHub.ArchitectureTests -c Release --no-build -v q
fi

if [[ "$scope" != "--backend" ]]; then
  if [[ -d "$web" ]]; then
    check "Frontend types" "$web" npx tsc -b
    # Not `pnpm lint`: that one rewrites files with --fix.
    check "Frontend lint" "$web" npx eslint src
    check "Frontend tests" "$web" npx vitest run
  else
    echo "Frontend not found at $web"
    results+=("failed")
    failed=$((failed + 1))
  fi
fi

echo
if [[ "$failed" -eq 0 ]]; then
  echo "All ${#results[@]} checks passed."
else
  echo "$failed of ${#results[@]} checks failed."
  exit 1
fi
