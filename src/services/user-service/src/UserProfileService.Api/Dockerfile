# Dockerfiles for other services follow similar pattern as IdentityService.Api

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/services/user-service/src/UserProfileService.Api/UserProfileService.Api.csproj", "src/services/user-service/src/UserProfileService.Api/"]
COPY ["src/services/user-service/src/UserProfileService.Application/UserProfileService.Application.csproj", "src/services/user-service/src/UserProfileService.Application/"]
COPY ["src/services/user-service/src/UserProfileService.Domain/UserProfileService.Domain.csproj", "src/services/user-service/src/UserProfileService.Domain/"]
COPY ["src/services/user-service/src/UserProfileService.Infrastructure/UserProfileService.Infrastructure.csproj", "src/services/user-service/src/UserProfileService.Infrastructure/"]
COPY ["src/building-blocks/", "src/building-blocks/"]

RUN dotnet restore "src/services/user-service/src/UserProfileService.Api/UserProfileService.Api.csproj"

COPY . .
WORKDIR "/src/src/services/user-service/src/UserProfileService.Api"
RUN dotnet build "UserProfileService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "UserProfileService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "UserProfileService.Api.dll"]
