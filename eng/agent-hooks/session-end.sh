#!/usr/bin/env bash
# sessionEnd: emit a summary of the session's tool use, denials, and errors.
#
# The summary is derived entirely from the day's audit trail, so it reflects what actually happened
# rather than what the agent reported.
set -euo pipefail
# shellcheck source=./common.sh
. "$(dirname "${BASH_SOURCE[0]}")/common.sh"

PAYLOAD="$(read_payload)"
SESSION_ID="$(payload_value "${PAYLOAD}" sessionId session id)"

TODAY="$(date -u +%Y%m%d)"
TRAIL="${AUDIT_DIR}/hooks-${TODAY}.jsonl"
SUMMARY="${AUDIT_DIR}/session-summary-${TODAY}.json"

mkdir -p "${AUDIT_DIR}" 2>/dev/null || true

count_event() {
  local event="$1"
  if [ -f "${TRAIL}" ] && have_jq; then
    jq -s --arg e "${event}" '[.[] | select(.event == $e)] | length' "${TRAIL}" 2>/dev/null || echo 0
  else
    echo 0
  fi
}

if have_jq && [ -f "${TRAIL}" ]; then
  # Denials are the interesting part of the trail: they show where policy actually bit.
  DENIALS="$(jq -s '[.[] | select(.event == "deny") | {tool, reason}]' "${TRAIL}" 2>/dev/null || echo '[]')"
else
  DENIALS='[]'
fi

if have_jq; then
  jq -n \
    --arg session "${SESSION_ID}" \
    --arg generated "$(date -u +%Y-%m-%dT%H:%M:%SZ)" \
    --argjson toolUse "$(count_event tool-use)" \
    --argjson allowed "$(count_event allow)" \
    --argjson denied "$(count_event deny)" \
    --argjson errors "$(count_event error)" \
    --argjson subagentStops "$(count_event subagent-stop)" \
    --argjson denials "${DENIALS}" \
    '{
       schemaVersion: "1.0.0",
       sessionId: $session,
       generatedAtUtc: $generated,
       counts: { toolUse: $toolUse, allowed: $allowed, denied: $denied, errors: $errors, subagentStops: $subagentStops },
       denials: $denials
     }' | tee "${SUMMARY}"
else
  printf '{"schemaVersion":"1.0.0","sessionId":"%s","note":"jq unavailable; summary not computed"}\n' "${SESSION_ID}"
fi

exit 0
