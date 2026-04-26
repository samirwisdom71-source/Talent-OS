using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class DevelopmentPlanPathsAndImpact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSystemSuggested",
                table: "DevelopmentPlans",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "DevelopmentPlanImpactSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DevelopmentPlanId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Phase = table.Column<byte>(type: "tinyint", nullable: false),
                    RecordedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SummaryNotes = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    MetricScore = table.Column<decimal>(type: "decimal(18,4)", precision: 18, scale: 4, nullable: true),
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
                    table.PrimaryKey("PK_DevelopmentPlanImpactSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevelopmentPlanImpactSnapshots_DevelopmentPlans_DevelopmentPlanId",
                        column: x => x.DevelopmentPlanId,
                        principalTable: "DevelopmentPlans",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DevelopmentPlanItemPaths",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DevelopmentPlanItemId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    SortOrder = table.Column<int>(type: "int", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    PlannedStartUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    PlannedEndUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
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
                    table.PrimaryKey("PK_DevelopmentPlanItemPaths", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevelopmentPlanItemPaths_DevelopmentPlanItems_DevelopmentPlanItemId",
                        column: x => x.DevelopmentPlanItemId,
                        principalTable: "DevelopmentPlanItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DevelopmentPlanItemPathHelpers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DevelopmentPlanItemPathId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HelperKind = table.Column<byte>(type: "tinyint", nullable: false),
                    HelperEntityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
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
                    table.PrimaryKey("PK_DevelopmentPlanItemPathHelpers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevelopmentPlanItemPathHelpers_DevelopmentPlanItemPaths_DevelopmentPlanItemPathId",
                        column: x => x.DevelopmentPlanItemPathId,
                        principalTable: "DevelopmentPlanItemPaths",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlanImpactSnapshots_DevelopmentPlanId",
                table: "DevelopmentPlanImpactSnapshots",
                column: "DevelopmentPlanId");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlanImpactSnapshots_Plan_Phase",
                table: "DevelopmentPlanImpactSnapshots",
                columns: new[] { "DevelopmentPlanId", "Phase" },
                unique: true,
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlanItemPathHelpers_DevelopmentPlanItemPathId",
                table: "DevelopmentPlanItemPathHelpers",
                column: "DevelopmentPlanItemPathId");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlanItemPathHelpers_DevelopmentPlanItemPathId_HelperKind_HelperEntityId",
                table: "DevelopmentPlanItemPathHelpers",
                columns: new[] { "DevelopmentPlanItemPathId", "HelperKind", "HelperEntityId" });

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlanItemPaths_DevelopmentPlanItemId",
                table: "DevelopmentPlanItemPaths",
                column: "DevelopmentPlanItemId");

            migrationBuilder.CreateIndex(
                name: "IX_DevelopmentPlanItemPaths_DevelopmentPlanItemId_SortOrder",
                table: "DevelopmentPlanItemPaths",
                columns: new[] { "DevelopmentPlanItemId", "SortOrder" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DevelopmentPlanImpactSnapshots");

            migrationBuilder.DropTable(
                name: "DevelopmentPlanItemPathHelpers");

            migrationBuilder.DropTable(
                name: "DevelopmentPlanItemPaths");

            migrationBuilder.DropColumn(
                name: "IsSystemSuggested",
                table: "DevelopmentPlans");
        }
    }
}
