# Makefile for Pet App Backend
# Common development commands

.PHONY: help build run test test-verbose test-coverage test-gateway test-identity test-pet clean restore db-migrate db-migrations-add db-migrations-apply docker-up docker-down logs setup init full-restart

help:
	@echo "Pet App Backend - Build Commands"
	@echo ""
	@echo "Usage: make [target]"
	@echo ""
	@echo "Build & Test:"
	@echo "  build              - Build the entire solution"
	@echo "  rebuild            - Clean and rebuild the entire solution"
	@echo "  clean              - Clean build artifacts"
	@echo "  restore            - Restore NuGet packages"
	@echo "  test               - Run all tests"
	@echo "  test-verbose       - Run all tests with detailed output"
	@echo "  test-coverage      - Run tests and generate coverage reports"
	@echo "  test-gateway       - Run Gateway and Integration tests"
	@echo "  test-identity      - Run Identity Service tests"
	@echo "  test-pet           - Run Pet Service tests"
	@echo ""
	@echo "Docker & Services:"
	@echo "  run                - Start all services with Docker"
	@echo "  docker-up          - Start Docker containers"
	@echo "  docker-down        - Stop Docker containers"
	@echo "  logs               - View Docker logs"
	@echo ""
	@echo "Database:"
	@echo "  db-migrate         - Run database migrations"
	@echo "  db-migrations-add  - Create new migrations for pending changes"
	@echo "  db-migrations-apply - Apply pending migrations"
	@echo ""
	@echo "Environment:"
	@echo "  setup              - Full setup (restore, build, docker, migrations)"
	@echo "  init               - Initialize for development"
	@echo "  full-restart       - Full restart of development environment"

# Build targets
build:
	@echo "Building solution..."
	dotnet build pet-app-backend.slnx -c Release

rebuild: clean restore build
	@echo "Rebuild complete!"

clean:
	@echo "Cleaning build artifacts..."
	dotnet clean pet-app-backend.slnx 2>/dev/null || true
	find . -type d \( -name bin -o -name obj \) -exec rm -rf {} + 2>/dev/null || true

restore:
	@echo "Restoring NuGet packages..."
	dotnet restore pet-app-backend.slnx

# Test targets
test:
	@echo "Running all tests..."
	dotnet test pet-app-backend.slnx -v minimal --no-build

test-verbose:
	@echo "Running all tests with verbose output..."
	dotnet test pet-app-backend.slnx -v detailed --no-build

test-coverage:
	@echo "Running tests with coverage..."
	@mkdir -p coverage
	dotnet test pet-app-backend.slnx --logger "trx;LogFileName=coverage.trx" --results-directory ./coverage -v minimal --no-build
	@echo "✓ Test results logged to ./coverage (TRX format)"

test-gateway:
	@echo "Running Gateway tests..."
	-dotnet test tests/api-gateway/Gateway.Api.Tests/Gateway.Api.Tests.csproj -v minimal --no-build
	-dotnet test tests/api-gateway/IntegrationTests/IntegrationTests.csproj -v minimal --no-build

test-identity:
	@echo "Running Identity Service tests..."
	-dotnet test tests/identity-service/src/IdentityService.Domain.Tests/IdentityService.Domain.Tests.csproj -v minimal --no-build
	-dotnet test tests/identity-service/src/IdentityService.Application.Tests/IdentityService.Application.Tests.csproj -v minimal --no-build
	-dotnet test tests/identity-service/src/IdentityService.Infrastructure.Tests/IdentityService.Infrastructure.Tests.csproj -v minimal --no-build

test-pet:
	@echo "Running Pet Service tests..."
	-dotnet test tests/pet-service/src/PetService.Domain.Tests/PetService.Domain.Tests.csproj -v minimal --no-build
	-dotnet test tests/pet-service/src/PetService.Application.Tests/PetService.Application.Tests.csproj -v minimal --no-build
	-dotnet test tests/pet-service/src/PetService.Infrastructure.Tests/PetService.Infrastructure.Tests.csproj -v minimal --no-build

# Service management
run: docker-up
	@echo "Services running!"

docker-up:
	@echo "Starting Docker containers..."
	docker-compose up -d --build

docker-up:
	@echo "Starting Docker containers..."
	docker-compose up -d --build

docker-down:
	@echo "Stopping Docker containers..."
	docker-compose down

logs:
	@echo "Showing Docker logs..."
	docker-compose logs -f

# Database management
db-migrate:
	@echo "Running database migrations..."
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

db-migrations-apply: db-migrate
	@echo "Migrations applied!"

# Development workflow
setup: restore build docker-up db-migrate
	@echo "✓ Setup complete! Services are running."

init: setup
	@echo "✓ Ready for development!"

full-restart: docker-down clean restore build docker-up db-migrate
	@echo "✓ Full restart complete! Services are running."

.DEFAULT_GOAL := help