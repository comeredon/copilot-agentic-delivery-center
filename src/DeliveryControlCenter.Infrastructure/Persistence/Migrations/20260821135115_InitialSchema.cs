using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DeliveryControlCenter.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class InitialSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "agent_evidence",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    WorkItemId = table.Column<Guid>(type: "uuid", nullable: false),
                    Source = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Severity = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Summary = table.Column<string>(type: "character varying(4096)", maxLength: 4096, nullable: false),
                    Model = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_agent_evidence", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "audit_events",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Actor = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Action = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Target = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    Outcome = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Denied = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_audit_events", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gate_ledger_entries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Sequence = table.Column<long>(type: "bigint", nullable: false),
                    GateId = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    EvidenceHash = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    PreviousEntryHash = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    EntryHash = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    proposer_agent = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    proposer_model = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    proposer_verdict = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    verifier_agent = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    verifier_model = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    verifier_verdict = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gate_ledger_entries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gate_reports",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Commit = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    Profile = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CanonicalHash = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                    SchemaVersion = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Verdict = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    WorkItemId = table.Column<Guid>(type: "uuid", nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gate_reports", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "repository_connections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Owner = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProjectNumber = table.Column<int>(type: "integer", nullable: true),
                    ProjectWritesEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_repository_connections", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "sync_cursors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Feed = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    LastSyncedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    ETag = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConsecutiveFailures = table.Column<int>(type: "integer", nullable: false),
                    LastError = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: true),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_sync_cursors", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "work_items",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Number = table.Column<int>(type: "integer", nullable: false),
                    Kind = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Title = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    Url = table.Column<string>(type: "character varying(512)", maxLength: 512, nullable: false),
                    State = table.Column<string>(type: "character varying(32)", maxLength: 32, nullable: false),
                    Stage = table.Column<string>(type: "character varying(48)", maxLength: 48, nullable: false),
                    Risk = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    Workstream = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    LatestGateHash = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: true),
                    HumanAuthorized = table.Column<bool>(type: "boolean", nullable: false),
                    Version = table.Column<long>(type: "bigint", nullable: false),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    UpdatedAtUtc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_items", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "gate_check_results",
                columns: table => new
                {
                    GateReportId = table.Column<Guid>(type: "uuid", nullable: false),
                    Ordinal = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Passed = table.Column<bool>(type: "boolean", nullable: false),
                    Detail = table.Column<string>(type: "character varying(2048)", maxLength: 2048, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_gate_check_results", x => new { x.GateReportId, x.Ordinal });
                    table.ForeignKey(
                        name: "FK_gate_check_results_gate_reports_GateReportId",
                        column: x => x.GateReportId,
                        principalTable: "gate_reports",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_agent_evidence_WorkItemId",
                table: "agent_evidence",
                column: "WorkItemId");

            migrationBuilder.CreateIndex(
                name: "IX_audit_events_CreatedAtUtc",
                table: "audit_events",
                column: "CreatedAtUtc");

            migrationBuilder.CreateIndex(
                name: "IX_gate_ledger_entries_Sequence",
                table: "gate_ledger_entries",
                column: "Sequence",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_gate_reports_WorkItemId_CreatedAtUtc",
                table: "gate_reports",
                columns: new[] { "WorkItemId", "CreatedAtUtc" });

            migrationBuilder.CreateIndex(
                name: "IX_repository_connections_Owner_Name",
                table: "repository_connections",
                columns: new[] { "Owner", "Name" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_sync_cursors_Feed",
                table: "sync_cursors",
                column: "Feed",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_items_Number",
                table: "work_items",
                column: "Number",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "agent_evidence");

            migrationBuilder.DropTable(
                name: "audit_events");

            migrationBuilder.DropTable(
                name: "gate_check_results");

            migrationBuilder.DropTable(
                name: "gate_ledger_entries");

            migrationBuilder.DropTable(
                name: "repository_connections");

            migrationBuilder.DropTable(
                name: "sync_cursors");

            migrationBuilder.DropTable(
                name: "work_items");

            migrationBuilder.DropTable(
                name: "gate_reports");
        }
    }
}
