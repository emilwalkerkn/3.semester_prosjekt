using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemesterProsjekt.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddOrganizationAdminInvitationType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsOrganizationAdminInvitation",
                table: "OrganizationInvitations",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsOrganizationAdminInvitation",
                table: "OrganizationInvitations");
        }
    }
}
