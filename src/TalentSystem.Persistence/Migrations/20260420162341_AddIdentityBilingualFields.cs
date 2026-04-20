using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TalentSystem.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityBilingualFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AppRoles",
                newName: "NameEn");

            migrationBuilder.RenameColumn(
                name: "Description",
                table: "AppRoles",
                newName: "DescriptionEn");

            migrationBuilder.RenameIndex(
                name: "IX_AppRoles_Name",
                table: "AppRoles",
                newName: "IX_AppRoles_NameEn");

            migrationBuilder.RenameColumn(
                name: "Name",
                table: "AppPermissions",
                newName: "NameEn");

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "AppUsers",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameEn",
                table: "AppUsers",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "AppRoles",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "AppRoles",
                type: "nvarchar(128)",
                maxLength: 128,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "DescriptionAr",
                table: "AppPermissions",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DescriptionEn",
                table: "AppPermissions",
                type: "nvarchar(512)",
                maxLength: 512,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "NameAr",
                table: "AppPermissions",
                type: "nvarchar(256)",
                maxLength: 256,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "NameEn",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "AppRoles");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "AppRoles");

            migrationBuilder.DropColumn(
                name: "DescriptionAr",
                table: "AppPermissions");

            migrationBuilder.DropColumn(
                name: "DescriptionEn",
                table: "AppPermissions");

            migrationBuilder.DropColumn(
                name: "NameAr",
                table: "AppPermissions");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "AppRoles",
                newName: "Name");

            migrationBuilder.RenameColumn(
                name: "DescriptionEn",
                table: "AppRoles",
                newName: "Description");

            migrationBuilder.RenameIndex(
                name: "IX_AppRoles_NameEn",
                table: "AppRoles",
                newName: "IX_AppRoles_Name");

            migrationBuilder.RenameColumn(
                name: "NameEn",
                table: "AppPermissions",
                newName: "Name");
        }
    }
}
