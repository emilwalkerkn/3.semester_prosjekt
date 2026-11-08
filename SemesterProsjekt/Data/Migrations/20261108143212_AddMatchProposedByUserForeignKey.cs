using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SemesterProsjekt.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMatchProposedByUserForeignKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 ALTER TABLE `Match`
                                 ADD CONSTRAINT FK_Match_ProposedByUser
                                 FOREIGN KEY (ProposedByUserId)
                                 REFERENCES AspNetUsers(Id)
                                 ON DELETE SET NULL;
                                 """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                                 ALTER TABLE `Match`
                                 DROP FOREIGN KEY FK_Match_ProposedByUser;
                                 """);
        }
    }
}
