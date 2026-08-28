namespace COService.Shared.Constants;

/// <summary>
/// Codes d'état des certificats (alignés V2 / ReferentielService).
/// Codes numériques = noyau commun SEG. Codes CO_* = domaine CERTIFICAT_ORIGINE
/// tant que le référentiel ne les publie pas encore.
/// </summary>
public static class StatutsCertificats
{
    // Noyau commun V2
    public const string Elabore = "42";
    public const string Soumis = "79";          // Visa demandé
    public const string Valide = "50";          // Ouvert / validé
    public const string Modification = "66";    // Modification demandée
    public const string ModificationSoumise = "68";
    public const string Rejete = "80";          // Visas refusés
    public const string Annule = "51";
    public const string Cloture = "52";

    // Domaine CERTIFICAT_ORIGINE (provisoires jusqu'à alimentation Ref)
    public const string Controle = "CO43";
    public const string Approuve = "45";        // Controller / CO approuvé (V2)
    public const string FormuleASoumise = "CO_FA_SOUMISE";
    public const string FormuleAControlee = "CO_FA_CONTROLEE";
    public const string FormuleAApprouvee = "CO_FA_APPROUVEE";
    public const string FormuleAValidee = "CO_FA_VALIDEE";

    public static class Domaines
    {
        public const string Commun = "COMMUN";
        public const string CertificatOrigine = "CERTIFICAT_ORIGINE";
    }

    public static class Types
    {
        public const string Metier = "METIER";
    }
}

/// <summary>
/// Constantes pour les rôles utilisateurs
/// Ces valeurs correspondent aux rôles dans le microservice d'authentification
/// </summary>
public static class RolesUtilisateurs
{
    public const string Controleur = "3";
    public const string Superviseur = "4";
    public const string President = "6";
    public const string Exportateur = "84";
}
