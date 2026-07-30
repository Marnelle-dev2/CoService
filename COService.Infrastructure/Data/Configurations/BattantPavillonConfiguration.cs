using COService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COService.Infrastructure.Data.Configurations;

/// <summary>
/// Configuration EF Core pour l'entité BattantPavillon (table locale CO).
/// </summary>
public class BattantPavillonConfiguration : IEntityTypeConfiguration<BattantPavillon>
{
    public void Configure(EntityTypeBuilder<BattantPavillon> builder)
    {
        builder.ToTable("BattantsPavillon");

        builder.HasKey(bp => bp.Id);

        builder.Property(bp => bp.Id)
            .HasColumnName("id")
            .IsRequired();

        builder.Property(bp => bp.Code)
            .HasColumnName("Code")
            .HasMaxLength(50)
            .IsRequired();

        builder.HasIndex(bp => bp.Code)
            .IsUnique()
            .HasDatabaseName("IX_BattantsPavillon_Code");

        builder.Property(bp => bp.Designation)
            .HasColumnName("Designation")
            .HasMaxLength(255)
            .IsRequired();

        builder.Property(bp => bp.Actif)
            .HasColumnName("Actif")
            .IsRequired()
            .HasDefaultValue(true);

        // Champs d'audit
        builder.Property(bp => bp.CreeLe)
            .HasColumnName("CreeLe")
            .HasColumnType("datetime2(7)");

        builder.Property(bp => bp.CreePar)
            .HasColumnName("CreePar")
            .HasColumnType("nvarchar(max)");

        builder.Property(bp => bp.ModifierLe)
            .HasColumnName("ModifierLe")
            .HasColumnType("datetime2(7)");

        builder.Property(bp => bp.ModifiePar)
            .HasColumnName("ModifiePar")
            .HasColumnType("nvarchar(max)");
    }
}
