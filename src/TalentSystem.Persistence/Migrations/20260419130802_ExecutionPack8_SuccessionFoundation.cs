using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExecutionPack8_SuccessionFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CriticalPositions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriticalityLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    RiskLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecordStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CriticalPositions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CriticalPositions_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SuccessionPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CriticalPositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformanceCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecordStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuccessionPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuccessionPlans_CriticalPositions_CriticalPositionId",
                        column: x => x.CriticalPositionId,
                        principalTable: "CriticalPositions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SuccessionPlans_PerformanceCycles_PerformanceCycleId",
                        column: x => x.PerformanceCycleId,
                        principalTable: "PerformanceCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SuccessionCoverageSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SuccessionPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TotalCandidates = table.Column<int>(type: "int", nullable: false),
                    HasReadyNow = table.Column<bool>(type: "bit", nullable: false),
                    HasPrimarySuccessor = table.Column<bool>(type: "bit", nullable: false),
                    CoverageScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CalculatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecordStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuccessionCoverageSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuccessionCoverageSnapshots_SuccessionPlans_SuccessionPlanId",
                        column: x => x.SuccessionPlanId,
                        principalTable: "SuccessionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SuccessorCandidates",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SuccessionPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ReadinessLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    RankOrder = table.Column<int>(type: "int", nullable: false),
                    IsPrimarySuccessor = table.Column<bool>(type: "bit", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CreatedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ModifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    ModifiedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecordStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    DeletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    DeletedByUserId = table.Column<Guid>(type: "uniqueidentifier", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuccessorCandidates", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SuccessorCandidates_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SuccessorCandidates_SuccessionPlans_SuccessionPlanId",
                        column: x => x.SuccessionPlanId,
                        principalTable: "SuccessionPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CriticalPositions_SingleActivePerPosition",
                table: "CriticalPositions",
                column: "PositionId",
                unique: true,
                filter: "[RecordStatus] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_SuccessionCoverageSnapshots_OnePerPlan",
                table: "SuccessionCoverageSnapshots",
                column: "SuccessionPlanId",
                unique: true,
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_SuccessionPlans_CriticalPositionId_PerformanceCycleId",
                table: "SuccessionPlans",
                columns: new[] { "CriticalPositionId", "PerformanceCycleId" },
                unique: true,
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_SuccessionPlans_PerformanceCycleId",
                table: "SuccessionPlans",
                column: "PerformanceCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_SuccessionPlans_Status",
                table: "SuccessionPlans",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_SuccessorCandidates_EmployeeId",
                table: "SuccessorCandidates",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_SuccessorCandidates_RankOrder",
                table: "SuccessorCandidates",
                column: "RankOrder");

            migrationBuilder.CreateIndex(
                name: "IX_SuccessorCandidates_ReadinessLevel",
                table: "SuccessorCandidates",
                column: "ReadinessLevel");

            migrationBuilder.CreateIndex(
                name: "IX_SuccessorCandidates_SinglePrimaryPerPlan",
                table: "SuccessorCandidates",
                column: "SuccessionPlanId",
                unique: true,
                filter: "[IsPrimarySuccessor] = 1 AND [RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_SuccessorCandidates_SuccessionPlanId_EmployeeId",
                table: "SuccessorCandidates",
                columns: new[] { "SuccessionPlanId", "EmployeeId" },
                unique: true,
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_SuccessorCandidates_SuccessionPlanId_RankOrder",
                table: "SuccessorCandidates",
                columns: new[] { "SuccessionPlanId", "RankOrder" },
                unique: true,
                filter: "[RecordStatus] <> 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SuccessionCoverageSnapshots");

            migrationBuilder.DropTable(
                name: "SuccessorCandidates");

            migrationBuilder.DropTable(
                name: "SuccessionPlans");

            migrationBuilder.DropTable(
                name: "CriticalPositions");
        }
    }
}
