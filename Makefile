# Alex Lee Exercise - Docker Makefile
# Cross-platform build and deployment automation

.PHONY: help dev prod stop clean logs status test build

# Default target
help:
	@echo "🚀 Alex Lee Exercise - Docker Management"
	@echo "========================================="
	@echo "Available commands:"
	@echo "  make dev     - Build and run development environment"
	@echo "  make prod    - Build and run production environment"
	@echo "  make stop    - Stop all containers"
	@echo "  make clean   - Clean up Docker resources"
	@echo "  make logs    - Show development logs"
	@echo "  make logs-prod - Show production logs"
	@echo "  make status  - Show container status"
	@echo "  make test    - Run backend unit tests"
	@echo "  make build   - Build development images"
	@echo "  make build-prod - Build production images"

# Development environment
dev:
	@echo "🔨 Building and starting development environment..."
	docker-compose down
	docker-compose up --build

# Production environment
prod:
	@echo "📦 Building and starting production environment..."
	docker-compose -f docker-compose.prod.yml down
	docker-compose -f docker-compose.prod.yml up --build -d

# Stop all containers
stop:
	@echo "🛑 Stopping all containers..."
	docker-compose down
	docker-compose -f docker-compose.prod.yml down

# Clean up Docker resources
clean:
	@echo "🧹 Cleaning up Docker resources..."
	docker-compose down -v
	docker-compose -f docker-compose.prod.yml down -v
	docker system prune -f

# Show development logs
logs:
	@echo "📄 Showing development logs..."
	docker-compose logs -f

# Show production logs
logs-prod:
	@echo "📄 Showing production logs..."
	docker-compose -f docker-compose.prod.yml logs -f

# Show container status
status:
	@echo "📊 Showing container status..."
	docker ps -a --filter "name=alexlee"

# Run backend tests
test:
	@echo "🧪 Running backend tests..."
	cd backend && dotnet test

# Build development images
build:
	@echo "🔨 Building development images..."
	docker-compose build

# Build production images
build-prod:
	@echo "🔨 Building production images..."
	docker-compose -f docker-compose.prod.yml build
