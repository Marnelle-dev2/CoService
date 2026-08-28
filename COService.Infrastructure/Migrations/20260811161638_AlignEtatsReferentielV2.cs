using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace COService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AlignEtatsReferentielV2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "Actif",
                table: "Etats",
                type: "bit",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<string>(
                name: "Domaine",
                table: "Etats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "TypeEtat",
                table: "Etats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Etats_Domaine",
                table: "Etats",
                column: "Domaine");

            // Remap codes texte legacy → codes V2 / CO (FK EtatCode → Etats.Code)
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Certificats_Etats_EtatCode')
                    ALTER TABLE [Certificats] NOCHECK CONSTRAINT [FK_Certificats_Etats_EtatCode];

                UPDATE Certificats SET EtatCode = '42' WHERE EtatCode = 'ELABORE';
                UPDATE Certificats SET EtatCode = '79' WHERE EtatCode = 'SOUMIS';
                UPDATE Certificats SET EtatCode = 'CO43' WHERE EtatCode = 'CONTROLE';
                UPDATE Certificats SET EtatCode = '45' WHERE EtatCode = 'APPROUVE';
                UPDATE Certificats SET EtatCode = '50' WHERE EtatCode = 'VALIDE';
                UPDATE Certificats SET EtatCode = '80' WHERE EtatCode = 'REJETE';
                UPDATE Certificats SET EtatCode = '66' WHERE EtatCode = 'MODIFICATION';
                UPDATE Certificats SET EtatCode = 'CO_FA_SOUMISE' WHERE EtatCode = 'FORMULE_A_SOUMISE';
                UPDATE Certificats SET EtatCode = 'CO_FA_CONTROLEE' WHERE EtatCode = 'FORMULE_A_CONTROLEE';
                UPDATE Certificats SET EtatCode = 'CO_FA_APPROUVEE' WHERE EtatCode = 'FORMULE_A_APPROUVEE';
                UPDATE Certificats SET EtatCode = 'CO_FA_VALIDEE' WHERE EtatCode = 'FORMULE_A_VALIDEE';

                UPDATE Etats SET Code = '42', Libelle = N'Élaboré', CodeEcran = 'E', Domaine = 'COMMUN', TypeEtat = 'METIER', Actif = 1 WHERE Code = 'ELABORE';
                UPDATE Etats SET Code = '79', Libelle = N'Visa demandé', CodeEcran = 'VD', Domaine = 'COMMUN', TypeEtat = 'METIER', Actif = 1 WHERE Code = 'SOUMIS';
                UPDATE Etats SET Code = 'CO43', Libelle = N'Contrôlé', CodeEcran = 'CC', Domaine = 'CERTIFICAT_ORIGINE', TypeEtat = 'METIER', Actif = 1 WHERE Code = 'CONTROLE';
                UPDATE Etats SET Code = '45', Libelle = N'Controller', CodeEcran = 'CO', Domaine = 'COMMUN', TypeEtat = 'METIER', Actif = 1 WHERE Code = 'APPROUVE';
                UPDATE Etats SET Code = '50', Libelle = N'Ouvert', CodeEcran = 'O', Domaine = 'COMMUN', TypeEtat = 'METIER', Actif = 1 WHERE Code = 'VALIDE';
                UPDATE Etats SET Code = '80', Libelle = N'Visas refusés', CodeEcran = 'VR', Domaine = 'COMMUN', TypeEtat = 'METIER', Actif = 1 WHERE Code = 'REJETE';
                UPDATE Etats SET Code = '66', Libelle = N'Modification demandée', CodeEcran = 'MD', Domaine = 'COMMUN', TypeEtat = 'METIER', Actif = 1 WHERE Code = 'MODIFICATION';
                UPDATE Etats SET Code = 'CO_FA_SOUMISE', Domaine = 'CERTIFICAT_ORIGINE', TypeEtat = 'METIER', Actif = 1 WHERE Code = 'FORMULE_A_SOUMISE';
                UPDATE Etats SET Code = 'CO_FA_CONTROLEE', Domaine = 'CERTIFICAT_ORIGINE', TypeEtat = 'METIER', Actif = 1 WHERE Code = 'FORMULE_A_CONTROLEE';
                UPDATE Etats SET Code = 'CO_FA_APPROUVEE', Domaine = 'CERTIFICAT_ORIGINE', TypeEtat = 'METIER', Actif = 1 WHERE Code = 'FORMULE_A_APPROUVEE';
                UPDATE Etats SET Code = 'CO_FA_VALIDEE', Domaine = 'CERTIFICAT_ORIGINE', TypeEtat = 'METIER', Actif = 1 WHERE Code = 'FORMULE_A_VALIDEE';

                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Certificats_Etats_EtatCode')
                    ALTER TABLE [Certificats] WITH CHECK CHECK CONSTRAINT [FK_Certificats_Etats_EtatCode];
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Certificats_Etats_EtatCode')
                    ALTER TABLE [Certificats] NOCHECK CONSTRAINT [FK_Certificats_Etats_EtatCode];

                UPDATE Certificats SET EtatCode = 'ELABORE' WHERE EtatCode = '42';
                UPDATE Certificats SET EtatCode = 'SOUMIS' WHERE EtatCode = '79';
                UPDATE Certificats SET EtatCode = 'CONTROLE' WHERE EtatCode = 'CO43';
                UPDATE Certificats SET EtatCode = 'APPROUVE' WHERE EtatCode = '45';
                UPDATE Certificats SET EtatCode = 'VALIDE' WHERE EtatCode = '50';
                UPDATE Certificats SET EtatCode = 'REJETE' WHERE EtatCode = '80';
                UPDATE Certificats SET EtatCode = 'MODIFICATION' WHERE EtatCode = '66';
                UPDATE Certificats SET EtatCode = 'FORMULE_A_SOUMISE' WHERE EtatCode = 'CO_FA_SOUMISE';
                UPDATE Certificats SET EtatCode = 'FORMULE_A_CONTROLEE' WHERE EtatCode = 'CO_FA_CONTROLEE';
                UPDATE Certificats SET EtatCode = 'FORMULE_A_APPROUVEE' WHERE EtatCode = 'CO_FA_APPROUVEE';
                UPDATE Certificats SET EtatCode = 'FORMULE_A_VALIDEE' WHERE EtatCode = 'CO_FA_VALIDEE';

                UPDATE Etats SET Code = 'ELABORE' WHERE Code = '42';
                UPDATE Etats SET Code = 'SOUMIS' WHERE Code = '79';
                UPDATE Etats SET Code = 'CONTROLE' WHERE Code = 'CO43';
                UPDATE Etats SET Code = 'APPROUVE' WHERE Code = '45';
                UPDATE Etats SET Code = 'VALIDE' WHERE Code = '50';
                UPDATE Etats SET Code = 'REJETE' WHERE Code = '80';
                UPDATE Etats SET Code = 'MODIFICATION' WHERE Code = '66';
                UPDATE Etats SET Code = 'FORMULE_A_SOUMISE' WHERE Code = 'CO_FA_SOUMISE';
                UPDATE Etats SET Code = 'FORMULE_A_CONTROLEE' WHERE Code = 'CO_FA_CONTROLEE';
                UPDATE Etats SET Code = 'FORMULE_A_APPROUVEE' WHERE Code = 'CO_FA_APPROUVEE';
                UPDATE Etats SET Code = 'FORMULE_A_VALIDEE' WHERE Code = 'CO_FA_VALIDEE';

                IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Certificats_Etats_EtatCode')
                    ALTER TABLE [Certificats] WITH CHECK CHECK CONSTRAINT [FK_Certificats_Etats_EtatCode];
                """);

            migrationBuilder.DropIndex(
                name: "IX_Etats_Domaine",
                table: "Etats");

            migrationBuilder.DropColumn(
                name: "Actif",
                table: "Etats");

            migrationBuilder.DropColumn(
                name: "Domaine",
                table: "Etats");

            migrationBuilder.DropColumn(
                name: "TypeEtat",
                table: "Etats");
        }
    }
}
