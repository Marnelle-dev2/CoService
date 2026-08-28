# Utiliser l'image de base .NET 8 SDK pour la compilation
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copier les fichiers de projet
COPY ["COService.API/COService.API.csproj", "COService.API/"]
COPY ["COService.Application/COService.Application.csproj", "COService.Application/"]
COPY ["COService.Domain/COService.Domain.csproj", "COService.Domain/"]
COPY ["COService.Infrastructure/COService.Infrastructure.csproj", "COService.Infrastructure/"]
COPY ["COService.Shared/COService.Shared.csproj", "COService.Shared/"]

# Restaurer les dépendances
RUN dotnet restore "COService.API/COService.API.csproj"

# Copier tout le code source
COPY . .

# Compiler et publier l'application
WORKDIR "/src/COService.API"
RUN dotnet build "COService.API.csproj" -c Release -o /app/build
RUN dotnet publish "COService.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Image finale avec .NET 8 Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Health check Portainer/Compose : probe TCP (pas de curl — apt debian parfois bloqué en build)
# Consul vérifie HTTP /sante depuis l'extérieur du conteneur.
RUN groupadd -r appuser && useradd -r -g appuser appuser

# Copier les fichiers publiés depuis l'étape de build
COPY --from=build /app/publish .

# Changer le propriétaire des fichiers
RUN chown -R appuser:appuser /app

# Passer à l'utilisateur non-root
USER appuser

# Exposer le port 8700
EXPOSE 8700

# Variables d'environnement par défaut
ENV ASPNETCORE_URLS=http://+:8700
ENV ASPNETCORE_ENVIRONMENT=Production

# Point d'entrée
ENTRYPOINT ["dotnet", "COService.API.dll"]

