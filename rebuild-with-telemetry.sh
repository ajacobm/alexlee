#!/bin/bash

echo "🔨 Rebuilding Alex Lee containers with telemetry features..."

# Stop and remove existing containers
echo "📦 Stopping existing containers..."
docker-compose down

# Remove existing images to force rebuild
echo "🗑️ Removing old images..."
docker rmi alex-lee_backend:latest 2>/dev/null || true
docker rmi alex-lee_frontend:latest 2>/dev/null || true

# Build with no cache to ensure fresh build
echo "🏗️ Building backend container..."
docker-compose build --no-cache backend

echo "🏗️ Building frontend container..."  
docker-compose build --no-cache frontend

# Start the full environment
echo "🚀 Starting development environment..."
docker-compose up -d

# Wait for services to be ready
echo "⏳ Waiting for services to start..."
sleep 30

# Check service health
echo "🔍 Checking service health..."
curl -f http://localhost:5000/health && echo "✅ Backend healthy"
curl -f http://localhost:5000/api/telemetry/stats && echo "✅ Telemetry API responding"
curl -f http://localhost:3000 && echo "✅ Frontend responding"

echo "🎉 Rebuild complete! Services available at:"
echo "  🌐 Frontend: http://localhost:3000"
echo "  🔗 API: http://localhost:5000" 
echo "  📊 Swagger: http://localhost:5000/index.html"
echo "  📈 Telemetry: http://localhost:3000/telemetry"