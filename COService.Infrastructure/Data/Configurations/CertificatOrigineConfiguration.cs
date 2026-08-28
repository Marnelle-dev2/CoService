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

        // Enrôlement (pas de table locale) — colonnes simples
        builder.Property(c => c.ExportateurNIU).HasColumnName("ExportateurNIU").HasMaxLength(50);
        builder.Property(c => c.ExportateurNom).HasColumnName("ExportateurNom").HasMaxLength(255);
        builder.Property(c => c.PartenaireNIU).HasColumnName("PartenaireNIU").HasMaxLength(50);
        builder.Property(c => c.PartenaireNom).HasColumnName("PartenaireNom").HasMaxLength(255);
        builder.Property(c => c.MandataireNIU).HasColumnName("MandataireNIU").HasMaxLength(50);
        builder.Property(c => c.MandataireNom).HasColumnName("MandataireNom").HasMaxLength(255);

        // Référentiel (copie locale, jointure par Code)
        builder.Property(c => c.PaysDestinationCode).HasColumnName("PaysDestinationCode").HasMaxLength(3);
        builder.Property(c => c.PortSortieCode).HasColumnName("PortSortieCode").HasMaxLength(20);
        builder.Property(c => c.PortCongoCode).HasColumnName("PortCongoCode").HasMaxLength(20);
        builder.Property(c => c.AeroportCode).HasColumnName("AeroportCode").HasMaxLength(10);
        builder.Property(c => c.RouteCode).HasColumnName("RouteCode").HasMaxLength(20);
        builder.Property(c => c.CarnetAdresseCode).HasColumnName("CarnetAdresseCode").HasMaxLength(50);
        builder.Property(c => c.ModuleCode).HasColumnName("ModuleCode").HasMaxLength(20);
        builder.Property(c => c.DeviseCode).HasColumnName("DeviseCode").HasMaxLength(3);
        builder.Property(c => c.BureauDedouanementCode).HasColumnName("BureauDedouanementCode").HasMaxLength(10);

        // État
        builder.Property(c => c.EtatCode).HasColumnName("EtatCode").HasMaxLength(50);

        // Interne CO
        builder.Property(c => c.TypeId).HasColumnName("TypeId");
        builder.Property(c => c.ZoneProductionCode).HasColumnName("ZoneProductionCode").HasMaxLength(50);
        builder.Property(c => c.BattantPavillonCode).HasColumnName("BattantPavillonCode").HasMaxLength(50);

        // Paiement (pas de table locale)
        builder.Property(c => c.ModePaiementCode).HasColumnName("ModePaiementCode").HasMaxLength(50);
        builder.Property(c => c.ModePaiement).HasColumnName("ModePaiement").HasMaxLength(255);

        builder.Property(c => c.Formule)
            .HasColumnName("Formule")
            .HasMaxLength(200);

        builder.Property(c => c.Observation)
            .HasColumnName("Observation")
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.Navire)
            .HasColumnName("navire")
            .HasMaxLength(255);

        builder.Property(c => c.AbonnementId)
            .HasColumnName("abonnement_id");

        // MinIO
        builder.Property(c => c.CodeDocument).HasColumnName("CodeDocument").HasMaxLength(255);

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

        // Relations enfants
        builder.HasMany(c => c.CertificatLignes)
            .WithOne(cl => cl.CertificatOrigine)
            .HasForeignKey(cl => cl.CertificatId)
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

        // Relation avec CertificateType (optionnelle)
        builder.HasOne(c => c.Type)
            .WithMany(ct => ct.Certificats)
            .HasForeignKey(c => c.TypeId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relations avec les référentiels — jointure par Code (clé métier, pas de FK GUID)
        builder.HasOne(c => c.PaysDestination)
            .WithMany()
            .HasForeignKey(c => c.PaysDestinationCode)
            .HasPrincipalKey(p => p.Code)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.PortSortie)
            .WithMany()
            .HasForeignKey(c => c.PortSortieCode)
            .HasPrincipalKey(p => p.Code)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.PortCongo)
            .WithMany()
            .HasForeignKey(c => c.PortCongoCode)
            .HasPrincipalKey(p => p.Code)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.Aeroport)
            .WithMany()
            .HasForeignKey(c => c.AeroportCode)
            .HasPrincipalKey(a => a.Code)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.Route)
            .WithMany()
            .HasForeignKey(c => c.RouteCode)
            .HasPrincipalKey(r => r.Code)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.Module)
            .WithMany()
            .HasForeignKey(c => c.ModuleCode)
            .HasPrincipalKey(m => m.Code)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.Devise)
            .WithMany()
            .HasForeignKey(c => c.DeviseCode)
            .HasPrincipalKey(d => d.Code)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.BureauDedouanement)
            .WithMany()
            .HasForeignKey(c => c.BureauDedouanementCode)
            .HasPrincipalKey(b => b.Code)
            .OnDelete(DeleteBehavior.NoAction);

        // CarnetAdresseCode = référence opaque vers MS Référentiel (/api/carnetadresses).
        // Plus de FK locale : le catalogue n'est plus géré dans CO.
        builder.Ignore(c => c.CarnetAdresse);

        // Zone de production : relation bidirectionnelle (ZoneProduction.Certificats)
        builder.HasOne(c => c.ZoneProduction)
            .WithMany(zp => zp.Certificats)
            .HasForeignKey(c => c.ZoneProductionCode)
            .HasPrincipalKey(zp => zp.Code)
            .OnDelete(DeleteBehavior.NoAction);

        builder.HasOne(c => c.BattantPavillon)
            .WithMany(bp => bp.Certificats)
            .HasForeignKey(c => c.BattantPavillonCode)
            .HasPrincipalKey(bp => bp.Code)
            .OnDelete(DeleteBehavior.NoAction);

        // Relation avec Etat
        builder.HasOne(c => c.Etat)
            .WithMany(e => e.Certificats)
            .HasForeignKey(c => c.EtatCode)
            .HasPrincipalKey(e => e.Code)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
