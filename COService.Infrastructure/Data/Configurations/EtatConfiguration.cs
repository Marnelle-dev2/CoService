using COService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COService.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité Etat (remplace StatutCertificat).
/// </summary>
public class EtatConfiguration : IEntityTypeConfiguration<Etat>
{
    public void Configure(EntityTypeBuilder<Etat> builder)
    {
        builder.ToTable("Etats");

        builder.HasKey(e => e.Id);

        builder.Property(e => e.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(e => e.Code)
            .HasColumnName("Code")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(e => e.Code)
            .IsUnique()
            .HasDatabaseName("IX_Etats_Code");

        builder.Property(e => e.Libelle)
            .HasColumnName("Libelle")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(e => e.Description)
            .HasColumnName("Description")
            .HasMaxLength(500);

        builder.Property(e => e.CodeEcran)
            .HasColumnName("CodeEcran")
            .HasMaxLength(50);

        // Champs d'audit
        builder.Property(e => e.CreeLe)
            .HasColumnName("CreeLe")
            .HasColumnType("datetime2(7)");

        builder.Property(e => e.CreePar)
            .HasColumnName("CreePar")
            .HasColumnType("nvarchar(max)");

        builder.Property(e => e.ModifierLe)
            .HasColumnName("ModifierLe")
            .HasColumnType("datetime2(7)");

        builder.Property(e => e.ModifiePar)
            .HasColumnName("ModifiePar")
            .HasColumnType("nvarchar(max)");
    }
}
