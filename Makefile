# Alex Lee Developer Exercise - SQL Server Express Edition
# Makefile for managing the full-stack application with SQL Server

.PHONY: help dev prod stop down clean logs logs-prod test status sql-shell sql-status backup-db test-api

# Default target
help: ## Show this help message
	@echo "Alex Lee Developer Exercise - SQL Server Express Edition"
	@echo "========================================================="
	@echo ""
	@echo "Available commands:"
	@echo ""
	@awk 'BEGIN {FS = ":.*##"} /^[a-zA-Z_-]+:.*##/ {printf "  \033[36m%-15s\033[0m %s\n", $$1, $$2}' $(MAKEFILE_LIST)

# Development environment with SQL Server Express
dev: ## Start development environment (SQL Server + Backend + Frontend)
	@echo "🚀 Starting Alex Lee development environment with SQL Server Express..."
	@./dev.sh dev

# Production environment  
prod: ## Start production environment
	@echo "🏭 Starting Alex Lee production environment..."
	@./dev.sh prod

# Stop all containers
stop: ## Stop all running containers
	@echo "⏹️  Stopping all containers..."
	@./dev.sh stop

# Stop and remove containers
down: ## Stop and remove all containers and networks
	@echo "⏬ Stopping and removing containers..."
	@./dev.sh down

# Full cleanup
clean: ## Remove all containers, volumes, and images (destructive)
	@echo "🧹 Performing full cleanup..."
	@./dev.sh clean

# Show logs
logs: ## Show development environment logs
	@echo "📋 Showing development logs..."
	@./dev.sh logs

logs-prod: ## Show production environment logs  
	@echo "📋 Showing production logs..."
	@docker-compose -f docker-compose.prod.yml logs -f

# Run backend tests
test: ## Run backend unit tests (29 tests)
	@echo "🧪 Running backend unit tests..."
	@cd backend && dotnet test --verbosity normal

# Show container status
status: ## Show status of all containers
	@echo "📊 Container status:"
	@docker ps --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
	@echo ""
	@./dev.sh sql-status

# SQL Server management
sql-shell: ## Connect to SQL Server interactive shell
	@echo "💾 Connecting to SQL Server shell..."
	@./dev.sh sql-shell

sql-status: ## Check SQL Server connection and database status
	@./dev.sh sql-status

backup-db: ## Backup SQL Server database
	@./dev.sh backup-db

# API testing
test-api: ## Test all API endpoints including stored procedures
	@echo "🧪 Testing API endpoints..."
	@./dev.sh test-api

test-file-search: ## Test file search functionality specifically
	@./dev.sh file-search

# Build commands
build-backend: ## Build backend Docker image only
	@echo "🔨 Building backend Docker image..."
	@docker build -t alexlee-backend ./backend

build-frontend: ## Build frontend Docker image only  
	@echo "🔨 Building frontend Docker image..."
	@docker build -t alexlee-frontend ./frontend

build-all: build-backend build-frontend ## Build all Docker images

# Development helpers
watch-logs: ## Watch logs in real-time
	@./dev.sh logs

debug-sql: ## Debug SQL Server with detailed information
	@echo "🔍 SQL Server Debug Information:"
	@echo "================================"
	@./dev.sh sql-status
	@echo ""
	@echo "📋 Recent SQL Server logs:"
	@docker logs alexlee-sqlserver --tail 20

# Quick verification
verify: ## Quick verification that everything is working
	@echo "✅ Verifying Alex Lee environment..."
	@echo ""
	@echo "🔍 Container status:"
	@docker ps --filter "name=alexlee" --format "table {{.Names}}\t{{.Status}}\t{{.Ports}}"
	@echo ""
	@echo "🗄️  SQL Server status:"
	@./dev.sh sql-status
	@echo ""
	@echo "🔗 Testing API endpoints:" 
	@curl -s -f http://localhost:5000/health && echo "✅ Health endpoint OK" || echo "❌ Health endpoint failed"
	@curl -s -f http://localhost:5000/api/purchasedetails >/dev/null && echo "✅ Purchase details API OK" || echo "❌ Purchase details API failed"
	@curl -s -f http://localhost:3000 >/dev/null && echo "✅ Frontend OK" || echo "❌ Frontend failed"

# Environment info
info: ## Show environment information
	@echo "Alex Lee Developer Exercise - Environment Information"
	@echo "===================================================="
	@echo ""
	@echo "🐳 Docker version:"
	@docker --version
	@echo ""
	@echo "🐳 Docker Compose version:"
	@docker-compose --version
	@echo ""
	@echo "📁 Project structure:"
	@echo "  SQL Server Express in Docker"
	@echo "  .NET 8 Backend API"
	@echo "  React.js Frontend" 
	@echo "  Cross-platform file search"
	@echo ""
	@echo "🗄️  Database: SQL Server Express 2022"
	@echo "📊 Tests: 29 unit tests + API integration"
	@echo "🎨 UI: Alex Lee corporate branding"

# Reset development environment
reset: down dev ## Reset development environment (down + dev)
	@echo "🔄 Development environment reset complete!"

# Complete setup for new users
setup: info dev verify ## Complete setup (info + dev + verify)
	@echo ""
	@echo "🎉 Alex Lee Developer Exercise setup complete!"
	@echo ""
	@echo "📖 Available endpoints:"
	@echo "  🌐 Frontend: http://localhost:3000"
	@echo "  🔗 Backend API: http://localhost:5000" 
	@echo "  📊 Swagger docs: http://localhost:5000/index.html"
	@echo "  🗄️  SQL Server: localhost:1433 (SA/P@ssw0rd123!)"
	@echo ""
	@echo "🔧 Useful commands:"
	@echo "  make sql-shell    # SQL Server interactive shell"
	@echo "  make test-api     # Test all API endpoints"
	@echo "  make logs         # View application logs"
	@echo "  make help         # Show all commands"