# Makefile for Pet App Backend
# Common development commands

.PHONY: help build run test test-verbose test-coverage test-gateway test-identity test-pet clean restore db-migrate db-migrations-add db-migrations-apply docker-up docker-down logs setup init full-restart

help:
	@echo "Pet App Backend - Build Commands"
	@echo ""
	@echo "Usage: make [target]"
	@echo ""
	@echo "Targets:"
	@echo "  build              - Build the solution"
	@echo "  run                - Run all services"
	@echo "  test               - Run all tests"
	@echo "  test-verbose       - Run all tests with detailed output"
	@echo "  test-coverage      - Run tests with code coverage"
	@echo "  test-gateway       - Run Gateway and Integration tests only"
	@echo "  test-identity      - Run Identity Service tests only"
	@echo "  test-pet           - Run Pet Service tests only"
	@echo "  clean              - Clean build artifacts"
	@echo "  restore            - Restore NuGet packages"
	@echo "  docker-up          - Start Docker containers"
	@echo "  docker-down        - Stop Docker containers"
	@echo "  db-migrate         - Run database migrations"
	@echo "  db-migrations-add  - Create new migrations for all services with pending changes"
	@echo "  db-migrations-apply - Apply all pending migrations to the database"
	@echo "  logs               - View Docker logs"
	@echo "  setup              - Setup development environment"
	@echo "  init               - Initialize development environment"
	@echo "  full-restart       - Full restart of development environment"

setup: restore build docker-up db-migrate
	@echo "Setup complete! Services are running."

init: setup
	@echo "Ready for development!"

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
	@echo "Running all test projects..."
	@echo ""
	@echo "=== Gateway Tests ==="
	@echo "Running Gateway.Api.Tests..."
	-dotnet test tests/api-gateway/Gateway.Api.Tests/Gateway.Api.Tests.csproj -v minimal
	@echo "Running Integration Tests..."
	-dotnet test tests/api-gateway/IntegrationTests/IntegrationTests.csproj -v minimal
	@echo ""
	@echo "=== Identity Service Tests ==="
	@echo "Running IdentityService.Domain.Tests..."
	-dotnet test tests/identity-service/src/IdentityService.Domain.Tests/IdentityService.Domain.Tests.csproj -v minimal
	@echo "Running IdentityService.Application.Tests..."
	-dotnet test tests/identity-service/src/IdentityService.Application.Tests/IdentityService.Application.Tests.csproj -v minimal
	@echo "Running IdentityService.Infrastructure.Tests..."
	-dotnet test tests/identity-service/src/IdentityService.Infrastructure.Tests/IdentityService.Infrastructure.Tests.csproj -v minimal
	@echo ""
	@echo "=== Pet Service Tests ==="
	@echo "Running PetService.Domain.Tests..."
	-dotnet test tests/pet-service/src/PetService.Domain.Tests/PetService.Domain.Tests.csproj -v minimal
	@echo "Running PetService.Application.Tests..."
	-dotnet test tests/pet-service/src/PetService.Application.Tests/PetService.Application.Tests.csproj -v minimal
	@echo "Running PetService.Infrastructure.Tests..."
	-dotnet test tests/pet-service/src/PetService.Infrastructure.Tests/PetService.Infrastructure.Tests.csproj -v minimal
	@echo ""
	@echo "✓ All test projects completed!"

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

docker-up:
	@echo "Starting Docker containers..."
	docker-compose up -d --build

docker-down:
	@echo "Stopping Docker containers..."
	docker-compose down

db-migrate:
	@echo "Running database migrations..."
	# Run migrations for all services that have them
	dotnet ef database update --project src/services/identity-service/src/IdentityService.Infrastructure --startup-project src/services/identity-service/src/IdentityService.Api
	dotnet ef database update --project src/services/user-service/src/UserProfileService.Infrastructure --startup-project src/services/user-service/src/UserProfileService.Api
	dotnet ef database update --project src/services/file-service/src/FileService.Infrastructure --startup-project src/services/file-service/src/FileService.Api
	dotnet ef database update --project src/services/pet-service/src/PetService.Infrastructure --startup-project src/services/pet-service/src/PetService.Api

db-migrations-add:
	@echo "Creating new migrations for all services with pending changes..."
	@echo ""
	@echo "=== Identity Service ==="
	-dotnet ef migrations add "PendingChanges" --project src/services/identity-service/src/IdentityService.Infrastructure --startup-project src/services/identity-service/src/IdentityService.Api 2>&1 | grep -E "Done|no changes|pending" || true
	@echo ""
	@echo "=== User Profile Service ==="
	-dotnet ef migrations add "PendingChanges" --project src/services/user-service/src/UserProfileService.Infrastructure --startup-project src/services/user-service/src/UserProfileService.Api 2>&1 | grep -E "Done|no changes|pending" || true
	@echo ""
	@echo "=== File Service ==="
	-dotnet ef migrations add "PendingChanges" --project src/services/file-service/src/FileService.Infrastructure --startup-project src/services/file-service/src/FileService.Api 2>&1 | grep -E "Done|no changes|pending" || true
	@echo ""
	@echo "=== Pet Service ==="
	-dotnet ef migrations add "PendingChanges" --project src/services/pet-service/src/PetService.Infrastructure --startup-project src/services/pet-service/src/PetService.Api 2>&1 | grep -E "Done|no changes|pending" || true
	@echo ""
	@echo "✓ Migration creation complete!"

db-migrations-apply:
	@echo "Applying all pending migrations to the database..."
	$(MAKE) db-migrate

logs:
	@echo "Showing Docker logs..."
	docker-compose logs -f

test-verbose:
	@echo "Running all tests with verbose output..."
	-dotnet test --verbosity detailed

test-coverage:
	@echo "Running tests with verbose output and logging results..."
	@mkdir -p coverage
	-dotnet test tests/api-gateway/Gateway.Api.Tests/Gateway.Api.Tests.csproj --logger "trx;LogFileName=gateway.trx" --results-directory ./coverage -v minimal
	-dotnet test tests/identity-service/src/IdentityService.Domain.Tests/IdentityService.Domain.Tests.csproj --logger "trx;LogFileName=identity-domain.trx" --results-directory ./coverage -v minimal
	-dotnet test tests/identity-service/src/IdentityService.Application.Tests/IdentityService.Application.Tests.csproj --logger "trx;LogFileName=identity-app.trx" --results-directory ./coverage -v minimal
	-dotnet test tests/identity-service/src/IdentityService.Infrastructure.Tests/IdentityService.Infrastructure.Tests.csproj --logger "trx;LogFileName=identity-infra.trx" --results-directory ./coverage -v minimal
	-dotnet test tests/pet-service/src/PetService.Domain.Tests/PetService.Domain.Tests.csproj --logger "trx;LogFileName=pet-domain.trx" --results-directory ./coverage -v minimal
	-dotnet test tests/pet-service/src/PetService.Application.Tests/PetService.Application.Tests.csproj --logger "trx;LogFileName=pet-app.trx" --results-directory ./coverage -v minimal
	@echo "✓ Test results logged to ./coverage (TRX format)"
	@echo "Note: For code coverage analysis, install 'dotnet tool install -g dotnet-cover' and use 'dotnet-cover' command"

test-gateway:
	@echo "Running Gateway and Integration tests..."
	-dotnet test tests/api-gateway/Gateway.Api.Tests/Gateway.Api.Tests.csproj -v minimal
	-dotnet test tests/api-gateway/IntegrationTests/IntegrationTests.csproj -v minimal

test-identity:
	@echo "Running Identity Service tests..."
	-dotnet test tests/identity-service/src/IdentityService.Domain.Tests/IdentityService.Domain.Tests.csproj -v minimal
	-dotnet test tests/identity-service/src/IdentityService.Application.Tests/IdentityService.Application.Tests.csproj -v minimal
	-dotnet test tests/identity-service/src/IdentityService.Infrastructure.Tests/IdentityService.Infrastructure.Tests.csproj -v minimal

test-pet:
	@echo "Running Pet Service tests..."
	-dotnet test tests/pet-service/src/PetService.Domain.Tests/PetService.Domain.Tests.csproj -v minimal
	-dotnet test tests/pet-service/src/PetService.Application.Tests/PetService.Application.Tests.csproj -v minimal
	-dotnet test tests/pet-service/src/PetService.Infrastructure.Tests/PetService.Infrastructure.Tests.csproj -v minimal

full-restart: docker-down clean restore build docker-up db-migrate
	@echo "Full restart complete! Services are running."

.DEFAULT_GOAL := help