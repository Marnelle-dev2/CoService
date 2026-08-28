namespace COService.Shared.Constants;

/// <summary>
/// Constantes pour identifier les chambres de commerce
/// Utilise les codes uniques (CodePartenaire ou Code Département) comme identifiants principaux
/// 
/// ⚠️ ATTENTION : Valeurs temporaires
/// Ces valeurs sont temporaires et seront remplacées par les vraies valeurs
/// une fois que la synchronisation avec le microservice Enrolement sera opérationnelle.
/// 
/// Voir VALEURS_TEMPORAIRES_CHAMBRES_COMMERCE.md pour plus de détails.
/// </summary>
public static class ChambresCommerce
{
    /// <summary>
    /// Chambre de Commerce de Pointe-Noire
    /// </summary>
    public static class PointeNoire
    {
        /// <summary>
        /// Code unique du partenaire (clé unique dans la table Partenaires)
        /// Utilisé comme identifiant principal pour identifier cette chambre
        /// </summary>
        public const string CodePartenaire = "CCIAM-PNR";

        /// <summary>
        /// Code département (clé unique dans la table Departements)
        /// Utilisé dans la génération des numéros de certificats
        /// 
        /// ⚠️ VALEUR TEMPORAIRE : "PNR"
        /// À remplacer par la vraie valeur après synchronisation avec le référentiel global
        /// </summary>
        public const string CodeDepartement = "PNR";

        /// <summary>
        /// Nom de la chambre de commerce
        /// </summary>
        public const string Nom = "Chambre de Commerce de Pointe-Noire";
    }

    /// <summary>
    /// Chambre de Commerce d'Ouesso
    /// </summary>
    public static class Ouesso
    {
        /// <summary>
        /// Code unique du partenaire (clé unique dans la table Partenaires)
        /// Utilisé comme identifiant principal pour identifier cette chambre
        /// </summary>
        public const string CodePartenaire = "CCIAM-OUE";

        /// <summary>
        /// Code département (clé unique dans la table Departements)
        /// Utilisé dans la génération des numéros de certificats
        /// 
        /// ⚠️ VALEUR TEMPORAIRE : "OUE"
        /// À remplacer par la vraie valeur après synchronisation avec le référentiel global
        /// </summary>
        public const string CodeDepartement = "OUE";

        /// <summary>
        /// Nom de la chambre de commerce
        /// </summary>
        public const string Nom = "Chambre de Commerce d'Ouesso";
    }

    /// <summary>
    /// Méthode helper pour identifier une chambre de commerce par son code partenaire
    /// </summary>
    /// <param name="codePartenaire">Code du partenaire (clé unique, ex: "PNR", "OUE")</param>
    /// <returns>Le nom de la chambre de commerce ou null si inconnu</returns>
    public static string? GetChambreByCodePartenaire(string? codePartenaire)
    {
        return codePartenaire?.ToUpperInvariant() switch
        {
            PointeNoire.CodePartenaire => PointeNoire.Nom,
            Ouesso.CodePartenaire => Ouesso.Nom,
            _ => null
        };
    }

    /// <summary>
    /// Méthode helper pour identifier une chambre de commerce par son code département
    /// </summary>
    /// <param name="codeDepartement">Code du département (clé unique, ex: "PNR", "OUE")</param>
    /// <returns>Le nom de la chambre de commerce ou null si inconnu</returns>
    public static string? GetChambreByCodeDepartement(string? codeDepartement)
    {
        return codeDepartement?.ToUpperInvariant() switch
        {
            PointeNoire.CodeDepartement => PointeNoire.Nom,
            Ouesso.CodeDepartement => Ouesso.Nom,
            _ => null
        };
    }

    /// <summary>
    /// Vérifie si un code partenaire correspond à Pointe-Noire
    /// </summary>
    public static bool EstPointeNoire(string? codePartenaire)
    {
        return string.Equals(codePartenaire, PointeNoire.CodePartenaire, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Vérifie si un code partenaire correspond à Ouesso
    /// </summary>
    public static bool EstOuesso(string? codePartenaire)
    {
        return string.Equals(codePartenaire, Ouesso.CodePartenaire, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Vérifie si un code département correspond à Pointe-Noire
    /// </summary>
    public static bool EstPointeNoireParDepartement(string? codeDepartement)
    {
        return string.Equals(codeDepartement, PointeNoire.CodeDepartement, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Vérifie si un code département correspond à Ouesso
    /// </summary>
    public static bool EstOuessoParDepartement(string? codeDepartement)
    {
        return string.Equals(codeDepartement, Ouesso.CodeDepartement, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Résout le code département pour la numérotation (GECO : PNR, OUE…).
    /// Accepte les codes legacy CCIAM-* et les codes Organisation (SEG…).
    /// </summary>
    public static string? ResolveCodeDepartement(string? codePartenaire, string? nomPartenaire = null)
    {
        if (EstPointeNoire(codePartenaire))
        {
            return PointeNoire.CodeDepartement;
        }

        if (EstOuesso(codePartenaire))
        {
            return Ouesso.CodeDepartement;
        }

        var haystack = $"{codePartenaire} {nomPartenaire}".ToUpperInvariant();
        if (haystack.Contains("OUESSO") || haystack.Contains("-OUE"))
        {
            return Ouesso.CodeDepartement;
        }

        if (haystack.Contains("POINTE") || haystack.Contains("PNR")
            || haystack.Contains("CHAMBRE") || haystack.Contains("CCI") || haystack.Contains("COMMERCE"))
        {
            return PointeNoire.CodeDepartement;
        }

        return null;
    }

    /// <summary>
    /// Code partenaire normalisé pour le routage workflow (Pointe-Noire / Ouesso).
    /// </summary>
    public static string ResolveWorkflowPartenaireCode(string? codePartenaire, string? nomPartenaire = null)
    {
        var dept = ResolveCodeDepartement(codePartenaire, nomPartenaire);
        return dept switch
        {
            var d when string.Equals(d, Ouesso.CodeDepartement, StringComparison.OrdinalIgnoreCase) => Ouesso.CodePartenaire,
            _ => PointeNoire.CodePartenaire
        };
    }
}
