using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExecutionPack5_PotentialFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "PotentialAssessments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformanceCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AssessedByEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AgilityScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    LeadershipScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    GrowthScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    MobilityScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    OverallPotentialScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    PotentialLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    AssessedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_PotentialAssessments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PotentialAssessments_Employees_AssessedByEmployeeId",
                        column: x => x.AssessedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PotentialAssessments_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_PotentialAssessments_PerformanceCycles_PerformanceCycleId",
                        column: x => x.PerformanceCycleId,
                        principalTable: "PerformanceCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "PotentialAssessmentFactors",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PotentialAssessmentId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FactorName = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    Weight = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_PotentialAssessmentFactors", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PotentialAssessmentFactors_PotentialAssessments_PotentialAssessmentId",
                        column: x => x.PotentialAssessmentId,
                        principalTable: "PotentialAssessments",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_PotentialAssessmentFactors_PotentialAssessmentId",
                table: "PotentialAssessmentFactors",
                column: "PotentialAssessmentId");

            migrationBuilder.CreateIndex(
                name: "IX_PotentialAssessments_AssessedByEmployeeId",
                table: "PotentialAssessments",
                column: "AssessedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PotentialAssessments_EmployeeId",
                table: "PotentialAssessments",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_PotentialAssessments_EmployeeId_PerformanceCycleId",
                table: "PotentialAssessments",
                columns: new[] { "EmployeeId", "PerformanceCycleId" },
                unique: true,
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_PotentialAssessments_PerformanceCycleId",
                table: "PotentialAssessments",
                column: "PerformanceCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_PotentialAssessments_PotentialLevel",
                table: "PotentialAssessments",
                column: "PotentialLevel");

            migrationBuilder.CreateIndex(
                name: "IX_PotentialAssessments_Status",
                table: "PotentialAssessments",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "PotentialAssessmentFactors");

            migrationBuilder.DropTable(
                name: "PotentialAssessments");
        }
    }
}
