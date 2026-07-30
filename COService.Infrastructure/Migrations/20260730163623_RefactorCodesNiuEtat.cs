using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace COService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class RefactorCodesNiuEtat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_BureauxDedouanements_BureauDedouanementId",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_CarnetsAdresses_CarnetAdresseId",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_Devises_DeviseId",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_Exportateurs_ExportateurId",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_Modules_ModuleId",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_Partenaires_PartenaireId",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_Pays_PaysDestinationId",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_Ports_PortCongoId",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_Ports_PortSortieId",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_StatutsCertificats_StatutCertificatId",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_ZonesProductions_ZoneProductionId",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_ZonesProductions_Partenaires_PartenaireId",
                table: "ZonesProductions");

            migrationBuilder.DropTable(
                name: "Exportateurs");

            migrationBuilder.DropTable(
                name: "LignesCertificats");

            migrationBuilder.DropTable(
                name: "StatutsCertificats");

            migrationBuilder.DropTable(
                name: "Partenaires");

            migrationBuilder.DropTable(
                name: "TypesPartenaires");

            migrationBuilder.DropIndex(
                name: "IX_ZonesProductions_PartenaireId",
                table: "ZonesProductions");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_BureauDedouanementId",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_CarnetAdresseId",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_DeviseId",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_ExportateurId",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_ModuleId",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_PartenaireId",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_PaysDestinationId",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_PortCongoId",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_PortSortieId",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_StatutCertificatId",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_ZoneProductionId",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "PartenaireId",
                table: "ZonesProductions");

            migrationBuilder.DropColumn(
                name: "BureauDedouanementId",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "CarnetAdresseId",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "DeviseId",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "ExportateurId",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "Mandataire",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "ModuleId",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "PartenaireId",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "PaysDestinationId",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "PortCongoId",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "PortSortieId",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "StatutCertificatId",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "ZoneProductionId",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "documents_id",
                table: "Certificats");

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "ZonesProductions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PartenaireNIU",
                table: "ZonesProductions",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AeroportCode",
                table: "Certificats",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BattantPavillonCode",
                table: "Certificats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BureauDedouanementCode",
                table: "Certificats",
                type: "nvarchar(10)",
                maxLength: 10,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CarnetAdresseCode",
                table: "Certificats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "CodeDocument",
                table: "Certificats",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviseCode",
                table: "Certificats",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EtatCode",
                table: "Certificats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExportateurNIU",
                table: "Certificats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ExportateurNom",
                table: "Certificats",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MandataireNIU",
                table: "Certificats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "MandataireNom",
                table: "Certificats",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModePaiement",
                table: "Certificats",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModePaiementCode",
                table: "Certificats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModuleCode",
                table: "Certificats",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartenaireNIU",
                table: "Certificats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PartenaireNom",
                table: "Certificats",
                type: "nvarchar(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PaysDestinationCode",
                table: "Certificats",
                type: "nvarchar(3)",
                maxLength: 3,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PortCongoCode",
                table: "Certificats",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PortSortieCode",
                table: "Certificats",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "RouteCode",
                table: "Certificats",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ZoneProductionCode",
                table: "Certificats",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Code",
                table: "CarnetsAdresses",
                type: "nvarchar(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(@"
UPDATE ZonesProductions SET Code = LEFT(REPLACE(CONVERT(nvarchar(36), Id), '-', ''), 50)
WHERE Code IS NULL OR LTRIM(RTRIM(Code)) = N'';
UPDATE CarnetsAdresses SET Code = LEFT(REPLACE(CONVERT(nvarchar(36), Id), '-', ''), 50)
WHERE Code IS NULL OR LTRIM(RTRIM(Code)) = N'';
");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_ZonesProductions_Code",
                table: "ZonesProductions",
                column: "Code");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_RoutesNationales_Code",
                table: "RoutesNationales",
                column: "Code");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Ports_Code",
                table: "Ports",
                column: "Code");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Pays_Code",
                table: "Pays",
                column: "Code");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Modules_Code",
                table: "Modules",
                column: "Code");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Devises_Code",
                table: "Devises",
                column: "Code");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_CarnetsAdresses_Code",
                table: "CarnetsAdresses",
                column: "Code");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_BureauxDedouanements_Code",
                table: "BureauxDedouanements",
                column: "Code");

            migrationBuilder.AddUniqueConstraint(
                name: "AK_Aeroports_Code",
                table: "Aeroports",
                column: "Code");

            migrationBuilder.CreateTable(
                name: "BattantsPavillon",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Designation = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreeLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreePar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ModifiePar = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BattantsPavillon", x => x.id);
                    table.UniqueConstraint("AK_BattantsPavillon_Code", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Etats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Libelle = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CodeEcran = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreeLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreePar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ModifiePar = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Etats", x => x.id);
                    table.UniqueConstraint("AK_Etats_Code", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "Produits",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Nom = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Categorie = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    UniteStatistiqueCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UniteStatistique = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    Actif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    CreeLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreePar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ModifiePar = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Produits", x => x.id);
                    table.UniqueConstraint("AK_Produits_Code", x => x.Code);
                });

            migrationBuilder.CreateTable(
                name: "SagasCertificatPostValidation",
                columns: table => new
                {
                    CorrelationId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CurrentState = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    CertificateNo = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    ExportateurNIU = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    PartenaireNIU = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    NumeroFacture = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PdfUrl = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    LastError = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true),
                    FacturationRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PdfRequestId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Version = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SagasCertificatPostValidation", x => x.CorrelationId);
                });

            migrationBuilder.CreateTable(
                name: "CertificatLignes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    CertificatId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    HSCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PositionTarifaire = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    NatureProduit = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Quantite = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    UniteStatistiqueCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    UniteStatistique = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PoidsBrut = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    PoidsNet = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ValeurFOB = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Volume = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    DeviseCode = table.Column<string>(type: "nvarchar(3)", maxLength: 3, nullable: true),
                    Devise = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    ProduitCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CreeLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreePar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    ModifiePar = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CertificatLignes", x => x.id);
                    table.ForeignKey(
                        name: "FK_CertificatLignes_Certificats_CertificatId",
                        column: x => x.CertificatId,
                        principalTable: "Certificats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CertificatLignes_Produits_ProduitCode",
                        column: x => x.ProduitCode,
                        principalTable: "Produits",
                        principalColumn: "Code");
                });

            migrationBuilder.CreateIndex(
                name: "IX_ZonesProductions_Code",
                table: "ZonesProductions",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_AeroportCode",
                table: "Certificats",
                column: "AeroportCode");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_BattantPavillonCode",
                table: "Certificats",
                column: "BattantPavillonCode");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_BureauDedouanementCode",
                table: "Certificats",
                column: "BureauDedouanementCode");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_CarnetAdresseCode",
                table: "Certificats",
                column: "CarnetAdresseCode");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_DeviseCode",
                table: "Certificats",
                column: "DeviseCode");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_EtatCode",
                table: "Certificats",
                column: "EtatCode");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_ModuleCode",
                table: "Certificats",
                column: "ModuleCode");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_PaysDestinationCode",
                table: "Certificats",
                column: "PaysDestinationCode");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_PortCongoCode",
                table: "Certificats",
                column: "PortCongoCode");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_PortSortieCode",
                table: "Certificats",
                column: "PortSortieCode");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_RouteCode",
                table: "Certificats",
                column: "RouteCode");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_ZoneProductionCode",
                table: "Certificats",
                column: "ZoneProductionCode");

            migrationBuilder.CreateIndex(
                name: "IX_CarnetsAdresses_Code",
                table: "CarnetsAdresses",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_BattantsPavillon_Code",
                table: "BattantsPavillon",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CertificatLignes_CertificatId",
                table: "CertificatLignes",
                column: "CertificatId");

            migrationBuilder.CreateIndex(
                name: "IX_CertificatLignes_ProduitCode",
                table: "CertificatLignes",
                column: "ProduitCode");

            migrationBuilder.CreateIndex(
                name: "IX_Etats_Code",
                table: "Etats",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Produits_Code",
                table: "Produits",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_Aeroports_AeroportCode",
                table: "Certificats",
                column: "AeroportCode",
                principalTable: "Aeroports",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_BattantsPavillon_BattantPavillonCode",
                table: "Certificats",
                column: "BattantPavillonCode",
                principalTable: "BattantsPavillon",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_BureauxDedouanements_BureauDedouanementCode",
                table: "Certificats",
                column: "BureauDedouanementCode",
                principalTable: "BureauxDedouanements",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_CarnetsAdresses_CarnetAdresseCode",
                table: "Certificats",
                column: "CarnetAdresseCode",
                principalTable: "CarnetsAdresses",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_Devises_DeviseCode",
                table: "Certificats",
                column: "DeviseCode",
                principalTable: "Devises",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_Etats_EtatCode",
                table: "Certificats",
                column: "EtatCode",
                principalTable: "Etats",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_Modules_ModuleCode",
                table: "Certificats",
                column: "ModuleCode",
                principalTable: "Modules",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_Pays_PaysDestinationCode",
                table: "Certificats",
                column: "PaysDestinationCode",
                principalTable: "Pays",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_Ports_PortCongoCode",
                table: "Certificats",
                column: "PortCongoCode",
                principalTable: "Ports",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_Ports_PortSortieCode",
                table: "Certificats",
                column: "PortSortieCode",
                principalTable: "Ports",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_RoutesNationales_RouteCode",
                table: "Certificats",
                column: "RouteCode",
                principalTable: "RoutesNationales",
                principalColumn: "Code");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_ZonesProductions_ZoneProductionCode",
                table: "Certificats",
                column: "ZoneProductionCode",
                principalTable: "ZonesProductions",
                principalColumn: "Code");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_Aeroports_AeroportCode",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_BattantsPavillon_BattantPavillonCode",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_BureauxDedouanements_BureauDedouanementCode",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_CarnetsAdresses_CarnetAdresseCode",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_Devises_DeviseCode",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_Etats_EtatCode",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_Modules_ModuleCode",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_Pays_PaysDestinationCode",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_Ports_PortCongoCode",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_Ports_PortSortieCode",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_RoutesNationales_RouteCode",
                table: "Certificats");

            migrationBuilder.DropForeignKey(
                name: "FK_Certificats_ZonesProductions_ZoneProductionCode",
                table: "Certificats");

            migrationBuilder.DropTable(
                name: "BattantsPavillon");

            migrationBuilder.DropTable(
                name: "CertificatLignes");

            migrationBuilder.DropTable(
                name: "Etats");

            migrationBuilder.DropTable(
                name: "SagasCertificatPostValidation");

            migrationBuilder.DropTable(
                name: "Produits");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_ZonesProductions_Code",
                table: "ZonesProductions");

            migrationBuilder.DropIndex(
                name: "IX_ZonesProductions_Code",
                table: "ZonesProductions");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_RoutesNationales_Code",
                table: "RoutesNationales");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Ports_Code",
                table: "Ports");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Pays_Code",
                table: "Pays");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Modules_Code",
                table: "Modules");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Devises_Code",
                table: "Devises");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_AeroportCode",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_BattantPavillonCode",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_BureauDedouanementCode",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_CarnetAdresseCode",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_DeviseCode",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_EtatCode",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_ModuleCode",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_PaysDestinationCode",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_PortCongoCode",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_PortSortieCode",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_RouteCode",
                table: "Certificats");

            migrationBuilder.DropIndex(
                name: "IX_Certificats_ZoneProductionCode",
                table: "Certificats");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_CarnetsAdresses_Code",
                table: "CarnetsAdresses");

            migrationBuilder.DropIndex(
                name: "IX_CarnetsAdresses_Code",
                table: "CarnetsAdresses");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_BureauxDedouanements_Code",
                table: "BureauxDedouanements");

            migrationBuilder.DropUniqueConstraint(
                name: "AK_Aeroports_Code",
                table: "Aeroports");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "ZonesProductions");

            migrationBuilder.DropColumn(
                name: "PartenaireNIU",
                table: "ZonesProductions");

            migrationBuilder.DropColumn(
                name: "AeroportCode",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "BattantPavillonCode",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "BureauDedouanementCode",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "CarnetAdresseCode",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "CodeDocument",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "DeviseCode",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "EtatCode",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "ExportateurNIU",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "ExportateurNom",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "MandataireNIU",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "MandataireNom",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "ModePaiement",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "ModePaiementCode",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "ModuleCode",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "PartenaireNIU",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "PartenaireNom",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "PaysDestinationCode",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "PortCongoCode",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "PortSortieCode",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "RouteCode",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "ZoneProductionCode",
                table: "Certificats");

            migrationBuilder.DropColumn(
                name: "Code",
                table: "CarnetsAdresses");

            migrationBuilder.AddColumn<Guid>(
                name: "PartenaireId",
                table: "ZonesProductions",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<Guid>(
                name: "BureauDedouanementId",
                table: "Certificats",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "CarnetAdresseId",
                table: "Certificats",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeviseId",
                table: "Certificats",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ExportateurId",
                table: "Certificats",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Mandataire",
                table: "Certificats",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ModuleId",
                table: "Certificats",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PartenaireId",
                table: "Certificats",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PaysDestinationId",
                table: "Certificats",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PortCongoId",
                table: "Certificats",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "PortSortieId",
                table: "Certificats",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "StatutCertificatId",
                table: "Certificats",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "ZoneProductionId",
                table: "Certificats",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "documents_id",
                table: "Certificats",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "LignesCertificats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    certificate_id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DeviseId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    IncotermId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PositionTarifaireId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    UniteStatistiqueId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    CreeLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreePar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    HSCode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LineFOBValue = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LineGrossWeight = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LineNatureOfProduct = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    LineNetWeight = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LineQuantity = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LineUnits = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    LineVolume = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    ModifiePar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LignesCertificats", x => x.id);
                    table.ForeignKey(
                        name: "FK_LignesCertificats_Certificats_certificate_id",
                        column: x => x.certificate_id,
                        principalTable: "Certificats",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_LignesCertificats_Devises_DeviseId",
                        column: x => x.DeviseId,
                        principalTable: "Devises",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LignesCertificats_Incoterms_IncotermId",
                        column: x => x.IncotermId,
                        principalTable: "Incoterms",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LignesCertificats_PositionsTariffaires_PositionTarifaireId",
                        column: x => x.PositionTarifaireId,
                        principalTable: "PositionsTariffaires",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_LignesCertificats_UniteStatistiques_UniteStatistiqueId",
                        column: x => x.UniteStatistiqueId,
                        principalTable: "UniteStatistiques",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "StatutsCertificats",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreeLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreePar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifiePar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Nom = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StatutsCertificats", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "TypesPartenaires",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Actif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Code = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreeLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreePar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Description = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    ModifiePar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Nom = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TypesPartenaires", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Partenaires",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartementId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    TypePartenaireId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Actif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Adresse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CodePartenaire = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreeLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreePar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DerniereSynchronisation = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiePar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Nom = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    Telephone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Partenaires", x => x.id);
                    table.ForeignKey(
                        name: "FK_Partenaires_Departements_DepartementId",
                        column: x => x.DepartementId,
                        principalTable: "Departements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Partenaires_TypesPartenaires_TypePartenaireId",
                        column: x => x.TypePartenaireId,
                        principalTable: "TypesPartenaires",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Exportateurs",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    DepartementId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    PartenaireId = table.Column<Guid>(type: "uniqueidentifier", nullable: true),
                    Actif = table.Column<bool>(type: "bit", nullable: false, defaultValue: true),
                    Adresse = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true),
                    CodeActivite = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    CodeExportateur = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    CreeLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    CreePar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    DerniereSynchronisation = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    Email = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    ModifiePar = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    ModifierLe = table.Column<DateTime>(type: "datetime2(7)", nullable: true),
                    NIU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    Nom = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    RCCM = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    RaisonSociale = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    Telephone = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: true),
                    TypeExportateur = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Exportateurs", x => x.id);
                    table.ForeignKey(
                        name: "FK_Exportateurs_Departements_DepartementId",
                        column: x => x.DepartementId,
                        principalTable: "Departements",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_Exportateurs_Partenaires_PartenaireId",
                        column: x => x.PartenaireId,
                        principalTable: "Partenaires",
                        principalColumn: "id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ZonesProductions_PartenaireId",
                table: "ZonesProductions",
                column: "PartenaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_BureauDedouanementId",
                table: "Certificats",
                column: "BureauDedouanementId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_CarnetAdresseId",
                table: "Certificats",
                column: "CarnetAdresseId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_DeviseId",
                table: "Certificats",
                column: "DeviseId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_ExportateurId",
                table: "Certificats",
                column: "ExportateurId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_ModuleId",
                table: "Certificats",
                column: "ModuleId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_PartenaireId",
                table: "Certificats",
                column: "PartenaireId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_PaysDestinationId",
                table: "Certificats",
                column: "PaysDestinationId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_PortCongoId",
                table: "Certificats",
                column: "PortCongoId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_PortSortieId",
                table: "Certificats",
                column: "PortSortieId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_StatutCertificatId",
                table: "Certificats",
                column: "StatutCertificatId");

            migrationBuilder.CreateIndex(
                name: "IX_Certificats_ZoneProductionId",
                table: "Certificats",
                column: "ZoneProductionId");

            migrationBuilder.CreateIndex(
                name: "IX_Exportateurs_CodeExportateur",
                table: "Exportateurs",
                column: "CodeExportateur",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Exportateurs_DepartementId",
                table: "Exportateurs",
                column: "DepartementId");

            migrationBuilder.CreateIndex(
                name: "IX_Exportateurs_PartenaireId",
                table: "Exportateurs",
                column: "PartenaireId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesCertificats_certificate_id",
                table: "LignesCertificats",
                column: "certificate_id");

            migrationBuilder.CreateIndex(
                name: "IX_LignesCertificats_DeviseId",
                table: "LignesCertificats",
                column: "DeviseId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesCertificats_IncotermId",
                table: "LignesCertificats",
                column: "IncotermId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesCertificats_PositionTarifaireId",
                table: "LignesCertificats",
                column: "PositionTarifaireId");

            migrationBuilder.CreateIndex(
                name: "IX_LignesCertificats_UniteStatistiqueId",
                table: "LignesCertificats",
                column: "UniteStatistiqueId");

            migrationBuilder.CreateIndex(
                name: "IX_Partenaires_CodePartenaire",
                table: "Partenaires",
                column: "CodePartenaire",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Partenaires_DepartementId",
                table: "Partenaires",
                column: "DepartementId");

            migrationBuilder.CreateIndex(
                name: "IX_Partenaires_TypePartenaireId",
                table: "Partenaires",
                column: "TypePartenaireId");

            migrationBuilder.CreateIndex(
                name: "IX_StatutsCertificats_Code",
                table: "StatutsCertificats",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TypesPartenaires_Code",
                table: "TypesPartenaires",
                column: "Code",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_BureauxDedouanements_BureauDedouanementId",
                table: "Certificats",
                column: "BureauDedouanementId",
                principalTable: "BureauxDedouanements",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_CarnetsAdresses_CarnetAdresseId",
                table: "Certificats",
                column: "CarnetAdresseId",
                principalTable: "CarnetsAdresses",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_Devises_DeviseId",
                table: "Certificats",
                column: "DeviseId",
                principalTable: "Devises",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_Exportateurs_ExportateurId",
                table: "Certificats",
                column: "ExportateurId",
                principalTable: "Exportateurs",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_Modules_ModuleId",
                table: "Certificats",
                column: "ModuleId",
                principalTable: "Modules",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_Partenaires_PartenaireId",
                table: "Certificats",
                column: "PartenaireId",
                principalTable: "Partenaires",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_Pays_PaysDestinationId",
                table: "Certificats",
                column: "PaysDestinationId",
                principalTable: "Pays",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_Ports_PortCongoId",
                table: "Certificats",
                column: "PortCongoId",
                principalTable: "Ports",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_Ports_PortSortieId",
                table: "Certificats",
                column: "PortSortieId",
                principalTable: "Ports",
                principalColumn: "id");

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_StatutsCertificats_StatutCertificatId",
                table: "Certificats",
                column: "StatutCertificatId",
                principalTable: "StatutsCertificats",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Certificats_ZonesProductions_ZoneProductionId",
                table: "Certificats",
                column: "ZoneProductionId",
                principalTable: "ZonesProductions",
                principalColumn: "id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_ZonesProductions_Partenaires_PartenaireId",
                table: "ZonesProductions",
                column: "PartenaireId",
                principalTable: "Partenaires",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
