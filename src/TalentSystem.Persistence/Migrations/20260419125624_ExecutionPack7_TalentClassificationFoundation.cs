using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExecutionPack7_TalentClassificationFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ClassificationRuleSets",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Version = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    LowThreshold = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    HighThreshold = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_ClassificationRuleSets", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TalentClassifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformanceCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TalentScoreId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformanceBand = table.Column<byte>(type: "tinyint", nullable: false),
                    PotentialBand = table.Column<byte>(type: "tinyint", nullable: false),
                    NineBoxCode = table.Column<byte>(type: "tinyint", nullable: false),
                    CategoryName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    IsHighPotential = table.Column<bool>(type: "bit", nullable: false),
                    IsHighPerformer = table.Column<bool>(type: "bit", nullable: false),
                    ClassifiedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_TalentClassifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalentClassifications_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TalentClassifications_PerformanceCycles_PerformanceCycleId",
                        column: x => x.PerformanceCycleId,
                        principalTable: "PerformanceCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TalentClassifications_TalentScores_TalentScoreId",
                        column: x => x.TalentScoreId,
                        principalTable: "TalentScores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassificationRuleSets_SingleActiveRuleSet",
                table: "ClassificationRuleSets",
                column: "RecordStatus",
                unique: true,
                filter: "[RecordStatus] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_ClassificationRuleSets_Version",
                table: "ClassificationRuleSets",
                column: "Version",
                unique: true,
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_TalentClassifications_EmployeeId_PerformanceCycleId",
                table: "TalentClassifications",
                columns: new[] { "EmployeeId", "PerformanceCycleId" },
                unique: true,
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_TalentClassifications_IsHighPerformer",
                table: "TalentClassifications",
                column: "IsHighPerformer");

            migrationBuilder.CreateIndex(
                name: "IX_TalentClassifications_IsHighPotential",
                table: "TalentClassifications",
                column: "IsHighPotential");

            migrationBuilder.CreateIndex(
                name: "IX_TalentClassifications_NineBoxCode",
                table: "TalentClassifications",
                column: "NineBoxCode");

            migrationBuilder.CreateIndex(
                name: "IX_TalentClassifications_PerformanceBand",
                table: "TalentClassifications",
                column: "PerformanceBand");

            migrationBuilder.CreateIndex(
                name: "IX_TalentClassifications_PerformanceCycleId",
                table: "TalentClassifications",
                column: "PerformanceCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_TalentClassifications_PotentialBand",
                table: "TalentClassifications",
                column: "PotentialBand");

            migrationBuilder.CreateIndex(
                name: "IX_TalentClassifications_TalentScoreId",
                table: "TalentClassifications",
                column: "TalentScoreId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassificationRuleSets");

            migrationBuilder.DropTable(
                name: "TalentClassifications");
        }
    }
}
