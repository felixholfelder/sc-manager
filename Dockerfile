# --- Build Stage ---
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Nur die csproj zuerst kopieren, für besseres Docker-Layer-Caching
COPY ["sc-manager-server/sc-manager-server.csproj", "sc-manager-server/"]
RUN dotnet restore "sc-manager-server/sc-manager-server.csproj"

# Restlichen Code kopieren und veröffentlichen
COPY . .
WORKDIR /src/sc-manager-server
RUN dotnet publish -c Release -o /app/publish --no-restore

# --- Runtime Stage ---
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build --chown=appuser:appuser /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "sc-manager-server.dll"]