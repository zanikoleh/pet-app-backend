FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
RUN apt-get update && apt-get install -y --no-install-recommends \
    libgssapi-krb5-2 \
    postgresql-client \
    && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 80 443

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/services/notification-service/src/NotificationService.Api/NotificationService.Api.csproj", "src/services/notification-service/src/NotificationService.Api/"]
COPY ["src/services/notification-service/src/NotificationService.Application/NotificationService.Application.csproj", "src/services/notification-service/src/NotificationService.Application/"]
COPY ["src/services/notification-service/src/NotificationService.Infrastructure/NotificationService.Infrastructure.csproj", "src/services/notification-service/src/NotificationService.Infrastructure/"]
COPY ["src/building-blocks/", "src/building-blocks/"]

RUN dotnet restore "src/services/notification-service/src/NotificationService.Api/NotificationService.Api.csproj"

COPY . .
WORKDIR "/src/src/services/notification-service/src/NotificationService.Api"
RUN dotnet build "NotificationService.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "NotificationService.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "NotificationService.Api.dll"]
