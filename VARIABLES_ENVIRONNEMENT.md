# Guide des Variables d'Environnement pour COService

## 📝 Format des Variables d'Environnement dans .NET

Dans .NET, les variables d'environnement utilisent le **double underscore** (`__`) pour représenter les sections imbriquées dans `appsettings.json`.

### Exemple : ConnectionStrings

Dans `appsettings.json` :
```json
{
  "ConnectionStrings": {
    "chaine": "Server=..."
  }
}
```

En variable d'environnement, cela devient :
```env
ConnectionStrings__chaine=Server=...
```

**Note** : Le double underscore (`__`) remplace le point (`.`) dans la hiérarchie JSON.

---

## 🔧 Configuration dans Portainer

### Option 1 : Variables d'environnement simples

Dans Portainer, lors de la création de la stack, ajoutez dans **Environment variables** :

| Nom de la variable | Valeur |
|-------------------|--------|
| `ConnectionStrings__chaine` | `Server=192.168.2.118;Database=GUOT_TE_PROD;User ID=msuser;Password=9$SViSWexRn5hWq;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;` |
| `Consul__Enabled` | `true` |
| `Consul__Address` | `http://srv-guot-cont.gumar.local:8500` |
| `ASPNETCORE_ENVIRONMENT` | `Production` |
| `GATEWAY_BEARER_TOKEN` | JWT Auth (sans préfixe `Bearer `) — exportateurs / chambres via gateway |
| `ApiGateway__BearerToken` | même JWT (si tu ne passes pas par `GATEWAY_BEARER_TOKEN`) |

### Token Organisation (sans rebuild)

Le client Organisation lit `ApiGateway__BearerToken` ou `ExternalServices__EnrolementService__BearerToken`.

Quand le JWT expire :
1. Portainer → stack **coservice** → **Editor** / Environment
2. Mets à jour `GATEWAY_BEARER_TOKEN` (ou `ApiGateway__BearerToken`)
3. **Update the stack** / Restart du conteneur

Pas besoin de rebuild ni de `docker push` — uniquement redémarrer le conteneur pour recharger l’env.

### Option 2 : Via docker-compose.yml

Dans le fichier `docker-compose.yml`, les variables sont définies ainsi :

```yaml
environment:
  - ConnectionStrings__chaine=${DB_CONNECTION_STRING}
  - Consul__Enabled=${CONSUL_ENABLED:-true}
  - Consul__Address=${CONSUL_ADDRESS:-http://srv-guot-cont.gumar.local:8500}
```

Puis dans Portainer, vous définissez :
- `DB_CONNECTION_STRING` = votre chaîne de connexion complète
- `CONSUL_ENABLED` = `true` ou `false`
- `CONSUL_ADDRESS` = l'adresse de Consul

---

## 📋 Liste complète des variables d'environnement

### Variables obligatoires

```env
# Base de données (OBLIGATOIRE)
ConnectionStrings__chaine=Server=192.168.2.118;Database=GUOT_TE_PROD;User ID=msuser;Password=9$SViSWexRn5hWq;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;
```

### Variables optionnelles

```env
# Application
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8700

# Consul
Consul__Enabled=true
Consul__Address=http://srv-guot-cont.gumar.local:8500
Consul__ServiceName=coservice
Consul__ServiceId=coservice-1
Consul__ServiceAddress=http://coservice:8700
Consul__HealthCheck__Endpoint=/sante
Consul__HealthCheck__Interval=10
Consul__HealthCheck__Timeout=5
Consul__HealthCheck__DeregisterCriticalServiceAfter=30
```

---

## 🎯 Exemples concrets

### Exemple 1 : Configuration minimale

Dans Portainer, ajoutez seulement :

```
ConnectionStrings__chaine=Server=192.168.2.118;Database=GUOT_TE_PROD;User ID=msuser;Password=9$SViSWexRn5hWq;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;
```

### Exemple 2 : Configuration complète

```
ConnectionStrings__chaine=Server=192.168.2.118;Database=GUOT_TE_PROD;User ID=msuser;Password=9$SViSWexRn5hWq;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;
Consul__Enabled=true
Consul__Address=http://srv-guot-cont.gumar.local:8500
ASPNETCORE_ENVIRONMENT=Production
```

### Exemple 3 : Désactiver Consul

```
ConnectionStrings__chaine=Server=192.168.2.118;Database=GUOT_TE_PROD;User ID=msuser;Password=9$SViSWexRn5hWq;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;
Consul__Enabled=false
```

---

## 🔍 Vérification dans Portainer

### Comment ajouter les variables

1. Dans Portainer, allez dans **Stacks** → votre stack `coservice`
2. Cliquez sur **Editor**
3. Dans la section **Environment variables**, ajoutez :

```
Name: ConnectionStrings__chaine
Value: Server=192.168.2.118;Database=GUOT_TE_PROD;User ID=msuser;Password=9$SViSWexRn5hWq;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;
```

4. Cliquez sur **Update the stack**

### Format dans l'éditeur Portainer

```
ConnectionStrings__chaine | Server=192.168.2.118;Database=GUOT_TE_PROD;User ID=msuser;Password=9$SViSWexRn5hWq;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;
Consul__Enabled | true
Consul__Address | http://srv-guot-cont.gumar.local:8500
```

---

## ⚠️ Points importants

### 1. Double underscore obligatoire

❌ **FAUX** :
```
ConnectionStrings.chaine=...
ConnectionStrings_chaine=...
```

✅ **CORRECT** :
```
ConnectionStrings__chaine=...
```

### 2. Sensibilité à la casse

Les noms de variables sont **sensibles à la casse** :
- ✅ `ConnectionStrings__chaine` (correct)
- ❌ `connectionstrings__chaine` (incorrect)
- ❌ `CONNECTIONSTRINGS__CHAINE` (incorrect)

### 3. Caractères spéciaux dans les valeurs

Si votre mot de passe contient des caractères spéciaux, utilisez des guillemets dans docker-compose.yml :

```yaml
environment:
  - ConnectionStrings__chaine="Server=...;Password=9$SViSWexRn5hWq;..."
```

Dans Portainer, vous pouvez directement coller la valeur sans guillemets.

---

## 🧪 Test de la configuration

### Vérifier que la variable est bien lue

1. Dans Portainer, allez dans **Containers** → `coservice`
2. Cliquez sur **Console**
3. Exécutez :
```bash
env | grep ConnectionStrings
```

Vous devriez voir :
```
ConnectionStrings__chaine=Server=192.168.2.118;...
```

### Vérifier dans les logs

Les logs de l'application doivent afficher :
```
info: Microsoft.EntityFrameworkCore.Database.Command[20101]
      Executed DbCommand ...
```

Si vous voyez une erreur de connexion, vérifiez la variable d'environnement.

---

## 📝 Template pour Portainer

Copiez-collez ce template dans Portainer :

```
ConnectionStrings__chaine=Server=VOTRE_SERVEUR;Database=VOTRE_BASE;User ID=VOTRE_USER;Password=VOTRE_PASSWORD;Connect Timeout=30;Encrypt=False;Trust Server Certificate=True;
Consul__Enabled=true
Consul__Address=http://srv-guot-cont.gumar.local:8500
ASPNETCORE_ENVIRONMENT=Production
```

Remplacez :
- `VOTRE_SERVEUR` par l'adresse IP ou le nom de votre serveur SQL
- `VOTRE_BASE` par le nom de votre base de données
- `VOTRE_USER` par votre utilisateur SQL
- `VOTRE_PASSWORD` par votre mot de passe SQL

---

## 🔐 Sécurité : Utiliser les secrets Portainer

Pour plus de sécurité, utilisez les secrets de Portainer :

1. Dans Portainer, allez dans **Secrets**
2. Créez un secret nommé `db_connection_string`
3. Valeur : votre chaîne de connexion complète
4. Dans `docker-compose.yml`, référencez-le :

```yaml
environment:
  - ConnectionStrings__chaine=/run/secrets/db_connection_string
secrets:
  - db_connection_string
```

---

## ❓ FAQ

### Q : Pourquoi double underscore ?

R : C'est la convention .NET pour mapper les variables d'environnement aux sections imbriquées de `appsettings.json`.

### Q : Puis-je utiliser un point à la place ?

R : Non, .NET ne reconnaît que le double underscore pour les sections imbriquées.

### Q : Comment savoir si ma variable est bien lue ?

R : Vérifiez les logs de l'application. Si la connexion à la base de données fonctionne, la variable est correctement lue.

### Q : Puis-je utiliser plusieurs chaînes de connexion ?

R : Oui, ajoutez :
```
ConnectionStrings__chaine=...
ConnectionStrings__autre_chaine=...
```

Puis dans le code :
```csharp
var connectionString = builder.Configuration.GetConnectionString("autre_chaine");
```

