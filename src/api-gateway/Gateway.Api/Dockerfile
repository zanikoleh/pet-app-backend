# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy building blocks - copy csproj files first for better layer caching
COPY ["src/building-blocks/contracts/Contracts.csproj", "src/building-blocks/contracts/"]
COPY ["src/building-blocks/event-bus/EventBus.csproj", "src/building-blocks/event-bus/"]
COPY ["src/building-blocks/infrastructure/Infrastructure.csproj", "src/building-blocks/infrastructure/"]
COPY ["src/building-blocks/infrastructure-web/InfrastructureWeb.csproj", "src/building-blocks/infrastructure-web/"]
COPY ["src/building-blocks/observability/Observability.csproj", "src/building-blocks/observability/"]
COPY ["src/building-blocks/shared-kernel/SharedKernel.csproj", "src/building-blocks/shared-kernel/"]

# Copy API gateway project
COPY ["src/api-gateway/Gateway.Api/Gateway.Api.csproj", "src/api-gateway/Gateway.Api/"]

# Restore dependencies
RUN dotnet restore "src/api-gateway/Gateway.Api/Gateway.Api.csproj"

# Copy source files
COPY ["src/building-blocks/", "src/building-blocks/"]
COPY ["src/api-gateway/Gateway.Api/Program.cs", "src/api-gateway/Gateway.Api/"]
COPY ["src/api-gateway/Gateway.Api/Properties/", "src/api-gateway/Gateway.Api/Properties/"]
COPY ["src/api-gateway/Gateway.Api/yarpconfig.json", "src/api-gateway/Gateway.Api/"]

# Build the project
RUN dotnet build "src/api-gateway/Gateway.Api/Gateway.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
WORKDIR /src
RUN dotnet publish "src/api-gateway/Gateway.Api/Gateway.Api.csproj" -c Release -o /app/publish /p:PublishReadyToRun=true /p:PublishTrimmed=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
LABEL maintainer="pet-app team" \
      description="API Gateway service"

# Create non-root user for security
RUN useradd -m -u 1000 appuser

WORKDIR /app
EXPOSE 80 443

COPY --from=publish --chown=appuser:appuser /app/publish .

# Add health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
  CMD dotnet /app/healthcheck.dll || exit 1

USER appuser
ENTRYPOINT ["dotnet", "Gateway.Api.dll"]

