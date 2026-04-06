# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy building blocks csproj files first for better caching
COPY ["src/building-blocks/contracts/Contracts.csproj", "src/building-blocks/contracts/"]
COPY ["src/building-blocks/event-bus/EventBus.csproj", "src/building-blocks/event-bus/"]
COPY ["src/building-blocks/infrastructure/Infrastructure.csproj", "src/building-blocks/infrastructure/"]
COPY ["src/building-blocks/infrastructure-web/InfrastructureWeb.csproj", "src/building-blocks/infrastructure-web/"]
COPY ["src/building-blocks/observability/Observability.csproj", "src/building-blocks/observability/"]
COPY ["src/building-blocks/shared-kernel/SharedKernel.csproj", "src/building-blocks/shared-kernel/"]

# Copy service project files
COPY ["src/services/pet-service/src/PetService.Api/PetService.Api.csproj", "src/services/pet-service/src/PetService.Api/"]
COPY ["src/services/pet-service/src/PetService.Application/PetService.Application.csproj", "src/services/pet-service/src/PetService.Application/"]
COPY ["src/services/pet-service/src/PetService.Domain/PetService.Domain.csproj", "src/services/pet-service/src/PetService.Domain/"]
COPY ["src/services/pet-service/src/PetService.Infrastructure/PetService.Infrastructure.csproj", "src/services/pet-service/src/PetService.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "src/services/pet-service/src/PetService.Api/PetService.Api.csproj"

# Copy source files
COPY ["src/building-blocks/", "src/building-blocks/"]
COPY ["src/services/pet-service/src/", "src/services/pet-service/src/"]

WORKDIR "/src/src/services/pet-service/src/PetService.Api"
RUN dotnet build "PetService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
WORKDIR /src/src/services/pet-service/src/PetService.Api
RUN dotnet publish "PetService.Api.csproj" -c Release -o /app/publish /p:PublishReadyToRun=true /p:PublishTrimmed=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
LABEL maintainer="pet-app team" \
      description="Pet Service API"

# Install system dependencies with specific versions
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgssapi-krb5-2=1.18.3-6+deb11u3 \
    postgresql-client=12.15-1.pgdg110+1 \
    && apt-get clean \
    && rm -rf /var/lib/apt/lists/* /tmp/* /var/tmp/*

# Create non-root user for security
RUN useradd -m -u 1000 appuser

WORKDIR /app
EXPOSE 80 443

COPY --from=publish --chown=appuser:appuser /app/publish .

# Add health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD wget --no-verbose --tries=1 --spider http://localhost:80/health || exit 1

USER appuser
ENTRYPOINT ["dotnet", "PetService.Api.dll"]
