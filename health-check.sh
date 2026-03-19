#!/bin/bash

# Docker Compose Health Check Script
# Monitors the health of all running services

echo "🔍 Checking Pet App Backend Services..."
echo ""

GATEWAY_URL="https://localhost:44300/health"
IDENTITY_URL="https://localhost:44301/health"
PROFILE_URL="https://localhost:44302/health"
FILE_URL="https://localhost:44303/health"
NOTIFICATION_URL="https://localhost:44304/health"
PET_URL="http://localhost:5000/health"

check_service() {
    local name=$1
    local url=$2
    
    echo -n "Checking $name... "
    
    if [[ $url == https* ]]; then
        status=$(curl -s -o /dev/null -w "%{http_code}" -k "$url")
    else
        status=$(curl -s -o /dev/null -w "%{http_code}" "$url")
    fi
    
    if [ "$status" = "200" ]; then
        echo "✅ Healthy (Status: $status)"
    else
        echo "❌ Unhealthy (Status: $status)"
    fi
}

check_docker_service() {
    local container=$1
    echo -n "Container $container: "
    
    if docker ps | grep -q "$container"; then
        echo "✅ Running"
    else
        echo "❌ Not running"
    fi
}

echo "📦 Container Status:"
check_docker_service "sqlserver"
check_docker_service "rabbitmq"
check_docker_service "identity-service"
check_docker_service "user-profile-service"
check_docker_service "pet-service"
check_docker_service "file-service"
check_docker_service "notification-service"
check_docker_service "api-gateway"

echo ""
echo "🌐 Service Health Checks:"
check_service "API Gateway" "$GATEWAY_URL"
check_service "Identity Service" "$IDENTITY_URL"
check_service "User Profile Service" "$PROFILE_URL"
check_service "File Service" "$FILE_URL"
check_service "Notification Service" "$NOTIFICATION_URL"
check_service "Pet Service" "$PET_URL"

echo ""
echo "✨ Health check complete!"
