using COService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COService.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité CertificatLigne (remplace CertificateLine).
/// </summary>
public class CertificatLigneConfiguration : IEntityTypeConfiguration<CertificatLigne>
{
    public void Configure(EntityTypeBuilder<CertificatLigne> builder)
    {
        builder.ToTable("CertificatLignes");

        builder.HasKey(cl => cl.Id);

        builder.Property(cl => cl.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(cl => cl.CertificatId)
            .HasColumnName("CertificatId")
            .IsRequired();

        builder.Property(cl => cl.HSCode)
            .HasColumnName("HSCode")
            .HasMaxLength(50);

        builder.Property(cl => cl.PositionTarifaire)
            .HasColumnName("PositionTarifaire")
            .HasMaxLength(255);

        builder.Property(cl => cl.NatureProduit)
            .HasColumnName("NatureProduit")
            .HasMaxLength(255);

        builder.Property(cl => cl.Quantite)
            .HasColumnName("Quantite")
            .HasMaxLength(50);

        builder.Property(cl => cl.UniteStatistiqueCode)
            .HasColumnName("UniteStatistiqueCode")
            .HasMaxLength(20);

        builder.Property(cl => cl.UniteStatistique)
            .HasColumnName("UniteStatistique")
            .HasMaxLength(100);

        builder.Property(cl => cl.PoidsBrut)
            .HasColumnName("PoidsBrut")
            .HasMaxLength(50);

        builder.Property(cl => cl.PoidsNet)
            .HasColumnName("PoidsNet")
            .HasMaxLength(50);

        builder.Property(cl => cl.ValeurFOB)
            .HasColumnName("ValeurFOB")
            .HasMaxLength(50);

        builder.Property(cl => cl.Volume)
            .HasColumnName("Volume")
            .HasMaxLength(50);

        builder.Property(cl => cl.DeviseCode)
            .HasColumnName("DeviseCode")
            .HasMaxLength(3);

        builder.Property(cl => cl.Devise)
            .HasColumnName("Devise")
            .HasMaxLength(100);

        builder.Property(cl => cl.ProduitCode)
            .HasColumnName("ProduitCode")
            .HasMaxLength(50);

        // Champs d'audit
        builder.Property(cl => cl.CreeLe)
            .HasColumnName("CreeLe")
            .HasColumnType("datetime2(7)");

        builder.Property(cl => cl.CreePar)
            .HasColumnName("CreePar")
            .HasColumnType("nvarchar(max)");

        builder.Property(cl => cl.ModifierLe)
            .HasColumnName("ModifierLe")
            .HasColumnType("datetime2(7)");

        builder.Property(cl => cl.ModifiePar)
            .HasColumnName("ModifiePar")
            .HasColumnType("nvarchar(max)");

        // Relation avec Produit (référentiel local par Code)
        builder.HasOne(cl => cl.Produit)
            .WithMany()
            .HasForeignKey(cl => cl.ProduitCode)
            .HasPrincipalKey(p => p.Code)
            .OnDelete(DeleteBehavior.NoAction);
    }
}
