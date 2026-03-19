#!/bin/bash

echo "Stopping and removing containers..."
docker-compose down

echo "Removing volumes..."
docker-compose down -v

echo "✓ Cleanup complete!"
