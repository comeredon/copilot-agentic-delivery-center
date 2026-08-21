#!/usr/bin/env bash
# subagentStop: capture a subagent's terminal verdict so the gate ledger can cite it.
#
# This is what makes "a different agent verified it" checkable after the fact rather than asserted
# in prose.
set -euo pipefail
# shellcheck source=./common.sh
. "$(dirname "${BASH_SOURCE[0]}")/common.sh"

PAYLOAD="$(read_payload)"
AGENT="$(payload_value "${PAYLOAD}" agent agentName subagent name)"
MODEL="$(payload_value "${PAYLOAD}" model modelId)"
VERDICT="$(payload_value "${PAYLOAD}" verdict status outcome result)"

write_audit "subagent-stop" "${AGENT}" "${VERDICT} (${MODEL})"
exit 0
