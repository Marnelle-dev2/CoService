using COService.Infrastructure.Sagas;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace COService.Infrastructure.Data.Configurations;

public class CertificatPostValidationStateConfiguration : IEntityTypeConfiguration<CertificatPostValidationState>
{
    public void Configure(EntityTypeBuilder<CertificatPostValidationState> builder)
    {
        builder.ToTable("SagasCertificatPostValidation");
        builder.HasKey(x => x.CorrelationId);

        builder.Property(x => x.CurrentState).IsRequired().HasMaxLength(64);
        builder.Property(x => x.CertificateNo).IsRequired().HasMaxLength(100);
        builder.Property(x => x.NumeroFacture).HasMaxLength(100);
        builder.Property(x => x.PdfUrl).HasMaxLength(500);
        builder.Property(x => x.LastError).HasMaxLength(1000);
        builder.Property(x => x.Version).IsConcurrencyToken();
    }
}
