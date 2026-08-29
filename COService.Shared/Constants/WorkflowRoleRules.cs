namespace COService.Shared.Constants;

/// <summary>
/// Règles de rôle par étape du workflow CO (GECO : contrôleur → superviseur → président).
/// </summary>
public static class WorkflowRoleRules
{
    public static bool PeutControler(IReadOnlyCollection<string> roles)
        => roles.Contains(RolesUtilisateurs.Controleur);

    public static bool PeutApprouver(IReadOnlyCollection<string> roles)
        => roles.Contains(RolesUtilisateurs.Superviseur);

    public static bool PeutValiderFinal(IReadOnlyCollection<string> roles)
        => roles.Contains(RolesUtilisateurs.President);

    public static bool PeutRejeter(string? etatCode, IReadOnlyCollection<string> roles)
    {
        var code = StatutsCertificats.NormaliserCodeEtat(etatCode);
        return code switch
        {
            StatutsCertificats.Soumis => PeutControler(roles),
            StatutsCertificats.Controle => PeutApprouver(roles),
            StatutsCertificats.Approuve => PeutValiderFinal(roles),
            _ => false
        };
    }
}
