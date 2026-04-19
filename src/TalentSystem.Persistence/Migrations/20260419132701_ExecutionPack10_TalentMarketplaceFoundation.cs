using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class ExecutionPack10_TalentMarketplaceFoundation : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "MarketplaceOpportunities",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(256)", maxLength: 256, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    OpportunityType = table.Column<byte>(type: "tinyint", nullable: false),
                    OrganizationUnitId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    PositionId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    RequiredCompetencySummary = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Status = table.Column<byte>(type: "tinyint", nullable: false),
                    OpenDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    CloseDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    MaxApplicants = table.Column<int>(type: "int", nullable: true),
                    IsConfidential = table.Column<bool>(type: "bit", nullable: false),
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
                    table.PrimaryKey("PK_MarketplaceOpportunities", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MarketplaceOpportunities_OrganizationUnits_OrganizationUnitId",
                        column: x => x.OrganizationUnitId,
                        principalTable: "OrganizationUnits",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MarketplaceOpportunities_Positions_PositionId",
                        column: x => x.PositionId,
                        principalTable: "Positions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarketplaceOpportunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ApplicationStatus = table.Column<byte>(type: "tinyint", nullable: false),
                    MotivationStatement = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: true),
                    AppliedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ReviewedOnUtc = table.Column<DateTime>(type: "datetime2", nullable: true),
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
                    table.PrimaryKey("PK_OpportunityApplications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityApplications_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpportunityApplications_MarketplaceOpportunities_MarketplaceOpportunityId",
                        column: x => x.MarketplaceOpportunityId,
                        principalTable: "MarketplaceOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "OpportunityMatchSnapshots",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MarketplaceOpportunityId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    EmployeeId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    MatchScore = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: false),
                    MatchLevel = table.Column<byte>(type: "tinyint", nullable: false),
                    Notes = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
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
                    table.PrimaryKey("PK_OpportunityMatchSnapshots", x => x.Id);
                    table.ForeignKey(
                        name: "FK_OpportunityMatchSnapshots_Employees_EmployeeId",
                        column: x => x.EmployeeId,
                        principalTable: "Employees",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_OpportunityMatchSnapshots_MarketplaceOpportunities_MarketplaceOpportunityId",
                        column: x => x.MarketplaceOpportunityId,
                        principalTable: "MarketplaceOpportunities",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceOpportunities_OpenDate",
                table: "MarketplaceOpportunities",
                column: "OpenDate");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceOpportunities_OpportunityType",
                table: "MarketplaceOpportunities",
                column: "OpportunityType");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceOpportunities_OrganizationUnitId",
                table: "MarketplaceOpportunities",
                column: "OrganizationUnitId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceOpportunities_PositionId",
                table: "MarketplaceOpportunities",
                column: "PositionId");

            migrationBuilder.CreateIndex(
                name: "IX_MarketplaceOpportunities_Status",
                table: "MarketplaceOpportunities",
                column: "Status");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityApplications_ApplicationStatus",
                table: "OpportunityApplications",
                column: "ApplicationStatus");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityApplications_AppliedOnUtc",
                table: "OpportunityApplications",
                column: "AppliedOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityApplications_EmployeeId",
                table: "OpportunityApplications",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityApplications_MarketplaceOpportunityId_EmployeeId",
                table: "OpportunityApplications",
                columns: new[] { "MarketplaceOpportunityId", "EmployeeId" },
                unique: true,
                filter: "[RecordStatus] <> 3");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityMatchSnapshots_EmployeeId",
                table: "OpportunityMatchSnapshots",
                column: "EmployeeId");

            migrationBuilder.CreateIndex(
                name: "IX_OpportunityMatchSnapshots_Opportunity_Employee",
                table: "OpportunityMatchSnapshots",
                columns: new[] { "MarketplaceOpportunityId", "EmployeeId" },
                unique: true,
                filter: "[RecordStatus] <> 3");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OpportunityApplications");

            migrationBuilder.DropTable(
                name: "OpportunityMatchSnapshots");

            migrationBuilder.DropTable(
                name: "MarketplaceOpportunities");
        }
    }
}
