#!/usr/bin/env bash
# preToolUse guardrail.
#
# Denies merges, pushes to main, destructive commands, credential reads, and edits to the guardrail
# files themselves. Fails closed: if the payload cannot be understood, it is not approved.
#
# This is a deterrent and an audit trail. Branch protection, required checks, least-privilege
# tokens, and the human merge signal remain the authoritative controls.

set -euo pipefail

# shellcheck source=./common.sh
. "$(dirname "${BASH_SOURCE[0]}")/common.sh"

PAYLOAD="$(read_payload)"

if ! printf '%s' "${PAYLOAD}" | jq empty >/dev/null 2>&1; then
  if have_jq; then
    write_decision "deny" "The hook payload could not be parsed. Failing closed."
    exit 0
  fi
fi

TOOL="$(payload_value "${PAYLOAD}" tool toolName name)"
COMMAND="$(payload_value "${PAYLOAD}" command commandLine input arguments)"
TARGET_PATH="$(payload_value "${PAYLOAD}" path filePath file target)"

# 1. Command patterns: merge, force push, destructive filesystem, piped remote execution.
if [ -n "${COMMAND}" ]; then
  while IFS= read -r pattern; do
    [ -z "${pattern}" ] && continue
    if printf '%s' "${COMMAND}" | grep -Eq "${pattern}"; then
      write_audit "deny" "${TOOL}" "command pattern: ${pattern}"
      write_decision "deny" "This command is blocked by repository policy (matched: ${pattern}). Agents may not merge, push to main, force push, bypass verification, delete recursively, or pipe remote content into a shell. A pull request at a passing quality gate is the end of agent authority; a human authorises the merge. If the policy is wrong, open an issue - do not route around the hook."
      exit 0
    fi
  done < <(policy_patterns deniedCommandPatterns)
fi

# 2. Path patterns: guardrail files and anything credential-shaped.
for candidate in "${TARGET_PATH}" "${COMMAND}"; do
  [ -z "${candidate}" ] && continue
  normalized="${candidate//\\//}"

  while IFS= read -r pattern; do
    [ -z "${pattern}" ] && continue
    if printf '%s' "${normalized}" | grep -Eq "${pattern}"; then
      # Deliberately do not echo the matched text; it may be a credential path.
      write_audit "deny" "${TOOL}" "path pattern: ${pattern}"
      write_decision "deny" "This path is blocked by repository policy (matched: ${pattern}). Guardrail files (.github/hooks, .github/workflows, .mcp.json, CODEOWNERS) are not agent-editable, and credential files are not agent-readable. If a guardrail needs to change, open an issue. If you need a secret to proceed, stop and say so."
      exit 0
    fi
  done < <(policy_patterns deniedPathPatterns)
done

write_audit "allow" "${TOOL}" "no policy match"
write_decision "allow"
exit 0
