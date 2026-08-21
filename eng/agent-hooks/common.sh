#!/usr/bin/env bash
# Shared helpers for the agent hooks. Sourced by every hook script.
#
# Everything here is defensive: hook payloads arrive from an agent runtime whose exact shape can
# change, so nothing assumes a field exists and nothing aborts on a surprise.

set -euo pipefail

REPO_ROOT="$(cd "$(dirname "${BASH_SOURCE[0]}")/../.." && pwd)"
AUDIT_DIR="${REPO_ROOT}/.agent-runtime"
POLICY_FILE="${REPO_ROOT}/.github/hooks/guardrails.json"

have_jq() {
  command -v jq >/dev/null 2>&1
}

# Reads stdin fully. Returns an empty object when there is nothing to read.
read_payload() {
  local raw
  raw="$(cat || true)"
  if [ -z "${raw}" ]; then
    printf '{}'
  else
    printf '%s' "${raw}"
  fi
}

# Extracts the first present value from a list of candidate JSON keys.
# Falls back to an empty string when jq is unavailable or the key is absent.
payload_value() {
  local payload="$1"
  shift

  if ! have_jq; then
    printf ''
    return 0
  fi

  local key value
  for key in "$@"; do
    value="$(printf '%s' "${payload}" \
      | jq -r --arg k "${key}" '
          (.[$k] // empty)
          | if type == "array" then map(tostring) | join(" ")
            elif type == "object" then tojson
            else tostring end
        ' 2>/dev/null || true)"

    if [ -n "${value}" ] && [ "${value}" != "null" ]; then
      printf '%s' "${value}"
      return 0
    fi
  done

  printf ''
}

# Emits the committed policy patterns, one per line.
# $1 is either deniedCommandPatterns or deniedPathPatterns.
policy_patterns() {
  local list="$1"

  if ! have_jq || [ ! -f "${POLICY_FILE}" ]; then
    return 0
  fi

  jq -r --arg list "${list}" '.policy[$list][]? // empty' "${POLICY_FILE}" 2>/dev/null || true
}

write_decision() {
  local decision="$1"
  local reason="${2:-}"

  if [ -z "${reason}" ]; then
    printf '{"decision":"%s"}\n' "${decision}"
  elif have_jq; then
    jq -nc --arg d "${decision}" --arg r "${reason}" '{decision:$d, reason:$r}'
  else
    # Minimal escaping fallback so the output is still valid JSON without jq.
    local escaped
    escaped="$(printf '%s' "${reason}" | tr '\n' ' ' | sed 's/\\/\\\\/g; s/"/\\"/g')"
    printf '{"decision":"%s","reason":"%s"}\n' "${decision}" "${escaped}"
  fi
}

# Appends one JSON line to the git-ignored local audit trail.
# Only the tool name, the decision, and the matched pattern name are recorded — never a payload,
# never a matched path (which could itself be a credential location).
write_audit() {
  local event="$1"
  local tool="${2:-}"
  local reason="${3:-}"

  {
    mkdir -p "${AUDIT_DIR}" 2>/dev/null || return 0
    local stamp file
    stamp="$(date -u +%Y-%m-%dT%H:%M:%SZ)"
    file="${AUDIT_DIR}/hooks-$(date -u +%Y%m%d).jsonl"

    if have_jq; then
      jq -nc \
        --arg t "${stamp}" --arg e "${event}" --arg tool "${tool}" --arg r "${reason}" \
        '{timestamp:$t, event:$e, tool:$tool, reason:$r}' >>"${file}"
    else
      printf '{"timestamp":"%s","event":"%s","tool":"%s","reason":"%s"}\n' \
        "${stamp}" "${event}" "${tool}" "${reason}" >>"${file}"
    fi
  } 2>/dev/null || true

  # Auditing must never break a session. A lost line beats a blocked agent.
  return 0
}
