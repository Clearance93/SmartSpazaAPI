FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore first so source-only changes do not invalidate the restore layer.
COPY SmartSpazaAPI.slnx ./
COPY SmartConfiguration/SmartConfiguration.csproj SmartConfiguration/
COPY SmartCore/SmartCore.csproj SmartCore/
COPY SmartData/SmartData.csproj SmartData/
COPY SmartDto/SmartDto.csproj SmartDto/
COPY SmartEnums/SmartEnums.csproj SmartEnums/
COPY SmartInterfaces/SmartInterfaces.csproj SmartInterfaces/
COPY SmartModels/SmartModels.csproj SmartModels/
COPY SmartRepository/SmartRepository.csproj SmartRepository/
COPY SmartServices/SmartServices.csproj SmartServices/
COPY SmartSpazaAPI/SmartSpazaAPI.csproj SmartSpazaAPI/
COPY SmartUtility/SmartUtility.csproj SmartUtility/
RUN dotnet restore SmartSpazaAPI/SmartSpazaAPI.csproj

COPY . .
RUN dotnet publish SmartSpazaAPI/SmartSpazaAPI.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl -fsS http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "SmartSpazaAPI.dll"]
