# Renouvellement manuel du token gateway (ops / debug)
# Usage:
#   .\scripts\gateway-token-renew.ps1
#   .\scripts\gateway-token-renew.ps1 -Username admin -Password 'ChangeMe123!' -GatewayUrl http://192.168.2.89:5000

param(
    [string]$GatewayUrl = "http://192.168.2.89:5000",
    [string]$Username = "admin",
    [string]$Password = "ChangeMe123!",
    [switch]$TestOrganisation
)

$loginUrl = "$($GatewayUrl.TrimEnd('/'))/bff/auth/password-login"
$body = @{ username = $Username; password = $Password } | ConvertTo-Json

try {
    $response = Invoke-RestMethod -Uri $loginUrl -Method Post -Body $body -ContentType "application/json" -TimeoutSec 30
}
catch {
    Write-Error "Login gateway échoué : $($_.Exception.Message)"
    exit 1
}

if (-not $response.accessToken) {
    Write-Error "Réponse sans accessToken"
    exit 1
}

$token = $response.accessToken
$preview = if ($token.Length -gt 20) { $token.Substring(0, 8) + "..." + $token.Substring($token.Length - 8) } else { $token }
Write-Host "OK — expiresIn=$($response.expiresInSeconds)s preview=$preview"

if ($TestOrganisation) {
    $headers = @{ Authorization = "Bearer $token" }
    foreach ($type in @("EXPORTATEUR", "PARTENAIRE")) {
        $url = "$($GatewayUrl.TrimEnd('/'))/organisation/Organisations/type/$type"
        try {
            $data = Invoke-RestMethod -Uri $url -Headers $headers -TimeoutSec 30
            $count = if ($data -is [array]) { $data.Count } else { 0 }
            Write-Host "  $type : $count organisation(s)"
        }
        catch {
            Write-Host "  $type : ERREUR $($_.Exception.Message)"
        }
    }
}

Write-Host ""
Write-Host "Pour Portainer (mode manuel, sans auto-refresh) :"
Write-Host "  GATEWAY_BEARER_TOKEN=$token"
Write-Host ""
Write-Host "Recommandé : activer ApiGateway__ServiceAccount dans docker-compose.portainer.yml"
Write-Host "  GATEWAY_SERVICE_USERNAME=$Username"
Write-Host "  GATEWAY_SERVICE_PASSWORD=<mot de passe>"
