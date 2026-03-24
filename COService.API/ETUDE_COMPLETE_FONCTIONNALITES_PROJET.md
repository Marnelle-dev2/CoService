# 📋 ÉTUDE COMPLÈTE DES FONCTIONNALITÉS - GECO
## Application de Gestion des Certificats d'Origine (CO) et Formule A
### Version pour migration vers microservice .NET Core

---

## 🎯 VUE D'ENSEMBLE DU PROJET

**Nom du projet :** GECO (Gestion Électronique des Certificats d'Origine)  
**Type d'application :** Application web de gestion des certificats d'origine pour la République du Congo 🇨🇬  
**Technologie actuelle :** Laravel (PHP) + Vue.js/Inertia.js  
**Objectif :** Migration vers microservice .NET Core avec architecture microservices

---

## 📊 ARCHITECTURE ACTUELLE

### Stack Technique
- **Backend :** Laravel 8.x/9.x (PHP)
- **Frontend :** Vue.js 3 + Inertia.js (SPA)
- **Base de données :** MySQL/PostgreSQL
- **Authentification :** Laravel Fortify + Sanctum
- **Gestion des rôles :** Spatie Permission
- **Génération PDF :** DomPDF
- **QR Code :** SimpleSoftwareIO/QrCode

### Structure des équipes (Teams)
Le système utilise un concept de `team_id` pour différencier les types d'organisations :
- **team_id = 1** : Chambre de Commerce (CCIAM)
- **team_id = 4** : Mandataire
- **team_id = 5** : Administration centrale
- **team_id = 84** : Exportateur (type spécifique)

---

## 👥 GESTION DES UTILISATEURS ET RÔLES

### Modèle User
**Table :** `users`

**Champs principaux :**
- `id` : Identifiant unique
- `username` : Nom d'utilisateur
- `firstname`, `lastname` : Prénom et nom
- `email` : Email (unique)
- `password` : Mot de passe hashé
- `mobile` : Téléphone mobile
- `role_id` : Rôle de l'utilisateur (relation)
- `team_id` : Type d'organisation (relation)
- `organisation_id` : ID de l'organisation (Exporter ou Partner)
- `activation` : Statut d'activation
- `profile_photo_path` : Photo de profil
- `last_seen` : Dernière connexion

### Rôles et Permissions

**Système de rôles (Spatie Permission) :**
- **Rôle 3** : Contrôleur
- **Rôle 4** : Superviseur
- **Rôle 6** : Président
- Autres rôles personnalisables

**Permissions principales :**
- `list_users`
- `create_certificates`
- `edit_certificates`
- `delete_certificates`
- `view_certificates`
- Gestion des permissions granulaire via Spatie

### Fonctionnalités d'authentification
1. **Inscription/Enregistrement**
   - Enregistrement des exportateurs
   - Enregistrement des partenaires (chambres de commerce)
   - Validation par email (optionnel)

2. **Connexion**
   - Authentification par email/username + mot de passe
   - Support 2FA (Two Factor Authentication)
   - Gestion des sessions

3. **Gestion du profil**
   - Modification des informations personnelles
   - Changement de mot de passe
   - Upload de photo de profil
   - Historique des actions

4. **Récupération de mot de passe**
   - Reset password par email
   - Tokens de réinitialisation

---

## 🏢 GESTION DES ORGANISATIONS

### 1. PARTENAIRES (Chambres de Commerce)

**Modèle :** `Partner`  
**Table :** `partners`

**Champs principaux :**
- `id` : Identifiant unique
- `PartnerCode` : Code unique du partenaire
- `Name` : Nom de la chambre de commerce
- `Adresse` : Adresse complète
- `Phone` : Téléphone
- `Mail` : Email
- `partners_types_id` : Type de partenaire (relation)
- `department_id` : Département (relation)
- `activation` : Statut d'activation (0/1)

**Types de partenaires :**
- Type 1 : Chambre de Commerce standard
- Type 3 : Chambre de Commerce Ouesso
- Type 5 : Autres types

**Fonctionnalités :**
- CRUD complet (Create, Read, Update, Delete)
- Activation/Désactivation des partenaires
- Gestion des logos
- Gestion des prix de certificats
- Gestion des zones de production
- Association avec des exportateurs

**Routes principales :**
- `GET /partners` : Liste des partenaires
- `POST /partners` : Création
- `PUT /partners/{id}` : Modification
- `POST /partStatus/{id}` : Activation/Désactivation

### 2. EXPORTATEURS

**Modèle :** `Exporter`  
**Table :** `exporters`

**Champs principaux :**
- `id` : Identifiant unique
- `ExporterCode` : Code unique de l'exportateur
- `Name` : Nom de l'entreprise
- `SocialReason` : Raison sociale
- `Niu` : Numéro d'Identification Unique
- `Rccm` : Numéro RCCM
- `ActiviyCodeNum` : Code d'activité
- `Adresse` : Adresse complète
- `Phone` : Téléphone
- `Mail` : Email
- `activation` : Statut d'activation (boolean)
- `partner_id` : Partenaire associé
- `department_id` : Département (relation)
- `exportertype` : Type d'exportateur

**Types d'exportateurs :**
- Type 3 : Exportateur spécial (peut créer des Formules A)

**Fonctionnalités :**
- CRUD complet
- Activation/Désactivation par les partenaires
- Gestion des documents (ExporterDocument)
- Gestion des logos
- Demande d'association avec un partenaire
- Envoi d'emails de confirmation lors de l'activation

**Relations :**
- `hasMany` : Certificates, ExporterDocuments, Logos, Facturations
- `belongsTo` : Department, Partner
- `belongsToMany` : Partners (via table pivot `exporters_partners`)

**Routes principales :**
- `GET /exporters` : Liste des exportateurs
- `POST /exporters` : Création
- `POST /exportStatus/{id}` : Activation/Désactivation
- `POST /exporters/{id}/request-partner` : Demande d'association

**Table pivot `exporters_partners` :**
- `exporter_id` : ID exportateur
- `partner_id` : ID partenaire
- `Actif` : Statut de l'association (0/1)

### 3. DESTINATAIRES DE PRODUITS

**Modèle :** `ProductsRecipient`  
**Table :** `products_recipients`

**Champs principaux :**
- `id` : Identifiant unique
- `ProductRecipientName` : Nom du destinataire
- `ProductRecipientAddress1`, `ProductRecipientAddress2` : Adresses
- `ProductRecipientCountry` : Pays
- `ProductRecipientCity` : Ville
- `ProductRecipientZipCode` : Code postal
- `ProductRecipientEmail` : Email
- `ProductRecipionPhoneNo` : Téléphone
- `ProductRecipientWebSiteUrl` : Site web
- `organisation_id` : Organisation propriétaire

**Fonctionnalités :**
- CRUD complet
- Association avec des certificats
- Gestion par organisation

---

## 📜 GESTION DES CERTIFICATS

### Types de Certificats

Le système gère deux types principaux de certificats :

#### 1. Certificat d'Origine (CO) - Standard
- **Préfixe :** CO (ex: CO100000241031224PNR)
- **Statuts :** 1 à 11
- **Workflow complet de validation**

#### 2. Formule A
- **Basé sur un CO validé**
- **Préfixe :** Formule A
- **Statuts :** 12 à 15
- **Workflow spécifique**

### Modèle Certificate

**Table :** `certificates`

**Champs principaux :**
- `id` : Identifiant unique
- `CertificateNo` : Numéro unique du certificat (généré automatiquement)
- `certificate_status_id` : Statut du certificat (relation)
- `statut` : Statut pour Formule A (champ séparé)
- `is_formule_a` : Boolean indiquant si c'est une Formule A
- `certificate_invoice_payment_statut_id` : Statut de paiement
- `certificat_customer_visa_id` : Visa client
- `certificate_payment_mode_id` : Mode de paiement
- `wood_production_area_id` : Zone de production (relation)
- `exporter_id` : Exportateur (relation)
- `partner_id` : Partenaire/Chambre de commerce (relation)
- `mandataire` : ID du mandataire (si applicable)
- `user_id` : Utilisateur créateur
- `products_recipient_id` : Destinataire (relation)
- `country_id` : Pays de destination (relation)
- `Pays_Origine` : Pays d'origine
- `adresse1`, `adresse2` : Adresses
- `navire` : Nom du navire
- `battantpavillon` : Pavillon
- `port_id` : Port de destination (relation)
- `portcongo_id` : Port du Congo (relation)
- `formule` : Type de formule
- `observations` : Observations
- `origin_country` : Pays d'origine
- `items_description` : Description des marchandises
- `gross_weight` : Masse brute
- `invoice_ref` : Référence facture
- `export_doc` : Document d'exportation
- `model_no` : Numéro de modèle
- `model_ref` : Référence modèle
- `customs_office` : Bureau de douane
- `issuing_country` : Pays de délivrance
- `issue_place` : Lieu de délivrance
- `issue_date` : Date de délivrance
- `decl_place` : Lieu de déclaration
- `decl_date` : Date de déclaration
- `control_request_address` : Adresse de demande de contrôle
- `control_place_date` : Lieu et date de contrôle
- `control_result_place_date` : Lieu et date du résultat

**Relations :**
- `belongsTo` : CertificateStatus, Exporter, Partner, Country, Port, Portcongo, ProductsRecipient, WoodProductionArea
- `hasMany` : CertificateLines, CertificateDocuments, CertificateValidations, Commentaires, Facturations

### Workflow de Validation - Certificat d'Origine (CO)

**Statuts et transitions :**

```
Statut 1 : Élaboré
    ↓ (Exportateur soumet)
Statut 2 : Soumis
    ↓ (Contrôleur/Superviseur - rôles 3 ou 4)
Statut 4 : Contrôlé
    ↓ (Contrôleur/Superviseur - rôles 3 ou 4)
Statut 7 : Approuvé
    ↓ (Président SEULEMENT - rôle 6, même organisation)
Statut 8 : Validé ✅ → PDF générable

Statuts alternatifs :
- Statut 5 : Rejeté (peut être fait depuis 2, 4, 7)
- Statut 6 : Autre
- Statut 9 : Autre
- Statut 10 : Modification (retour à 7 après validation)
- Statut 11 : Autre
```

**Règles de validation :**
- **Statut 2 → 4** : Seuls les rôles 3 (Contrôleur) et 4 (Superviseur) peuvent contrôler
- **Statut 4 → 7** : Seuls les rôles 3 et 4 peuvent approuver
- **Statut 7 → 8** : Seul le rôle 6 (Président) peut valider définitivement
- **Rejet (→ 5)** : Nécessite un commentaire obligatoire
- **Toutes les transitions** : Nécessitent la vérification du mot de passe

### Workflow de Validation - Formule A

**Statuts et transitions :**

```
Statut 12 : Formule A soumise
    ↓ (Contrôleur/Superviseur - rôles 3 ou 4)
Statut 13 : Formule A contrôlée
    ↓ (Contrôleur/Superviseur - rôles 3 ou 4)
Statut 14 : Formule A approuvée
    ↓ (Président SEULEMENT - rôle 6, même organisation)
Statut 15 : Formule A validée ✅ → PDF générable

Rejet possible : Statut 5 (avec commentaire obligatoire)
```

**Règles spécifiques :**
- Une Formule A ne peut être créée qu'à partir d'un CO validé (statut 8)
- Le CO original doit appartenir à Ouesso (partner_id = 3)
- Seuls les exportateurs de type 3 ou le propriétaire du CO peuvent créer une Formule A
- Nécessite la vérification du mot de passe pour chaque transition

### Lignes de Certificat (CertificateLine)

**Modèle :** `CertificateLine`  
**Table :** `certificate_lines`

**Champs principaux :**
- `id` : Identifiant unique
- `certificate_id` : Certificat parent (relation)
- `product_id` : Produit (relation)
- `LineNumberOfPackages` : Nombre de colis
- `LineNatureOfProduct` : Nature du produit
- `LineProductBrand` : Marque du produit
- `LineVolume` : Volume
- `LineGrossWeigh` : Poids brut
- `LineValue` : Valeur
- `Unity` : Unité
- `Currency` : Devise
- `wood_production_area_id` : Zone de production

**Fonctionnalités :**
- CRUD complet
- Association avec des produits
- Calculs automatiques (totaux, volumes, valeurs)

### Documents de Certificat

**Modèle :** `CertificateDocument`  
**Table :** `certificate_documents`

**Champs principaux :**
- `id` : Identifiant unique
- `certificate_id` : Certificat parent
- `document_path` : Chemin du document
- `document_type` : Type de document
- `uploaded_by` : Utilisateur uploader

**Fonctionnalités :**
- Upload de documents (PDF, images, etc.)
- Téléchargement
- Suppression
- Association avec certificats

### Validations de Certificat

**Modèle :** `CertificateValidation`  
**Table :** `certificate_validations`

**Champs principaux :**
- `id` : Identifiant unique
- `certificate_id` : Certificat validé
- `user_id` : Utilisateur validateur
- `CertificateValidationValue` : Valeur de validation (commentaire ou "1" pour validation)

**Fonctionnalités :**
- Enregistrement de chaque étape de validation
- Historique complet des validations
- Commentaires associés

### Commentaires

**Modèle :** `Commentaire`  
**Table :** `commentaires`

**Champs principaux :**
- `id` : Identifiant unique
- `certificate_id` : Certificat concerné
- `user_id` : Auteur du commentaire
- `comm` : Contenu du commentaire

**Fonctionnalités :**
- Ajout de commentaires lors du rejet
- Historique des commentaires
- Association avec certificats

### Génération de PDF

**Types de certificats PDF :**
1. **Certificat d'Origine (CO)** - Standard
2. **Formule A** - Format spécifique
3. **EUR.1** - Certificat de circulation des marchandises
4. **ALC** - Autre type

**Fonctionnalités PDF :**
- Génération avec DomPDF
- QR Code intégré
- Signature numérique
- Mise en page professionnelle
- Export en PDF téléchargeable

**Routes de génération :**
- `GET /certiprint/{id}` : Génération CO standard
- `GET /certigenerate/{id}` : Génération CO
- `GET /certigenerate-ouesso/{id}` : Génération CO Ouesso
- `GET /eur1generate/{id}` : Génération EUR.1
- `GET /alcgenerate/{id}` : Génération ALC
- `GET /formule-a/{id}/generate` : Génération Formule A

---

## 💰 GESTION FINANCIÈRE

### Facturation

**Modèle :** `Facturation`  
**Table :** `facturations`

**Champs principaux :**
- `id` : Identifiant unique
- `certificate_id` : Certificat facturé
- `exporter_id` : Exportateur facturé
- `partner_id` : Partenaire facturant
- `FacturationStatus` : Statut de la facturation
- `amount` : Montant
- `factureNo` : Numéro de facture
- Dates de création/paiement

**Fonctionnalités :**
- Génération automatique de factures
- Association avec certificats
- Gestion des statuts de paiement
- Génération de PDF de facture
- Historique des facturations

### Prix des Certificats

**Modèle :** `CertificatePrice`  
**Table :** `certificate_prices`

**Champs principaux :**
- `id` : Identifiant unique
- `partner_id` : Partenaire
- `CertificatePriceValue` : Prix du certificat

**Fonctionnalités :**
- Définition du prix par partenaire
- Utilisation dans la facturation

### Abonnements

**Modèle :** `Abonnement`  
**Table :** `abonnements`

**Champs principaux :**
- `id` : Identifiant unique
- `numero` : Numéro d'abonnement unique
- `exporter_id` : Exportateur
- `partner_id` : Partenaire
- `certificate_id` : Certificat associé
- `formule` : Type de formule
- `factureNo` : Numéro de facture
- `statut_id` : Statut de l'abonnement
- `user_id` : Utilisateur créateur

**Statuts d'abonnement :**
- Statut 3 : Disponible
- Statut 12 : Utilisé

**Fonctionnalités :**
- Création d'abonnements groupés
- Génération automatique de certificats lors de l'abonnement
- Suivi des lignes utilisées/disponibles
- Gestion par exportateur et partenaire

**Workflow :**
1. Partenaire crée un abonnement pour un exportateur
2. Spécifie le nombre de certificats et la formule
3. Le système génère automatiquement les certificats avec statut 3
4. Les certificats sont utilisés (statut passe à 12) lors de la validation

---

## 📦 GESTION DES PRODUITS

### Produits

**Modèle :** `Product`  
**Table :** `products`

**Champs principaux :**
- `id` : Identifiant unique
- `ProductName` : Nom du produit
- `ProductCode` : Code du produit
- `ProductDescription` : Description
- Autres champs spécifiques

**Fonctionnalités :**
- CRUD complet
- Association avec lignes de certificat
- Gestion par organisation

### Prix des Produits

**Modèle :** `PrixProduit`  
**Table :** `prix_produits`

**Fonctionnalités :**
- Gestion des prix par produit
- Association avec partenaires
- Utilisation dans les calculs de certificats

---

## 🌍 GESTION GÉOGRAPHIQUE

### Pays

**Modèle :** `Country`  
**Table :** `countries`

**Champs principaux :**
- `id` : Identifiant unique
- `CountryCode` : Code pays (ISO)
- `CountryName` : Nom du pays
- Autres champs

**Note :** Cette table fait partie du référentiel global et sera gérée par un microservice référentiel séparé.

### Ports

**Modèle :** `Port`  
**Table :** `ports`

**Champs principaux :**
- `id` : Identifiant unique
- `PortCode` : Code du port
- `PortName` : Nom du port
- `country_id` : Pays (relation)
- `Type` : Type de port (maritime, fluvial, etc.)

**Note :** Fait partie du référentiel global.

### Aéroports

**Modèle :** `Aeroport`  
**Table :** `aeroports`

**Champs principaux :**
- `id` : Identifiant unique
- `AeroportCode` : Code aéroport
- `AeroportName` : Nom
- `country_id` : Pays (relation)

**Note :** Fait partie du référentiel global.

### Fleuves

**Modèle :** `Fleuve`  
**Table :** `fleuves`

**Note :** Fait partie du référentiel global.

### Routes Nationales

**Modèle :** `RoutesNationale`  
**Table :** `routes_nationales`

**Note :** Fait partie du référentiel global.

### Corridors

**Modèle :** `Corridor`  
**Table :** `corridors`

**Note :** Fait partie du référentiel global.

### Départements

**Modèle :** `Department`  
**Table :** `departments`

**Champs principaux :**
- `id` : Identifiant unique
- `DepartmentCode` : Code département
- `DepartmentName` : Nom du département

**Note :** Fait partie du référentiel global mais utilisé localement pour la génération de numéros de certificats.

### Zones de Production

**Modèle :** `WoodProductionArea`  
**Table :** `wood_production_areas`

**Champs principaux :**
- `id` : Identifiant unique
- `partner_id` : Partenaire propriétaire
- `ProductionAreaName` : Nom de la zone

**Fonctionnalités :**
- CRUD complet
- Association avec certificats
- Gestion par partenaire

---

## 📄 GESTION DES DOCUMENTS

### Documents d'Exportateur

**Modèle :** `ExporterDocument`  
**Table :** `exporter_documents`

**Champs principaux :**
- `id` : Identifiant unique
- `exporter_id` : Exportateur
- `document_path` : Chemin du document
- `document_type` : Type de document
- `uploaded_by` : Utilisateur

**Fonctionnalités :**
- Upload de documents
- Téléchargement
- Gestion par exportateur

### Logos

**Modèle :** `Logo`  
**Table :** `logos`

**Champs principaux :**
- `id` : Identifiant unique
- `organisation_id` : Organisation (Exporter ou Partner)
- `LogoPath` : Chemin du logo
- `partener_id` : Partenaire (si applicable)

**Fonctionnalités :**
- Upload de logos
- Utilisation dans les PDF
- Gestion par organisation

### Signatures

**Modèle :** `Signature`  
**Table :** `signatures`

**Champs principaux :**
- `id` : Identifiant unique
- `user_id` : Utilisateur
- `urlsign` : URL de la signature (chiffrée)

**Fonctionnalités :**
- Upload de signatures
- Chiffrement des signatures
- Utilisation dans les PDF
- Association avec utilisateurs

---

## 🔐 SÉCURITÉ ET PERMISSIONS

### Système de Rôles (Spatie Permission)

**Tables :**
- `roles` : Rôles
- `permissions` : Permissions
- `model_has_roles` : Association utilisateurs-rôles
- `model_has_permissions` : Association utilisateurs-permissions
- `role_has_permissions` : Association rôles-permissions

**Rôles principaux :**
- **Rôle 3** : Contrôleur
  - Peut contrôler les certificats (statut 2 → 4)
  - Peut approuver (statut 4 → 7)
  - Ne peut pas valider définitivement (statut 7 → 8)

- **Rôle 4** : Superviseur
  - Mêmes permissions que Contrôleur
  - Peut contrôler et approuver

- **Rôle 6** : Président
  - Peut valider définitivement (statut 7 → 8)
  - Peut valider Formule A (statut 14 → 15)
  - Doit appartenir à la même organisation que le certificat

**Permissions principales :**
- `list_users`
- `create_certificates`
- `edit_certificates`
- `delete_certificates`
- `view_certificates`
- Permissions granulaire par action

### Validation des Transitions

**Règles strictes :**
1. Vérification du rôle utilisateur
2. Vérification de l'organisation (pour Président)
3. Vérification du mot de passe (pour toutes les validations)
4. Vérification de la validité de la transition
5. Enregistrement de la validation dans l'historique

### CSRF Protection

- Protection CSRF sur toutes les routes POST/PUT/DELETE
- Tokens générés automatiquement
- Validation côté serveur

---

## 📊 DASHBOARD ET STATISTIQUES

### Tableau de Bord Principal

**Fonctionnalités :**
- Vue d'ensemble des certificats par statut
- Compteurs de certificats (validés, en attente, rejetés)
- Statistiques de masse et valeur
- Graphiques (ApexCharts)
- Filtres par statut, date, exportateur
- Recherche avancée

**Vues selon le rôle :**
- **Chambre de Commerce (team_id = 1)** :
  - Certificats soumis (statut 2, 4, 6, 7, 9, 10)
  - Formules A (statut 12, 13, 14, 15)
  - Actions selon le rôle

- **Mandataire (team_id = 4)** :
  - Certificats validés (statut 8)
  - Formules A validées (statut 15)

- **Administration (team_id = 5)** :
  - Vue globale de tous les certificats
  - Statistiques complètes

- **Exportateur (team_id = 84)** :
  - Ses propres certificats
  - Statuts de ses demandes

### Statistiques

**Métriques calculées :**
- Total de certificats par statut
- Masse totale exportée
- Valeur totale exportée
- Nombre d'exportateurs actifs
- Nombre de certificats par période
- Taux de validation/rejet

---

## 🔄 WORKFLOWS MÉTIER

### Workflow Complet - Création d'un Certificat d'Origine

1. **Création par Exportateur**
   - Exportateur crée un nouveau certificat
   - Remplit les informations (exportateur, destinataire, produits, etc.)
   - Upload des documents requis
   - Statut initial : 1 (Élaboré)

2. **Soumission**
   - Exportateur soumet le certificat
   - Statut passe à 2 (Soumis)
   - Notification à la chambre de commerce

3. **Contrôle**
   - Contrôleur/Superviseur examine le certificat
   - Statut passe à 4 (Contrôlé) ou 5 (Rejeté)
   - Si rejeté, commentaire obligatoire

4. **Approbation**
   - Contrôleur/Superviseur approuve
   - Statut passe à 7 (Approuvé)
   - Notification au Président

5. **Validation Finale**
   - Président valide définitivement
   - Statut passe à 8 (Validé)
   - PDF générable

6. **Génération PDF**
   - Exportateur ou mandataire génère le PDF
   - QR Code inclus
   - Signature numérique

### Workflow - Formule A

1. **Création depuis CO validé**
   - Exportateur sélectionne un CO validé (statut 8)
   - Vérification que le CO appartient à Ouesso
   - Création de la Formule A
   - Statut initial : 12 (Formule A soumise)

2. **Contrôle Formule A**
   - Contrôleur/Superviseur contrôle
   - Statut passe à 13 (Formule A contrôlée) ou 5 (Rejetée)

3. **Approbation Formule A**
   - Contrôleur/Superviseur approuve
   - Statut passe à 14 (Formule A approuvée)

4. **Validation Finale**
   - Président valide définitivement
   - Statut passe à 15 (Formule A validée)
   - PDF générable

### Workflow - Abonnement

1. **Création d'Abonnement**
   - Partenaire crée un abonnement pour un exportateur
   - Spécifie le nombre de certificats et la formule
   - Génération automatique des certificats (statut 3)

2. **Utilisation**
   - Exportateur utilise les certificats de l'abonnement
   - Statut passe à 12 (Utilisé) lors de la validation

3. **Suivi**
   - Suivi des lignes disponibles/utilisées
   - Statistiques par exportateur

---

## 📧 NOTIFICATIONS ET EMAILS

### Types d'Emails

1. **Confirmation d'Activation**
   - Envoyé lors de l'activation d'un exportateur
   - Contient les informations de connexion

2. **Enrôlement Partenaire**
   - Envoyé lors de l'enregistrement d'un partenaire

3. **Notifications de Validation**
   - Notifications lors des changements de statut
   - Alertes aux utilisateurs concernés

### Système de Notifications

- Notifications en temps réel (via Inertia.js)
- Emails de confirmation
- Alertes dans l'interface

---

## 🔍 RECHERCHE ET FILTRES

### Fonctionnalités de Recherche

**Par Certificat :**
- Numéro de certificat
- Exportateur
- Destinataire
- Statut
- Date de création
- Pays de destination

**Par Exportateur :**
- Nom
- Code
- Email
- Statut d'activation

**Par Partenaire :**
- Nom
- Code
- Type

### Filtres Avancés

- Filtres par statut
- Filtres par date (période)
- Filtres par organisation
- Filtres par rôle
- Filtres combinés

---

## 🗄️ STRUCTURE DE BASE DE DONNÉES

### Tables Principales

**Gestion des Utilisateurs :**
- `users` : Utilisateurs
- `roles` : Rôles
- `permissions` : Permissions
- `model_has_roles` : Association utilisateurs-rôles
- `model_has_permissions` : Association utilisateurs-permissions
- `role_has_permissions` : Association rôles-permissions
- `teams` : Équipes/Organisations
- `signatures` : Signatures utilisateurs

**Gestion des Organisations :**
- `partners` : Partenaires (Chambres de Commerce)
- `exporters` : Exportateurs
- `exporters_partners` : Table pivot exportateurs-partenaires
- `partners_types` : Types de partenaires
- `departments` : Départements

**Gestion des Certificats :**
- `certificates` : Certificats
- `certificate_lines` : Lignes de certificat
- `certificate_statuses` : Statuts de certificat
- `certificate_validations` : Validations
- `certificate_documents` : Documents de certificat
- `certificat_lignes` : Lignes (ancien format)
- `commentaires` : Commentaires

**Gestion Financière :**
- `facturations` : Facturations
- `certificate_prices` : Prix des certificats
- `abonnements` : Abonnements
- `prix_produits` : Prix des produits

**Gestion des Produits :**
- `products` : Produits
- `products_recipients` : Destinataires

**Gestion Géographique :**
- `countries` : Pays (référentiel)
- `ports` : Ports (référentiel)
- `aeroports` : Aéroports (référentiel)
- `fleuves` : Fleuves (référentiel)
- `routes_nationales` : Routes nationales (référentiel)
- `corridors` : Corridors (référentiel)
- `portcongos` : Ports du Congo
- `wood_production_areas` : Zones de production

**Gestion des Documents :**
- `exporter_documents` : Documents d'exportateur
- `logos` : Logos
- `documents` : Documents généraux

**Autres :**
- `certificat_customer_visas` : Visas clients
- `posts` : Posts/Actualités
- `users_pictures` : Photos utilisateurs

### Relations Clés

**Certificates :**
- `belongsTo` : Exporter, Partner, Country, Port, Portcongo, ProductsRecipient, WoodProductionArea, CertificateStatus
- `hasMany` : CertificateLines, CertificateDocuments, CertificateValidations, Commentaires, Facturations

**Exporters :**
- `belongsTo` : Department, Partner
- `hasMany` : Certificates, ExporterDocuments, Logos, Facturations
- `belongsToMany` : Partners (via exporters_partners)

**Partners :**
- `belongsTo` : Department, PartnersTypes
- `hasMany` : Certificates, WoodProductionAreas, Logos, Facturations

---

## 🔌 API ET INTÉGRATIONS

### Routes API (Laravel Sanctum)

**Authentification :**
- `POST /api/login` : Connexion
- `GET /api/user` : Utilisateur connecté (protégé)

**Certificats :**
- `GET /api/getcertificate` : Récupération de certificat (intégration externe)

**Posts :**
- `GET /api/posts` : Liste des posts
- `POST /api/posts` : Création
- `PUT /api/posts/{id}` : Modification
- `DELETE /api/posts/{id}` : Suppression

### Intégrations Externes

**GECO vers AFTRA :**
- Export de données de certificats
- Synchronisation

---

## 📝 GÉNÉRATION DE NUMÉROS

### Numéro de Certificat (CO)

**Format :** `CO{Numéro}{Date}{CodeDépartement}`

**Exemple :** `CO100000241031224PNR`

**Génération :**
1. Récupération du dernier numéro pour le partenaire
2. Extraction du numéro séquentiel
3. Incrémentation
4. Ajout de la date (format ddmmyy)
5. Ajout du code département du partenaire

### Numéro d'Abonnement

**Format :** `{Année}{Mois}{Jour}{Heure}{Minute}{Seconde}{LettreAléatoire}`

**Exemple :** `20241003143025A`

**Génération :**
- Timestamp + lettre aléatoire
- Garantit l'unicité

---

## 🎨 INTERFACE UTILISATEUR

### Technologies Frontend

- **Vue.js 3** : Framework JavaScript
- **Inertia.js** : Bridge Laravel-Vue (SPA)
- **Tailwind CSS** : Framework CSS
- **ApexCharts** : Graphiques
- **DataTables** : Tableaux interactifs
- **Select2** : Sélecteurs avancés

### Pages Principales

**Authentification :**
- `/` : Page d'accueil (inscription/connexion)
- `/login` : Connexion
- `/forgot-password` : Mot de passe oublié
- `/reset-password` : Réinitialisation

**Dashboard :**
- `/dashboard` : Tableau de bord principal
- `/spa/dashboard` : Dashboard SPA

**Certificats :**
- `/certificats` : Liste des certificats
- `/certificats/create` : Création
- `/certificats/{id}` : Détail
- `/certificats/{id}/edit` : Modification
- `/spa/certificates` : Liste SPA
- `/spa/certificates-create` : Création SPA

**Exportateurs :**
- `/exporters` : Liste
- `/exporters/create` : Création
- `/exporters/{id}` : Détail
- `/spa/exporters` : Liste SPA

**Partenaires :**
- `/partners` : Liste
- `/spa/partners` : Liste SPA

**Abonnements :**
- `/abonnements` : Liste
- `/spa/abonnements` : Liste SPA

**Facturation :**
- `/factures` : Liste
- `/spa/billing/invoices` : Liste SPA

**Administration :**
- `/spa/admin/users` : Gestion utilisateurs
- `/spa/admin/roles` : Gestion rôles
- `/spa/admin/permissions` : Gestion permissions

---

## 🔧 FONCTIONNALITÉS TECHNIQUES

### Validation des Données

**Côté Serveur (Laravel) :**
- Form Requests pour validation
- Règles de validation personnalisées
- Messages d'erreur personnalisés

**Côté Client (Vue.js) :**
- Validation en temps réel
- Feedback utilisateur immédiat

### Gestion des Fichiers

- Upload de documents (PDF, images)
- Stockage dans `storage/app`
- Génération de noms uniques
- Validation des types de fichiers
- Taille maximale limitée

### Transactions Base de Données

- Utilisation de transactions pour les opérations critiques
- Rollback en cas d'erreur
- Cohérence des données garantie

### Logging

- Logs des actions importantes
- Traçabilité des validations
- Logs d'erreurs
- Debug logging (environnement développement)

### Cache

- Cache des configurations
- Cache des routes
- Cache des vues
- Optimisation des performances

---

## 📋 RÉFÉRENTIELS NÉCESSAIRES POUR LE MICROSERVICE

### Référentiels à Conserver Localement

Ces référentiels sont nécessaires pour le fonctionnement du microservice mais seront synchronisés avec le microservice référentiel global :

1. **Départements (Departments)**
   - Utilisé pour la génération de numéros de certificats
   - Code département nécessaire dans le format

2. **Zones de Production (WoodProductionAreas)**
   - Gérées par les partenaires
   - Spécifiques au domaine métier

3. **Produits (Products)**
   - Catalogue de produits
   - Gérés par organisation

4. **Types de Partenaires (PartnersTypes)**
   - Classification des partenaires
   - Spécifique au domaine

### Référentiels à Consulter via API (Microservice Référentiel)

Ces référentiels seront consultés via des appels API au microservice référentiel global :

1. **Pays (Countries)**
   - Consultation pour sélection dans formulaires
   - Pas de modification locale

2. **Ports (Ports)**
   - Consultation pour sélection
   - Filtrage par pays

3. **Aéroports (Aeroports)**
   - Consultation pour sélection
   - Filtrage par pays

4. **Fleuves (Fleuves)**
   - Consultation pour sélection

5. **Routes Nationales (RoutesNationales)**
   - Consultation pour sélection

6. **Corridors (Corridors)**
   - Consultation pour sélection

7. **Tronçons (Troncons)**
   - Consultation pour sélection

8. **Sections (Sections)**
   - Consultation pour sélection

9. **Devises (Devises)**
   - Consultation pour sélection

10. **Taux de Change (TauxDeChanges)**
    - Consultation pour calculs

11. **Incoterms**
    - Consultation pour sélection

12. **Bureaux de Douanement**
    - Consultation pour sélection

13. **Sections/Chapitres/Divisions/Catégories/Positions Tarifaires**
    - Consultation pour classification

---

## 🎯 POINTS CLÉS POUR LA MIGRATION .NET CORE

### Architecture Microservice Recommandée

**Microservice Certificats (Ce projet) :**
- Gestion complète du cycle de vie des certificats
- Workflows de validation
- Génération de PDF
- Gestion des abonnements
- Facturation

**Microservice Référentiel (Existant) :**
- Tous les référentiels géographiques, transport, douane, finance
- API REST pour consultation

**Microservice Authentification (À créer ou utiliser existant) :**
- Gestion des utilisateurs
- Authentification JWT
- Gestion des rôles et permissions

**Microservice Notifications (Optionnel) :**
- Envoi d'emails
- Notifications en temps réel

### Entités Principales à Migrer

1. **Certificate** : Entité centrale
2. **Exporter** : Exportateurs
3. **Partner** : Partenaires/Chambres de commerce
4. **CertificateLine** : Lignes de certificat
5. **CertificateStatus** : Statuts
6. **CertificateValidation** : Validations
7. **Abonnement** : Abonnements
8. **Facturation** : Facturations
9. **ProductsRecipient** : Destinataires
10. **WoodProductionArea** : Zones de production

### Services à Implémenter

1. **CertificateService** : Logique métier certificats
2. **ValidationService** : Workflows de validation
3. **PDFGenerationService** : Génération PDF
4. **AbonnementService** : Gestion abonnements
5. **FacturationService** : Gestion facturation
6. **NotificationService** : Envoi notifications
7. **ReferentielService** : Appels API référentiel

### Contrôleurs API REST

1. **CertificatesController** : CRUD + workflows
2. **ExportersController** : CRUD exportateurs
3. **PartnersController** : CRUD partenaires
4. **AbonnementsController** : Gestion abonnements
5. **FacturationsController** : Gestion facturation
6. **ValidationsController** : Validations
7. **PDFController** : Génération PDF

### DTOs (Data Transfer Objects)

- **CertificateDTO** : Transfert certificat
- **CertificateLineDTO** : Transfert ligne
- **ValidationDTO** : Transfert validation
- **AbonnementDTO** : Transfert abonnement
- **FacturationDTO** : Transfert facturation

### Validations Métier

- Validation des transitions de statut
- Validation des rôles et permissions
- Validation des données de certificat
- Validation des workflows

### Intégrations

- **API Référentiel** : Consultation référentiels
- **Service d'Authentification** : Vérification tokens JWT
- **Service de Notifications** : Envoi emails
- **Service de Stockage** : Upload/download fichiers

---

## 📊 STATISTIQUES ET MÉTRIQUES

### Métriques à Suivre

1. **Certificats :**
   - Nombre total par statut
   - Taux de validation
   - Taux de rejet
   - Temps moyen de traitement

2. **Exportateurs :**
   - Nombre d'exportateurs actifs
   - Nombre de certificats par exportateur
   - Exportateurs les plus actifs

3. **Partenaires :**
   - Nombre de certificats par partenaire
   - Performance de validation

4. **Financier :**
   - Chiffre d'affaires
   - Factures générées
   - Paiements reçus

---

## 🔒 SÉCURITÉ

### Mesures de Sécurité Actuelles

1. **Authentification :**
   - Hashage des mots de passe (bcrypt)
   - Tokens CSRF
   - Sessions sécurisées

2. **Autorisation :**
   - Vérification des rôles
   - Vérification des permissions
   - Vérification de l'organisation

3. **Validation :**
   - Validation côté serveur
   - Validation des transitions
   - Vérification du mot de passe pour validations critiques

4. **Protection des Données :**
   - Chiffrement des signatures
   - Stockage sécurisé des fichiers
   - Protection contre les injections SQL (Eloquent ORM)

### Recommandations pour .NET Core

1. **JWT Authentication** : Tokens JWT pour API
2. **Policy-based Authorization** : Politiques d'autorisation
3. **Data Annotations** : Validation des modèles
4. **HTTPS** : Communication sécurisée
5. **CORS** : Configuration CORS appropriée
6. **Rate Limiting** : Limitation des requêtes
7. **Input Validation** : Validation stricte des entrées
8. **SQL Injection Protection** : Entity Framework Core

---

## 📝 CONCLUSION

Ce document présente une vue complète des fonctionnalités du système GECO. Pour la migration vers .NET Core en microservice, il sera nécessaire de :

1. **Séparer les responsabilités** entre microservices
2. **Conserver localement** les référentiels spécifiques au domaine
3. **Consulter via API** les référentiels globaux
4. **Implémenter les workflows** de validation avec les mêmes règles métier
5. **Maintenir la compatibilité** avec les formats de PDF existants
6. **Assurer la sécurité** avec JWT et politiques d'autorisation
7. **Optimiser les performances** avec cache et requêtes optimisées

Le microservice Certificats sera le cœur du système, gérant tout le cycle de vie des certificats d'origine et des formules A, tout en s'intégrant avec les autres microservices pour les référentiels, l'authentification et les notifications.

---

**Document généré le :** 2025-01-XX  
**Version :** 1.0  
**Projet :** GECO - Migration .NET Core Microservice
