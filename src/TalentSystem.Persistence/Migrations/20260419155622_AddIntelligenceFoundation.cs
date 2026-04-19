using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIntelligenceFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "IntelligenceRuns",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    RunType = table.Column<byte>(type: "tinyint", nullable: false),
                    PerformanceCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    StartedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CompletedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    TotalInsightsGenerated = table.Column<int>(type: "int", nullable: false),
                    TotalRecommendationsGenerated = table.Column<int>(type: "int", nullable: false),
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
                    table.PrimaryKey("PK_IntelligenceRuns", x => x.Id);
                    table.ForeignKey(
                        name: "FK_IntelligenceRuns_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_IntelligenceRuns_PerformanceCycles_PerformanceCycleId",
                        column: x => x.PerformanceCycleId,
                        principalTable: "PerformanceCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TalentInsights",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PerformanceCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    InsightType = table.Column<byte>(type: "tinyint", nullable: false),
                    Severity = table.Column<byte>(type: "tinyint", nullable: false),
                    Source = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Summary = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    ConfidenceScore = table.Column<byte>(type: "tinyint", nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    GeneratedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_TalentInsights", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalentInsights_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TalentInsights_PerformanceCycles_PerformanceCycleId",
                        column: x => x.PerformanceCycleId,
                        principalTable: "PerformanceCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TalentRecommendations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformanceCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RecommendationType = table.Column<byte>(type: "tinyint", nullable: false),
                    Priority = table.Column<byte>(type: "tinyint", nullable: false),
                    Source = table.Column<byte>(type: "tinyint", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false),
                    RecommendedAction = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    ConfidenceScore = table.Column<byte>(type: "tinyint", nullable: false),
                    RelatedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RelatedEntityType = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    GeneratedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
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
                    table.PrimaryKey("PK_TalentRecommendations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TalentRecommendations_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TalentRecommendations_PerformanceCycles_PerformanceCycleId",
                        column: x => x.PerformanceCycleId,
                        principalTable: "PerformanceCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceRuns_EmployeeId",
                table: "IntelligenceRuns",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceRuns_PerformanceCycleId",
                table: "IntelligenceRuns",
                column: "PerformanceCycleId",
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceRuns_RunType",
                table: "IntelligenceRuns",
                column: "RunType",
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceRuns_StartedOnUtc",
                table: "IntelligenceRuns",
                column: "StartedOnUtc",
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_IntelligenceRuns_Status",
                table: "IntelligenceRuns",
                column: "Status",
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_TalentInsights_EmployeeId",
                table: "TalentInsights",
                column: "EmployeeId",
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_TalentInsights_GeneratedOnUtc",
                table: "TalentInsights",
                column: "GeneratedOnUtc",
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_TalentInsights_InsightType",
                table: "TalentInsights",
                column: "InsightType",
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_TalentInsights_PerformanceCycleId",
                table: "TalentInsights",
                column: "PerformanceCycleId",
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_TalentInsights_Severity",
                table: "TalentInsights",
                column: "Severity",
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_TalentInsights_Status",
                table: "TalentInsights",
                column: "Status",
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_TalentRecommendations_EmployeeId",
                table: "TalentRecommendations",
                column: "EmployeeId",
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_TalentRecommendations_GeneratedOnUtc",
                table: "TalentRecommendations",
                column: "GeneratedOnUtc",
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_TalentRecommendations_PerformanceCycleId",
                table: "TalentRecommendations",
                column: "PerformanceCycleId",
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_TalentRecommendations_Priority",
                table: "TalentRecommendations",
                column: "Priority",
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_TalentRecommendations_RecommendationType",
                table: "TalentRecommendations",
                column: "RecommendationType",
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_TalentRecommendations_Status",
                table: "TalentRecommendations",
                column: "Status",
                filter: "[RecordStatus] <> 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "IntelligenceRuns");

            migrationBuilder.DropTable(
                name: "TalentInsights");

            migrationBuilder.DropTable(
                name: "TalentRecommendations");
        }
    }
}
