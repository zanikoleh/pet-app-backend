# Use the official Microsoft .NET image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project files
COPY ["src/api-gateway/Gateway.Api/Gateway.Api.csproj", "src/api-gateway/Gateway.Api/"]
COPY ["src/api-gateway/Gateway.Api/Program.cs", "src/api-gateway/Gateway.Api/"]
COPY ["src/api-gateway/Gateway.Api/Properties/", "src/api-gateway/Gateway.Api/Properties/"]
COPY ["src/api-gateway/Gateway.Api/yarpconfig.json", "src/api-gateway/Gateway.Api/"]

# Restore dependencies
RUN dotnet restore "src/api-gateway/Gateway.Api/Gateway.Api.csproj"

# Build the project
RUN dotnet build "src/api-gateway/Gateway.Api/Gateway.Api.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
WORKDIR /src
RUN dotnet publish "src/api-gateway/Gateway.Api/Gateway.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gateway.Api.dll"]

