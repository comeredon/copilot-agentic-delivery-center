#!/usr/bin/env bash
# errorOccurred: record the error class and the tool that raised it.
#
# Deliberately records no payload. An error message can contain a token, a connection string, or
# untrusted remote content, and the audit trail must be safe to read.
set -euo pipefail
# shellcheck source=./common.sh
. "$(dirname "${BASH_SOURCE[0]}")/common.sh"

PAYLOAD="$(read_payload)"
TOOL="$(payload_value "${PAYLOAD}" tool toolName name)"
ERROR_TYPE="$(payload_value "${PAYLOAD}" errorType type class code)"
[ -z "${ERROR_TYPE}" ] && ERROR_TYPE="unclassified"

write_audit "error" "${TOOL}" "${ERROR_TYPE}"
exit 0
