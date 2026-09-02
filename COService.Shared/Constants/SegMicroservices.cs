namespace COService.Shared.Constants;

/// <summary>
/// Référentiel officiel des microservices SEG (ports et préfixes gateway).
/// </summary>
public static class SegMicroservices
{
    public const string GatewayPrefix = "gateway";
    public const int GatewayPort = 5000;

    public const string ReferentielApplication = "ReferentielService";
    public const string ReferentielPrefix = "referentiel";
    public const int ReferentielPort = 8290;

    public const string EnrolementApplication = "EnrolementService";
    public const string EnrolementPrefix = "enrolement";

    public const string ActeursApplication = "ActeursService";
    public const string ActeursPrefix = "acteur";
    public const int ActeursPort = 8300;

    public const string DeclarationImportApplication = "DeclarationImportService";
    public const string DeclarationImportPrefix = "declaration-import";
    public const int DeclarationImportPort = 5460;

    public const string DeclarationExportApplication = "DeclarationExportService";
    public const string DeclarationExportPrefix = "declaration-export";
    public const int DeclarationExportPort = 8302;

    public const string CertificatOrigineApplication = "CoService";
    public const string CertificatOriginePrefix = "cert-origine";
    public const int CertificatOriginePort = 8700;

    public const string DocumentsApplication = "DocumentsService";
    public const string DocumentsPrefix = "document";
    public const int DocumentsPort = 9010;

    public const string FacturationApplication = "FacturationService";
    public const string FacturationPrefix = "facturation";
    public const int FacturationPort = 8081;

    public const string DefaultActeursBaseUrl = "http://srv-guot-cont.gumar.local:8300";
    public const string DefaultReferentielBaseUrl = "http://srv-guot-cont.gumar.local:8290";
    public const string DefaultGatewayBaseUrl = "http://srv-guot-cont.gumar.local:5000";
}
