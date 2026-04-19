using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExecutionPack6_ScoringFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ScoringPolicies",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    PerformanceWeight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PotentialWeight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    EffectiveFromUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_ScoringPolicies", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TalentScores",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformanceCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformanceScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PotentialScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    FinalScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PerformanceWeight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PotentialWeight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    CalculationVersion = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CalculatedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_TalentScores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalentScores_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TalentScores_PerformanceCycles_PerformanceCycleId",
                        column: x => x.PerformanceCycleId,
                        principalTable: "PerformanceCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScoringPolicies_SingleActivePolicy",
                table: "ScoringPolicies",
                column: "RecordStatus",
                unique: true,
                filter: "[RecordStatus] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ScoringPolicies_Version",
                table: "ScoringPolicies",
                column: "Version",
                unique: true,
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_TalentScores_EmployeeId_PerformanceCycleId",
                table: "TalentScores",
                columns: new[] { "EmployeeId", "PerformanceCycleId" },
                unique: true,
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_TalentScores_FinalScore",
                table: "TalentScores",
                column: "FinalScore");

            migrationBuilder.CreateIndex(
                name: "IX_TalentScores_PerformanceCycleId",
                table: "TalentScores",
                column: "PerformanceCycleId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScoringPolicies");

            migrationBuilder.DropTable(
                name: "TalentScores");
        }
    }
}
