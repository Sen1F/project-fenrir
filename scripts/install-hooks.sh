#!/bin/sh
# Install local git hooks for Project Fenrir.
# Run once after cloning: sh scripts/install-hooks.sh

set -e

HOOKS_DIR="$(git rev-parse --show-toplevel)/.git/hooks"
SCRIPTS_DIR="$(git rev-parse --show-toplevel)/scripts/hooks"

for hook in pre-commit pre-push commit-msg; do
  cp "$SCRIPTS_DIR/$hook" "$HOOKS_DIR/$hook"
  chmod +x "$HOOKS_DIR/$hook"
  echo "✓ Installed $hook"
done

echo ""
echo "All hooks installed. See scripts/hooks/ for details."
