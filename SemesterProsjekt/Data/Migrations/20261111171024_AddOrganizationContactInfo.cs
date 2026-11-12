using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemesterProsjekt.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationContactInfo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ContactEmail",
                table: "Organizations",
                type: "varchar(256)",
                maxLength: 256,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AddColumn<string>(
                name: "ContactPersonName",
                table: "Organizations",
                type: "varchar(200)",
                maxLength: 200,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ContactEmail",
                table: "Organizations");

            migrationBuilder.DropColumn(
                name: "ContactPersonName",
                table: "Organizations");
        }
    }
}
