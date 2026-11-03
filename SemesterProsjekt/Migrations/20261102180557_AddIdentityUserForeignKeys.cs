using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemesterProsjekt.Migrations
{
    /// <inheritdoc />
    public partial class AddIdentityUserForeignKeys : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddForeignKey(
                name: "FK_Needs_AspNetUsers_UserId",
                table: "Needs",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Resources_AspNetUsers_UserId",
                table: "Resources",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Needs_AspNetUsers_UserId",
                table: "Needs");

            migrationBuilder.DropForeignKey(
                name: "FK_Resources_AspNetUsers_UserId",
                table: "Resources");

        }
    }
}
