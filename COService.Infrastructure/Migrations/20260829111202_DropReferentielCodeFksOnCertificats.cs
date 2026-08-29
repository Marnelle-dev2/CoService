using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace COService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class DropReferentielCodeFksOnCertificats : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Codes référentiels = MS Référentiel ; les FK locales bloquent la création de CO.
            DropFkIfExists(migrationBuilder, "FK_Certificats_Aeroports_AeroportCode");
            DropFkIfExists(migrationBuilder, "FK_Certificats_BattantsPavillon_BattantPavillonCode");
            DropFkIfExists(migrationBuilder, "FK_Certificats_BureauxDedouanements_BureauDedouanementCode");
            DropFkIfExists(migrationBuilder, "FK_Certificats_Devises_DeviseCode");
            DropFkIfExists(migrationBuilder, "FK_Certificats_Modules_ModuleCode");
            DropFkIfExists(migrationBuilder, "FK_Certificats_Pays_PaysDestinationCode");
            DropFkIfExists(migrationBuilder, "FK_Certificats_Ports_PortCongoCode");
            DropFkIfExists(migrationBuilder, "FK_Certificats_Ports_PortSortieCode");
            DropFkIfExists(migrationBuilder, "FK_Certificats_RoutesNationales_RouteCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Non restauré : les tables locales ne contiennent pas les codes du MS Référentiel.
        }

        private static void DropFkIfExists(MigrationBuilder migrationBuilder, string fkName)
        {
            migrationBuilder.Sql($@"
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = '{fkName}')
    ALTER TABLE [Certificats] DROP CONSTRAINT [{fkName}];
");
        }
    }
}
