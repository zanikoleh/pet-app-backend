# Use the official Microsoft .NET image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgssapi-krb5-2 \
    postgresql-client \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 80 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/services/identity-service/src/IdentityService.Api/IdentityService.Api.csproj", "src/services/identity-service/src/IdentityService.Api/"]
COPY ["src/services/identity-service/src/IdentityService.Application/IdentityService.Application.csproj", "src/services/identity-service/src/IdentityService.Application/"]
COPY ["src/services/identity-service/src/IdentityService.Domain/IdentityService.Domain.csproj", "src/services/identity-service/src/IdentityService.Domain/"]
COPY ["src/services/identity-service/src/IdentityService.Infrastructure/IdentityService.Infrastructure.csproj", "src/services/identity-service/src/IdentityService.Infrastructure/"]
COPY ["src/building-blocks/", "src/building-blocks/"]

RUN dotnet restore "src/services/identity-service/src/IdentityService.Api/IdentityService.Api.csproj"

COPY . .
WORKDIR "/src/src/services/identity-service/src/IdentityService.Api"
RUN dotnet build "IdentityService.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "IdentityService.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IdentityService.Api.dll"]
