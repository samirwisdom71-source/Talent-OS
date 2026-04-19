using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExecutionPack9_DevelopmentPlansFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "DevelopmentPlans",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PerformanceCycleId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PlanTitle = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    SourceType = table.Column<byte>(type: "tinyint", nullable: false),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    TargetCompletionDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    ApprovedByEmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    ApprovedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_DevelopmentPlans", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevelopmentPlans_Employees_ApprovedByEmployeeId",
                        column: x => x.ApprovedByEmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DevelopmentPlans_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DevelopmentPlans_PerformanceCycles_PerformanceCycleId",
                        column: x => x.PerformanceCycleId,
                        principalTable: "PerformanceCycles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DevelopmentPlanItems",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DevelopmentPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    ItemType = table.Column<byte>(type: "tinyint", nullable: false),
                    RelatedCompetencyId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TargetDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    ProgressPercentage = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
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
                    table.PrimaryKey("PK_DevelopmentPlanItems", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevelopmentPlanItems_Competencies_RelatedCompetencyId",
                        column: x => x.RelatedCompetencyId,
                        principalTable: "Competencies",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DevelopmentPlanItems_DevelopmentPlans_DevelopmentPlanId",
                        column: x => x.DevelopmentPlanId,
                        principalTable: "DevelopmentPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DevelopmentPlanLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DevelopmentPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    LinkType = table.Column<byte>(type: "tinyint", nullable: false),
                    LinkedEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_DevelopmentPlanLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevelopmentPlanLinks_DevelopmentPlans_DevelopmentPlanId",
                        column: x => x.DevelopmentPlanId,
                        principalTable: "DevelopmentPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlanItems_DevelopmentPlanId",
                table: "DevelopmentPlanItems",
                column: "DevelopmentPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlanItems_RelatedCompetencyId",
                table: "DevelopmentPlanItems",
                column: "RelatedCompetencyId");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlanItems_Status",
                table: "DevelopmentPlanItems",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlanLinks_DevelopmentPlanId",
                table: "DevelopmentPlanLinks",
                column: "DevelopmentPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlanLinks_LinkType",
                table: "DevelopmentPlanLinks",
                column: "LinkType");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlanLinks_Plan_Type_Entity",
                table: "DevelopmentPlanLinks",
                columns: new[] { "DevelopmentPlanId", "LinkType", "LinkedEntityId" },
                unique: true,
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlans_ApprovedByEmployeeId",
                table: "DevelopmentPlans",
                column: "ApprovedByEmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlans_EmployeeId",
                table: "DevelopmentPlans",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlans_PerformanceCycleId",
                table: "DevelopmentPlans",
                column: "PerformanceCycleId");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlans_SourceType",
                table: "DevelopmentPlans",
                column: "SourceType");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlans_Status",
                table: "DevelopmentPlans",
                column: "Status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevelopmentPlanItems");

            migrationBuilder.DropTable(
                name: "DevelopmentPlanLinks");

            migrationBuilder.DropTable(
                name: "DevelopmentPlans");
        }
    }
}
