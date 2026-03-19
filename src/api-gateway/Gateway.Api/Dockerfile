# Use the official Microsoft .NET image as the base image
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy sln and project files
COPY ["pet-app-backend.sln", "."]
COPY ["src/", "src/"]

# Build the solution
RUN dotnet build "pet-app-backend.sln" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "src/api-gateway/Gateway.Api/Gateway.Api.csproj" -c Release -o /app/publish

# Runtime stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Gateway.Api.dll"]
