#!/bin/bash

# Makefile for Pet App Backend
# Common development commands

.PHONY: help build run test clean restore db-migrate docker-up docker-down

help:
	@echo "Pet App Backend - Build Commands"
	@echo ""
	@echo "Usage: make [target]"
	@echo ""
	@echo "Targets:"
	@echo "  build           - Build the solution"
	@echo "  run             - Run all services"
	@echo "  test            - Run all tests"
	@echo "  clean           - Clean build artifacts"
	@echo "  restore         - Restore NuGet packages"
	@echo "  docker-up       - Start Docker containers"
	@echo "  docker-down     - Stop Docker containers"
	@echo "  db-migrate      - Run database migrations"
	@echo "  logs            - View Docker logs"

build:
	@echo "Building solution..."
	dotnet build src/api-gateway/Gateway.Api/Gateway.Api.csproj -c Release
	dotnet build src/services/identity-service/src/IdentityService.Api/IdentityService.Api.csproj -c Release
	dotnet build src/services/user-service/src/UserProfileService.Api/UserProfileService.Api.csproj -c Release
	dotnet build src/services/file-service/src/FileService.Api/FileService.Api.csproj -c Release
	dotnet build src/services/pet-service/src/PetService.Api/PetService.Api.csproj -c Release
	dotnet build src/services/notification-service/src/NotificationService.Api/NotificationService.Api.csproj -c Release

run:
	@echo "Running all services..."
	docker-compose up -d

test:
	@echo "Running tests..."
	dotnet test tests/IntegrationTests/IntegrationTests.csproj

clean:
	@echo "Cleaning build artifacts..."
	dotnet clean src/api-gateway/Gateway.Api/Gateway.Api.csproj 2>/dev/null || true
	dotnet clean src/services/identity-service/src/IdentityService.Api/IdentityService.Api.csproj 2>/dev/null || true
	dotnet clean src/services/user-service/src/UserProfileService.Api/UserProfileService.Api.csproj 2>/dev/null || true
	dotnet clean src/services/file-service/src/FileService.Api/FileService.Api.csproj 2>/dev/null || true
	dotnet clean src/services/pet-service/src/PetService.Api/PetService.Api.csproj 2>/dev/null || true
	dotnet clean src/services/notification-service/src/NotificationService.Api/NotificationService.Api.csproj 2>/dev/null || true
	find . -type d -name bin -o -name obj | xargs rm -rf 2>/dev/null || true

restore:
	@echo "Restoring NuGet packages..."
	dotnet restore src/api-gateway/Gateway.Api/Gateway.Api.csproj
	dotnet restore src/services/identity-service/src/IdentityService.Api/IdentityService.Api.csproj
	dotnet restore src/services/user-service/src/UserProfileService.Api/UserProfileService.Api.csproj
	dotnet restore src/services/file-service/src/FileService.Api/FileService.Api.csproj
	dotnet restore src/services/pet-service/src/PetService.Api/PetService.Api.csproj
	dotnet restore src/services/notification-service/src/NotificationService.Api/NotificationService.Api.csproj
	dotnet restore tests/IntegrationTests/IntegrationTests.csproj

docker-up:
	@echo "Starting Docker containers..."
	docker-compose up -d

docker-down:
	@echo "Stopping Docker containers..."
	docker-compose down

db-migrate:
	@echo "Running database migrations..."
	# Run migrations for all services that have them
	dotnet ef database update --project src/services/identity-service/src/IdentityService.Infrastructure
	dotnet ef database update --project src/services/user-service/src/UserProfileService.Infrastructure
	dotnet ef database update --project src/services/file-service/src/FileService.Infrastructure
	dotnet ef database update --project src/services/pet-service/src/PetService.Infrastructure

logs:
	@echo "Showing Docker logs..."
	docker-compose logs -f

.DEFAULT_GOAL := help