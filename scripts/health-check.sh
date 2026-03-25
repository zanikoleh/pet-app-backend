#!/bin/bash

# Pet App Backend Health Check Script
# Validates all microservices are healthy and accessible through the API Gateway
# All health endpoints are accessed via the gateway at http://localhost:44300

GATEWAY_HOST="${GATEWAY_HOST:-localhost}"
GATEWAY_PORT="${GATEWAY_PORT:-44300}"
GATEWAY_BASE_URL="http://${GATEWAY_HOST}:${GATEWAY_PORT}"

# ANSI color codes
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

# Track health check results
TOTAL_CHECKS=0
PASSED_CHECKS=0
FAILED_CHECKS=0
FAILED_SERVICES=()

echo ""
echo -e "${BLUE}╔════════════════════════════════════════════════════════════╗${NC}"
echo -e "${BLUE}║  Pet App Backend - Service Health Check via Gateway        ║${NC}"
echo -e "${BLUE}╚════════════════════════════════════════════════════════════╝${NC}"
echo ""

# Check if curl is installed
if ! command -v curl &> /dev/null; then
    echo -e "${RED}❌ Error: curl is not installed. Please install curl to run this script.${NC}"
    exit 1
fi

check_service_health() {
    local service_name=$1
    local endpoint=$2
    local full_url="$GATEWAY_BASE_URL$endpoint"
    
    TOTAL_CHECKS=$((TOTAL_CHECKS + 1))
    
    echo -n "Checking $service_name at $endpoint... "
    
    # Send request and extract HTTP status code
    status_code=$(curl -s -o /dev/null -w "%{http_code}" "$full_url" 2>/dev/null)
    
    if [ "$status_code" = "200" ]; then
        echo -e "${GREEN}✅ Healthy (HTTP $status_code)${NC}"
        PASSED_CHECKS=$((PASSED_CHECKS + 1))
    else
        echo -e "${RED}❌ Unhealthy (HTTP $status_code)${NC}"
        FAILED_CHECKS=$((FAILED_CHECKS + 1))
        FAILED_SERVICES+=("$service_name")
    fi
}

check_docker_container_status() {
    local container_name=$1
    
    if docker ps 2>/dev/null | grep -q "$container_name"; then
        echo -e "${GREEN}✅ Running${NC}"
        return 0
    else
        echo -e "${RED}❌ Not running${NC}"
        return 1
    fi
}

# First, check if services are running
echo -e "${YELLOW}📦 Docker Container Status:${NC}"
check_docker_container() {
    local name=$1
    if docker ps 2>/dev/null | grep -q "$name"; then
        echo -e "  $name: ${GREEN}✅ Running${NC}"
    else
        echo -e "  $name: ${RED}❌ Not running${NC}"
    fi
}
check_docker_container "postgres"
check_docker_container "rabbitmq"
check_docker_container "identity-service"
check_docker_container "user-profile-service"
check_docker_container "pet-service"
check_docker_container "file-service"
check_docker_container "notification-service"
check_docker_container "api-gateway"

echo ""
echo -e "${YELLOW}🌐 Service Health via Gateway (${GATEWAY_BASE_URL}):${NC}"

# Check each service through the API Gateway
check_service_health "API Gateway"          "/health"
check_service_health "Identity Service"     "/identity/health"
check_service_health "User Profile Service" "/profiles/health"
check_service_health "Pet Service"          "/pets/health"
check_service_health "File Service"         "/files/health"
check_service_health "Notification Service" "/notifications/health"

echo ""
echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
echo -e "Total Checks: $TOTAL_CHECKS | ${GREEN}Passed: $PASSED_CHECKS${NC} | ${RED}Failed: $FAILED_CHECKS${NC}"

if [ $FAILED_CHECKS -eq 0 ]; then
    echo -e "${GREEN}✅ All services are healthy!${NC}"
    echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
    echo ""
    exit 0
else
    echo -e "${RED}❌ Some services are not responding correctly:${NC}"
    for service in "${FAILED_SERVICES[@]}"; do
        echo -e "   ${RED}• $service${NC}"
    done
    echo -e "${BLUE}════════════════════════════════════════════════════════════${NC}"
    echo ""
    echo "Troubleshooting steps:"
    echo "1. Ensure all containers are running: docker-compose ps"
    echo "2. Check container logs: docker-compose logs [service-name]"
    echo "3. Verify gateway configuration: src/api-gateway/Gateway.Api/yarpconfig.json"
    echo "4. Make sure services are healthy: docker-compose up -d"
    echo ""
    exit 1
fi
