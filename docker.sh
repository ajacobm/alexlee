#!/bin/bash

# Alex Lee Exercise - Docker Development Scripts

echo "🚀 Alex Lee Exercise - Docker Management"
echo "========================================="

case $1 in
  "dev")
    echo "🔨 Building and starting development environment..."
    docker-compose down
    docker-compose up --build
    ;;
  "prod")
    echo "📦 Building and starting production environment..."
    docker-compose -f docker-compose.prod.yml down
    docker-compose -f docker-compose.prod.yml up --build -d
    ;;
  "stop")
    echo "🛑 Stopping all containers..."
    docker-compose down
    docker-compose -f docker-compose.prod.yml down
    ;;
  "clean")
    echo "🧹 Cleaning up Docker resources..."
    docker-compose down -v
    docker-compose -f docker-compose.prod.yml down -v
    docker system prune -f
    ;;
  "logs")
    echo "📄 Showing logs..."
    if [ "$2" = "prod" ]; then
      docker-compose -f docker-compose.prod.yml logs -f
    else
      docker-compose logs -f
    fi
    ;;
  "status")
    echo "📊 Showing container status..."
    docker ps -a --filter "name=alexlee"
    ;;
  "test")
    echo "🧪 Running backend tests..."
    cd backend
    dotnet test
    ;;
  "build")
    echo "🔨 Building images only..."
    if [ "$2" = "prod" ]; then
      docker-compose -f docker-compose.prod.yml build
    else
      docker-compose build
    fi
    ;;
  *)
    echo "Usage: ./docker.sh [command]"
    echo ""
    echo "Commands:"
    echo "  dev     - Build and run development environment (hot reload)"
    echo "  prod    - Build and run production environment (optimized)"
    echo "  stop    - Stop all containers"
    echo "  clean   - Clean up Docker resources"
    echo "  logs    - Show container logs (add 'prod' for production logs)"
    echo "  status  - Show container status"
    echo "  test    - Run backend unit tests"
    echo "  build   - Build images only (add 'prod' for production build)"
    echo ""
    echo "Examples:"
    echo "  ./docker.sh dev      # Start development environment"
    echo "  ./docker.sh prod     # Start production environment"
    echo "  ./docker.sh logs     # Show development logs"
    echo "  ./docker.sh logs prod # Show production logs"
    ;;
esac
