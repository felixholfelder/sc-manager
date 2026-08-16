FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

ARG FIREBASE_API_KEY
ARG FIREBASE_AUTH_DOMAIN
ARG FIREBASE_PROJECT_ID
ARG FIREBASE_STORAGE_BUCKET
ARG FIREBASE_MESSAGING_SENDER_ID
ARG FIREBASE_APP_ID
ARG FIREBASE_MEASUREMENT_ID

ENV FIREBASE_API_KEY=$FIREBASE_API_KEY
ENV FIREBASE_AUTH_DOMAIN=$FIREBASE_AUTH_DOMAIN
ENV FIREBASE_PROJECT_ID=$FIREBASE_PROJECT_ID
ENV FIREBASE_STORAGE_BUCKET=$FIREBASE_STORAGE_BUCKET
ENV FIREBASE_MESSAGING_SENDER_ID=$FIREBASE_MESSAGING_SENDER_ID
ENV FIREBASE_APP_ID=$FIREBASE_APP_ID
ENV FIREBASE_MEASUREMENT_ID=$FIREBASE_MEASUREMENT_ID

# 1. csproj kopieren und restore ausführen
COPY ["sc-manager-server/sc-manager-server.csproj", "sc-manager-server/"]
RUN dotnet restore "sc-manager-server/sc-manager-server.csproj"

# 2. Gesamten Quellcode kopieren
COPY . .

# 3. Publish direkt auf die csproj ausführen (WICHTIG: Nicht im Unterordner workdir wechseln!)
RUN dotnet publish "sc-manager-server/sc-manager-server.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# 4. Den kompletten Publish-Output kopieren
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:3000
ENV ASPNETCORE_ENVIRONMENT=Production
EXPOSE 3000

ENTRYPOINT ["dotnet", "sc-manager-server.dll"]