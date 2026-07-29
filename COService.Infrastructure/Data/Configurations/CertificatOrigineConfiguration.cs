using COService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COService.Infrastructure.Data.Configurations;

public class CertificatOrigineConfiguration : IEntityTypeConfiguration<CertificatOrigine>
{
    public void Configure(EntityTypeBuilder<CertificatOrigine> builder)
    {
        builder.ToTable("Certificats");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(c => c.CertificateNo)
            .HasColumnName("CertificateNo")
            .HasMaxLength(255)
            .IsRequired();

        builder.HasIndex(c => c.CertificateNo)
            .IsUnique()
            .HasDatabaseName("IX_Certificats_CertificateNo");

        builder.Property(c => c.ExportateurId)
            .HasColumnName("ExportateurId");

        builder.Property(c => c.PartenaireId)
            .HasColumnName("PartenaireId");

        builder.Property(c => c.PaysDestinationId)
            .HasColumnName("PaysDestinationId");

        builder.Property(c => c.PortSortieId)
            .HasColumnName("PortSortieId");

        builder.Property(c => c.PortCongoId)
            .HasColumnName("PortCongoId");

        builder.Property(c => c.ZoneProductionId)
            .HasColumnName("ZoneProductionId");

        builder.Property(c => c.BureauDedouanementId)
            .HasColumnName("BureauDedouanementId");

        builder.Property(c => c.ModuleId)
            .HasColumnName("ModuleId");

        builder.Property(c => c.DeviseId)
            .HasColumnName("DeviseId");

        builder.Property(c => c.TypeId)
            .HasColumnName("TypeId");

        // Relation avec CertificateType (optionnelle)
        builder.HasOne(c => c.Type)
            .WithMany(ct => ct.Certificats)
            .HasForeignKey(c => c.TypeId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Property(c => c.Formule)
            .HasColumnName("Formule")
            .HasMaxLength(200);

        builder.Property(c => c.Mandataire)
            .HasColumnName("Mandataire")
            .HasMaxLength(200);

        builder.Property(c => c.StatutCertificatId)
            .HasColumnName("StatutCertificatId");

        builder.Property(c => c.Observation)
            .HasColumnName("Observation")
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.CarnetAdresseId)
            .HasColumnName("CarnetAdresseId");

        builder.Property(c => c.Navire)
            .HasColumnName("navire")
            .HasMaxLength(255);

        builder.Property(c => c.DocumentsId)
            .HasColumnName("documents_id");

        builder.Property(c => c.AbonnementId)
            .HasColumnName("abonnement_id");

        // Colonnes MinIO pas encore migrées en base — ignorées temporairement
        builder.Ignore(c => c.FactureUrl);
        builder.Ignore(c => c.PiecesJustificativesUrls);
        builder.Ignore(c => c.CertificatGenereUrl);

        // Champs d'audit
        builder.Property(c => c.CreeLe)
            .HasColumnName("CreeLe")
            .HasColumnType("datetime2(7)")
            .IsRequired();

        builder.Property(c => c.CreePar)
            .HasColumnName("CreePar")
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.ModifierLe)
            .HasColumnName("ModifierLe")
            .HasColumnType("datetime2(7)");

        builder.Property(c => c.ModifiePar)
            .HasColumnName("ModifiePar")
            .HasColumnType("nvarchar(max)");

        // Relations
        builder.HasMany(c => c.CertificateLines)
            .WithOne(cl => cl.CertificatOrigine)
            .HasForeignKey(cl => cl.CertificateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.CertificateValidations)
            .WithOne(cv => cv.CertificatOrigine)
            .HasForeignKey(cv => cv.CertificateId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Commentaires)
            .WithOne(com => com.CertificatOrigine)
            .HasForeignKey(com => com.CertificateId)
            .OnDelete(DeleteBehavior.Cascade);

        // Relation avec Abonnement (optionnelle, un certificat peut ne pas avoir d'abonnement)
        builder.HasOne(c => c.Abonnement)
            .WithMany(a => a.Certificats)
            .HasForeignKey(c => c.AbonnementId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relations avec les référentiels
        // Exportateur : relation bidirectionnelle (Exportateur.Certificats)
        builder.HasOne(c => c.Exportateur)
            .WithMany(e => e.Certificats)
            .HasForeignKey(c => c.ExportateurId)
            .OnDelete(DeleteBehavior.SetNull);

        // Partenaire / Chambre de Commerce : relation bidirectionnelle (Partenaire.Certificats)
        builder.HasOne(c => c.Partenaire)
            .WithMany(p => p.Certificats)
            .HasForeignKey(c => c.PartenaireId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.PaysDestination)
            .WithMany()
            .HasForeignKey(c => c.PaysDestinationId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.PortSortie)
            .WithMany()
            .HasForeignKey(c => c.PortSortieId)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.PortCongo)
            .WithMany()
            .HasForeignKey(c => c.PortCongoId)
            .OnDelete(DeleteBehavior.NoAction);

        // Zone de production : relation bidirectionnelle (ZoneProduction.Certificats)
        builder.HasOne(c => c.ZoneProduction)
            .WithMany(zp => zp.Certificats)
            .HasForeignKey(c => c.ZoneProductionId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.BureauDedouanement)
            .WithMany()
            .HasForeignKey(c => c.BureauDedouanementId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Module)
            .WithMany()
            .HasForeignKey(c => c.ModuleId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.Devise)
            .WithMany()
            .HasForeignKey(c => c.DeviseId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(c => c.CarnetAdresse)
            .WithMany(ca => ca.Certificats)
            .HasForeignKey(c => c.CarnetAdresseId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relation avec StatutCertificat
        builder.HasOne(c => c.StatutCertificat)
            .WithMany(s => s.Certificats)
            .HasForeignKey(c => c.StatutCertificatId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}

