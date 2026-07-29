using COService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace COService.Infrastructure.Data;

/// <summary>
/// DbContext pour le microservice COService
/// </summary>
public class COServiceDbContext : DbContext
{
    public COServiceDbContext(DbContextOptions<COServiceDbContext> options)
        : base(options)
    {
    }

    public DbSet<CertificatOrigine> CertificatsOrigine { get; set; }
    public DbSet<CertificateLine> CertificateLines { get; set; }
    public DbSet<CertificateValidation> CertificateValidations { get; set; }
    public DbSet<Commentaire> Commentaires { get; set; }
    public DbSet<Abonnement> Abonnements { get; set; }
    public DbSet<CertificateType> CertificateTypes { get; set; }
    
    // Organisations (synchronisées depuis Enrolement)
    public DbSet<Partenaire> Partenaires { get; set; }
    public DbSet<Exportateur> Exportateurs { get; set; }
    
    // Référentiels (synchronisés depuis Référentiel global)
    public DbSet<Departement> Departements { get; set; }
    public DbSet<Pays> Pays { get; set; }
    public DbSet<Port> Ports { get; set; }
    public DbSet<Aeroport> Aeroports { get; set; }
    public DbSet<Module> Modules { get; set; }
    public DbSet<Devise> Devises { get; set; }
    public DbSet<TauxDeChange> TauxDeChanges { get; set; }
    public DbSet<Incoterm> Incoterms { get; set; }
    public DbSet<Corridor> Corridors { get; set; }
    public DbSet<RouteNationale> RoutesNationales { get; set; }
    public DbSet<Troncon> Troncons { get; set; }
    public DbSet<Section> Sections { get; set; }
    public DbSet<BureauDedouanement> BureauxDedouanements { get; set; }
    public DbSet<SectionTarifaire> SectionsTariffaires { get; set; }
    public DbSet<ChapitreTarifaire> ChapitresTariffaires { get; set; }
    public DbSet<DivisionTarifaire> DivisionsTariffaires { get; set; }
    public DbSet<CategorieTarifaire> CategoriesTariffaires { get; set; }
    public DbSet<PositionTarifaire> PositionsTariffaires { get; set; }
    public DbSet<UniteStatistique> UniteStatistiques { get; set; }
    
    // Entités propres au CO
    public DbSet<ZoneProduction> ZonesProductions { get; set; }
    public DbSet<TypePartenaire> TypesPartenaires { get; set; }
    public DbSet<CarnetAdresse> CarnetsAdresses { get; set; }
    public DbSet<StatutCertificat> StatutsCertificats { get; set; }

    /// <summary>États des sagas MassTransit (post-validation CO).</summary>
    public DbSet<COService.Infrastructure.Sagas.CertificatPostValidationState> SagasCertificatPostValidation { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Appliquer toutes les configurations depuis l'assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(COServiceDbContext).Assembly);
    }
}

