# Makefile for Pet App Backend
# Common development commands

.PHONY: help build rebuild run test test-verbose test-coverage test-gateway test-identity test-pet clean restore db-migrate db-migrations-add db-migrations-apply docker-up docker-down logs health-check setup init full-restart

help:
	@echo "Pet App Backend - Build Commands"
	@echo ""
	@echo "Usage: make [target]"
	@echo ""
	@echo "Build & Test:"
	@echo "  build              - Build the entire solution (Release)"
	@echo "  rebuild            - Clean and rebuild the entire solution"
	@echo "  clean              - Clean build artifacts"
	@echo "  restore            - Restore NuGet packages"
	@echo "  test               - Run all tests"
	@echo "  test-verbose       - Run all tests with detailed output"
	@echo "  test-coverage      - Run tests and generate coverage reports"
	@echo ""
	@echo "Docker & Services:"
	@echo "  docker-up          - Start Docker containers (PostgreSQL, RabbitMQ, Elasticsearch, Kibana, Jaeger)"
	@echo "  docker-down        - Stop Docker containers"
	@echo "  logs               - View Docker logs (follow mode)"
	@echo "  health-check       - Check health of running services"
	@echo ""
	@echo "Database:"
	@echo "  db-migrate         - Run database migrations (requires .env with connection strings)"
	@echo "  db-migrations-add  - Create new migrations for pending changes (requires .env)"
	@echo "  db-migrations-apply - Apply pending migrations (alias for db-migrate)"
	@echo ""
	@echo "Environment Setup:"
	@echo "  setup              - Full setup: restore → build → docker-up → db-migrate"
	@echo "  init               - Alias for setup"
	@echo "  full-restart       - Full restart: docker-down → rebuild → docker-up → db-migrate"
	@echo ""
	@echo "Local Development Workflow:"
	@echo "  1. cp .env.example .env                    # Configure local environment"
	@echo "  2. Edit .env with your database/service settings"
	@echo "  3. make setup                              # Initial setup (restore → build → docker-up → db-migrate)"
	@echo "  4. ASPNETCORE_ENVIRONMENT=Development dotnet run  # Run a service in another terminal"
	@echo ""
	@echo "Logs & Monitoring:"
	@echo "  - Kibana (logs):    http://localhost:5601"
	@echo "  - Jaeger (traces):  http://localhost:16686"
	@echo "  - RabbitMQ (events): http://localhost:15672"
	@echo ""

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

# Service management
docker-up:
	@echo "Starting Docker containers..."
	docker-compose up -d --build

docker-down:
	@echo "Stopping Docker containers..."
	docker-compose down

logs:
	@echo "Showing Docker logs (press Ctrl+C to stop)..."
	docker-compose logs -f

health-check:
	@echo "Checking service health..."
	@./scripts/health-check.sh

# Database management
db-migrate:
	@echo "Running database migrations..."
	@if [ ! -f .env ]; then echo "⚠️  .env file not found. Copy .env.example to .env and configure."; exit 1; fi
	@bash -c 'set -a; source .env; set +a; \
	echo "Migrations for Identity Service..."; \
	dotnet ef database update --project src/services/identity-service/src/IdentityService.Infrastructure --startup-project src/services/identity-service/src/IdentityService.Api && \
	echo "Migrations for User Profile Service..."; \
	dotnet ef database update --project src/services/user-service/src/UserProfileService.Infrastructure --startup-project src/services/user-service/src/UserProfileService.Api && \
	echo "Migrations for File Service..."; \
	dotnet ef database update --project src/services/file-service/src/FileService.Infrastructure --startup-project src/services/file-service/src/FileService.Api && \
	echo "Migrations for Pet Service..."; \
	dotnet ef database update --project src/services/pet-service/src/PetService.Infrastructure --startup-project src/services/pet-service/src/PetService.Api'

db-migrations-add:
	@echo "Creating new migrations for all services with pending changes..."
	@if [ ! -f .env ]; then echo "⚠️  .env file not found. Copy .env.example to .env and configure."; exit 1; fi
	@bash -c 'set -a; source .env; set +a; \
	echo ""; \
	echo "=== Identity Service ==="; \
	dotnet ef migrations add "PendingChanges" --project src/services/identity-service/src/IdentityService.Infrastructure --startup-project src/services/identity-service/src/IdentityService.Api 2>&1 | grep -E "Done|no changes|pending" || true; \
	echo ""; \
	echo "=== User Profile Service ==="; \
	dotnet ef migrations add "PendingChanges" --project src/services/user-service/src/UserProfileService.Infrastructure --startup-project src/services/user-service/src/UserProfileService.Api 2>&1 | grep -E "Done|no changes|pending" || true; \
	echo ""; \
	echo "=== File Service ==="; \
	dotnet ef migrations add "PendingChanges" --project src/services/file-service/src/FileService.Infrastructure --startup-project src/services/file-service/src/FileService.Api 2>&1 | grep -E "Done|no changes|pending" || true; \
	echo ""; \
	echo "=== Pet Service ==="; \
	dotnet ef migrations add "PendingChanges" --project src/services/pet-service/src/PetService.Infrastructure --startup-project src/services/pet-service/src/PetService.Api 2>&1 | grep -E "Done|no changes|pending" || true; \
	echo ""; \
	echo "✓ Migration creation complete!"'

db-migrations-apply: db-migrate
	@echo "Migrations applied!"

# Development workflow
setup: restore build docker-up db-migrate
	@echo ""
	@echo "✓ Setup complete! Infrastructure is running."
	@echo ""
	@echo "Next steps:"
	@echo "  1. Run: ASPNETCORE_ENVIRONMENT=Development dotnet run (in each service)"
	@echo ""

init: setup
	@echo "✓ Development environment initialized!"

full-restart: docker-down rebuild docker-up db-migrate
	@echo "✓ Full restart complete! Infrastructure is running."

.DEFAULT_GOAL := help