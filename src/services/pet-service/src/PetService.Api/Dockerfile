FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 80 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/services/pet-service/src/PetService.Api/PetService.Api.csproj", "src/services/pet-service/src/PetService.Api/"]
COPY ["src/services/pet-service/src/PetService.Application/PetService.Application.csproj", "src/services/pet-service/src/PetService.Application/"]
COPY ["src/services/pet-service/src/PetService.Domain/PetService.Domain.csproj", "src/services/pet-service/src/PetService.Domain/"]
COPY ["src/services/pet-service/src/PetService.Infrastructure/PetService.Infrastructure.csproj", "src/services/pet-service/src/PetService.Infrastructure/"]
COPY ["src/building-blocks/", "src/building-blocks/"]

RUN dotnet restore "src/services/pet-service/src/PetService.Api/PetService.Api.csproj"

COPY . .
WORKDIR "/src/src/services/pet-service/src/PetService.Api"
RUN dotnet build "PetService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PetService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PetService.Api.dll"]
