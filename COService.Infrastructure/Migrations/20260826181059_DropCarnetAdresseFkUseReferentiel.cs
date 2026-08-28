using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace COService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropCarnetAdresseFkUseReferentiel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_CarnetsAdresses_CarnetAdresseCode",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_CarnetAdresseCode",
                table: "Certificats");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_CarnetsAdresses_Code",
                table: "CarnetsAdresses");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddUniqueConstraint(
                name: "AK_CarnetsAdresses_Code",
                table: "CarnetsAdresses",
                column: "Code");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_CarnetAdresseCode",
                table: "Certificats",
                column: "CarnetAdresseCode");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_CarnetsAdresses_CarnetAdresseCode",
                table: "Certificats",
                column: "CarnetAdresseCode",
                principalTable: "CarnetsAdresses",
                principalColumn: "Code");
        }
    }
}
