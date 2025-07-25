# © 2024 DecVCPlat. All rights reserved.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["DecVCPlat.ApiGateway/DecVCPlat.ApiGateway.csproj", "DecVCPlat.ApiGateway/"]
COPY ["DecVCPlat.Shared/DecVCPlat.Shared.csproj", "DecVCPlat.Shared/"]
RUN dotnet restore "./DecVCPlat.ApiGateway/DecVCPlat.ApiGateway.csproj"
COPY . .
WORKDIR "/src/DecVCPlat.ApiGateway"
RUN dotnet build "./DecVCPlat.ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./DecVCPlat.ApiGateway.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create non-root user for security
RUN adduser --disabled-password --gecos '' decvcplatuser
RUN chown -R decvcplatuser:decvcplatuser /app
USER decvcplatuser

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD curl -f http://localhost:8080/health || exit 1

ENTRYPOINT ["dotnet", "DecVCPlat.ApiGateway.dll"]
