#!/usr/bin/env bash
# postToolUse: append a sanitized record of the invocation to the local audit trail.
set -euo pipefail
# shellcheck source=./common.sh
. "$(dirname "${BASH_SOURCE[0]}")/common.sh"

PAYLOAD="$(read_payload)"
TOOL="$(payload_value "${PAYLOAD}" tool toolName name)"
STATUS="$(payload_value "${PAYLOAD}" status outcome result)"

# Only the shape of what happened is recorded. Arguments and outputs may contain untrusted content
# or credentials, so they never reach the trail.
write_audit "tool-use" "${TOOL}" "${STATUS}"
exit 0
