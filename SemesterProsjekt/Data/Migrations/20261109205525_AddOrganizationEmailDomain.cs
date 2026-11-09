using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemesterProsjekt.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationEmailDomain : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AllowedEmailDomain",
                table: "Organizations",
                type: "varchar(253)",
                maxLength: 253,
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AllowedEmailDomain",
                table: "Organizations");
        }
    }
}
