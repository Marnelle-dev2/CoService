using COService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COService.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité CarnetAdresse.
/// </summary>
public class CarnetAdresseConfiguration : IEntityTypeConfiguration<CarnetAdresse>
{
    public void Configure(EntityTypeBuilder<CarnetAdresse> builder)
    {
        builder.ToTable("CarnetsAdresses");

        builder.HasKey(c => c.Id);

        builder.Property(c => c.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(c => c.Code)
            .HasColumnName("Code")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(c => c.Code)
            .IsUnique()
            .HasDatabaseName("IX_CarnetsAdresses_Code");

        builder.Property(c => c.Nom)
            .HasColumnName("Nom")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(c => c.RaisonSociale)
            .HasColumnName("RaisonSociale")
            .HasMaxLength(255);

        builder.Property(c => c.Coordonnees)
            .HasColumnName("Coordonnees")
            .HasMaxLength(255);

        builder.Property(c => c.Adresse)
            .HasColumnName("Adresse")
            .HasMaxLength(500);

        // Champs d'audit
        builder.Property(c => c.CreeLe)
            .HasColumnName("CreeLe")
            .HasColumnType("datetime2(7)");

        builder.Property(c => c.CreePar)
            .HasColumnName("CreePar")
            .HasColumnType("nvarchar(max)");

        builder.Property(c => c.ModifierLe)
            .HasColumnName("ModifierLe")
            .HasColumnType("datetime2(7)");

        builder.Property(c => c.ModifiePar)
            .HasColumnName("ModifiePar")
            .HasColumnType("nvarchar(max)");

        // Index pour les recherches fréquentes
        builder.HasIndex(c => c.Nom)
            .HasDatabaseName("IX_CarnetsAdresses_Nom");
    }
}

