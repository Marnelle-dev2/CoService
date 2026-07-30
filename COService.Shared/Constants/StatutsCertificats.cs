namespace COService.Shared.Constants;

/// <summary>
/// Codes d'état des certificats (copie locale table Etats / référentiel).
/// </summary>
public static class StatutsCertificats
{
    // Statuts pour Certificat d'Origine (CO)
    public const string Elabore = "ELABORE";
    public const string Soumis = "SOUMIS";
    public const string Controle = "CONTROLE";
    public const string Rejete = "REJETE";
    public const string Approuve = "APPROUVE";
    public const string Valide = "VALIDE";
    public const string Modification = "MODIFICATION";

    // Statuts pour Formule A (Ouesso uniquement)
    public const string FormuleASoumise = "FORMULE_A_SOUMISE";
    public const string FormuleAControlee = "FORMULE_A_CONTROLEE";
    public const string FormuleAApprouvee = "FORMULE_A_APPROUVEE";
    public const string FormuleAValidee = "FORMULE_A_VALIDEE";
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
