FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/services/file-service/src/FileService.Api/FileService.Api.csproj", "src/services/file-service/src/FileService.Api/"]
COPY ["src/services/file-service/src/FileService.Application/FileService.Application.csproj", "src/services/file-service/src/FileService.Application/"]
COPY ["src/services/file-service/src/FileService.Domain/FileService.Domain.csproj", "src/services/file-service/src/FileService.Domain/"]
COPY ["src/services/file-service/src/FileService.Infrastructure/FileService.Infrastructure.csproj", "src/services/file-service/src/FileService.Infrastructure/"]
COPY ["src/building-blocks/", "src/building-blocks/"]

RUN dotnet restore "src/services/file-service/src/FileService.Api/FileService.Api.csproj"

COPY . .
WORKDIR "/src/src/services/file-service/src/FileService.Api"
RUN dotnet build "FileService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FileService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileService.Api.dll"]
