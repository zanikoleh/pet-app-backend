#!/bin/bash

# Start Docker Compose
docker-compose up --build -d

# Wait for services to be healthy
echo "Waiting for services to start..."
sleep 30

# Run tests
echo "Running integration tests..."
dotnet test tests/IntegrationTests/IntegrationTests.csproj

# Check if tests passed
if [ $? -eq 0 ]; then
    echo "✓ All tests passed!"
    echo ""
    echo "Services are running at:"
    echo "  - API Gateway: https://localhost:44300"
    echo "  - Identity Service: https://localhost:44301"
    echo "  - User Profile Service: https://localhost:44302"
    echo "  - File Service: https://localhost:44303"
    echo "  - Notification Service: https://localhost:44304"
    echo "  - Pet Service: http://localhost:5000"
    echo ""
    echo "SQL Server: localhost:1433 (User: sa, Password: PetApp123!@#)"
    echo "RabbitMQ: http://localhost:15672 (User: guest, Password: guest)"
else
    echo "✗ Tests failed!"
    docker-compose down
    exit 1
fi
