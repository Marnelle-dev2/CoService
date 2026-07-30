using AutoMapper;
using COService.Application.DTOs;
using COService.Domain.Entities;

namespace COService.Application.Mappings;

/// <summary>
/// Profil AutoMapper pour les mappings entre Domain et DTOs
/// </summary>
public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // CertificatOrigine
        CreateMap<CertificatOrigine, CertificatOrigineDto>()
            .ForMember(dest => dest.PaysDestinationNom, opt => opt.MapFrom(src => src.PaysDestination != null ? src.PaysDestination.Nom : null))
            .ForMember(dest => dest.PortSortieNom, opt => opt.MapFrom(src => src.PortSortie != null ? src.PortSortie.Nom : null))
            .ForMember(dest => dest.PortCongoNom, opt => opt.MapFrom(src => src.PortCongo != null ? src.PortCongo.Nom : null))
            .ForMember(dest => dest.EtatLibelle, opt => opt.MapFrom(src => src.Etat != null ? src.Etat.Libelle : null))
            .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type != null ? src.Type.Designation : null))
            .ForMember(dest => dest.CertificatLignes, opt => opt.MapFrom(src => src.CertificatLignes));

        CreateMap<CreerCertificatOrigineDto, CertificatOrigine>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CertificateNo, opt => opt.MapFrom(src => src.CertificateNo))
            .ForMember(dest => dest.EtatCode, opt => opt.Ignore()) // Sera défini par le service
            .ForMember(dest => dest.Etat, opt => opt.Ignore())
            .ForMember(dest => dest.CreeLe, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CertificatLignes, opt => opt.Ignore())
            .ForMember(dest => dest.CertificateValidations, opt => opt.Ignore())
            .ForMember(dest => dest.Commentaires, opt => opt.Ignore())
            .ForMember(dest => dest.Abonnement, opt => opt.Ignore())
            // Ignorer les propriétés de navigation (seuls les codes sont mappés)
            .ForMember(dest => dest.PaysDestination, opt => opt.Ignore())
            .ForMember(dest => dest.PortSortie, opt => opt.Ignore())
            .ForMember(dest => dest.PortCongo, opt => opt.Ignore())
            .ForMember(dest => dest.Aeroport, opt => opt.Ignore())
            .ForMember(dest => dest.Route, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.ZoneProduction, opt => opt.Ignore())
            .ForMember(dest => dest.BattantPavillon, opt => opt.Ignore())
            .ForMember(dest => dest.BureauDedouanement, opt => opt.Ignore())
            .ForMember(dest => dest.Module, opt => opt.Ignore())
            .ForMember(dest => dest.Devise, opt => opt.Ignore())
            .ForMember(dest => dest.CarnetAdresse, opt => opt.Ignore());

        CreateMap<ModifierCertificatOrigineDto, CertificatOrigine>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CertificateNo, opt => opt.Ignore())
            .ForMember(dest => dest.EtatCode, opt => opt.Ignore())
            .ForMember(dest => dest.Etat, opt => opt.Ignore())
            .ForMember(dest => dest.CreeLe, opt => opt.Ignore())
            .ForMember(dest => dest.CreePar, opt => opt.Ignore())
            .ForMember(dest => dest.ModifierLe, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CertificatLignes, opt => opt.Ignore())
            .ForMember(dest => dest.CertificateValidations, opt => opt.Ignore())
            .ForMember(dest => dest.Commentaires, opt => opt.Ignore())
            .ForMember(dest => dest.Abonnement, opt => opt.Ignore())
            // Ignorer les propriétés de navigation (seuls les codes sont mappés)
            .ForMember(dest => dest.PaysDestination, opt => opt.Ignore())
            .ForMember(dest => dest.PortSortie, opt => opt.Ignore())
            .ForMember(dest => dest.PortCongo, opt => opt.Ignore())
            .ForMember(dest => dest.Aeroport, opt => opt.Ignore())
            .ForMember(dest => dest.Route, opt => opt.Ignore())
            .ForMember(dest => dest.Type, opt => opt.Ignore())
            .ForMember(dest => dest.ZoneProduction, opt => opt.Ignore())
            .ForMember(dest => dest.BattantPavillon, opt => opt.Ignore())
            .ForMember(dest => dest.BureauDedouanement, opt => opt.Ignore())
            .ForMember(dest => dest.Module, opt => opt.Ignore())
            .ForMember(dest => dest.Devise, opt => opt.Ignore())
            .ForMember(dest => dest.CarnetAdresse, opt => opt.Ignore());

        // CertificatLigne
        CreateMap<CertificatLigne, CertificatLigneDto>();
        CreateMap<CreerCertificatLigneDto, CertificatLigne>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CertificatId, opt => opt.Ignore())
            .ForMember(dest => dest.Produit, opt => opt.Ignore())
            .ForMember(dest => dest.CreeLe, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CertificatOrigine, opt => opt.Ignore());

        CreateMap<ModifierCertificatLigneDto, CertificatLigne>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CertificatId, opt => opt.Ignore())
            .ForMember(dest => dest.Produit, opt => opt.Ignore())
            .ForMember(dest => dest.CreeLe, opt => opt.Ignore())
            .ForMember(dest => dest.CreePar, opt => opt.Ignore())
            .ForMember(dest => dest.ModifierLe, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CertificatOrigine, opt => opt.Ignore());

        // Abonnement
        CreateMap<Abonnement, AbonnementDto>()
            .ForMember(dest => dest.NombreCertificats, opt => opt.MapFrom(src => src.Certificats.Count));

        CreateMap<CreerAbonnementDto, Abonnement>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CreeLe, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Certificats, opt => opt.Ignore());

        CreateMap<ModifierAbonnementDto, Abonnement>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreeLe, opt => opt.Ignore())
            .ForMember(dest => dest.CreePar, opt => opt.Ignore())
            .ForMember(dest => dest.ModifierLe, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Certificats, opt => opt.Ignore());

        // Commentaire
        CreateMap<Commentaire, CommentaireDto>();
        CreateMap<CreerCommentaireDto, Commentaire>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CertificateId, opt => opt.Ignore())
            .ForMember(dest => dest.CreeLe, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CertificatOrigine, opt => opt.Ignore());

        CreateMap<ModifierCommentaireDto, Commentaire>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CertificateId, opt => opt.Ignore())
            .ForMember(dest => dest.CreeLe, opt => opt.Ignore())
            .ForMember(dest => dest.CreePar, opt => opt.Ignore())
            .ForMember(dest => dest.ModifierLe, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.CertificatOrigine, opt => opt.Ignore());

        // CertificateValidation
        CreateMap<CertificateValidation, CertificateValidationDto>()
            .ForMember(dest => dest.Etape, opt => opt.MapFrom(src => src.Etape.ToString()))
            .ForMember(dest => dest.RoleVisa, opt => opt.MapFrom(src => src.RoleVisa.ToString()));

        // CertificateType
        CreateMap<CertificateType, CertificateTypeDto>()
            .ForMember(dest => dest.NombreCertificats, opt => opt.MapFrom(src => src.Certificats.Count));

        CreateMap<CreerCertificateTypeDto, CertificateType>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CreeLe, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Certificats, opt => opt.Ignore());

        CreateMap<ModifierCertificateTypeDto, CertificateType>()
            .ForMember(dest => dest.Id, opt => opt.Ignore())
            .ForMember(dest => dest.CreeLe, opt => opt.Ignore())
            .ForMember(dest => dest.CreePar, opt => opt.Ignore())
            .ForMember(dest => dest.ModifierLe, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Certificats, opt => opt.Ignore());

        // Etat
        CreateMap<Etat, EtatDto>();

        // ZoneProduction
        CreateMap<ZoneProduction, ZoneProductionDto>();
        CreateMap<CreerZoneProductionDto, ZoneProduction>()
            .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.NewGuid()))
            .ForMember(dest => dest.CreeLe, opt => opt.MapFrom(src => DateTime.UtcNow))
            .ForMember(dest => dest.Certificats, opt => opt.Ignore());
    }
}
