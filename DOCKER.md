# Docker Deployment Guide

This guide covers containerization and deployment of the Alex Lee Exercise application.

## 📋 Overview

The application is containerized with:
- **Backend**: .NET 8 WebAPI in multi-stage Docker build
- **Frontend**: React app served via Nginx with API proxying
- **Database**: SQLite embedded with persistent volumes
- **Orchestration**: Docker Compose for development/production

## 🏗️ Architecture

```
┌─────────────────┐    ┌─────────────────┐
│   Frontend      │    │    Backend      │
│  (Nginx)        │────│   (.NET 8)      │
│  Port 80/3000   │    │   Port 5000     │
└─────────────────┘    └─────────────────┘
                              │
                       ┌─────────────────┐
                       │    SQLite       │
                       │  (Volume)       │
                       └─────────────────┘
```

## 🚀 Quick Commands

### Development
```bash
# Start development environment
make dev
# or
docker-compose up --build

# Stop development environment  
make stop

# View logs
make logs
```

### Production
```bash
# Start production environment
make prod
# or  
docker-compose -f docker-compose.prod.yml up --build -d

# View production logs
make logs-prod

# Stop production environment
docker-compose -f docker-compose.prod.yml down
```

### Maintenance
```bash
# Clean up resources
make clean

# Check container status
make status

# Run tests
make test
```

## 🔧 Configuration

### Environment Variables

**Backend:**
- `ASPNETCORE_ENVIRONMENT`: Development/Production
- `ASPNETCORE_URLS`: http://+:5000
- `ConnectionStrings__DefaultConnection`: SQLite connection

**Frontend:**
- `NODE_ENV`: development/production
- API proxying handled by Nginx in production

### Ports

**Development:**
- Frontend: `localhost:3000`
- Backend: `localhost:5000` 
- Database: SQLite file

**Production:**
- Application: `localhost:80`
- Backend internal: `5000`
- Database: SQLite volume

## 📁 File Structure

```
docker/
├── docker-compose.yml          # Development environment
├── docker-compose.prod.yml     # Production environment  
├── backend/
│   ├── Dockerfile             # .NET 8 multi-stage build
│   └── .dockerignore          # Backend ignore rules
├── frontend/
│   ├── Dockerfile             # React + Nginx build
│   ├── nginx.conf             # Nginx configuration
│   └── .dockerignore          # Frontend ignore rules
├── docker.sh                  # Management script
└── Makefile                   # Cross-platform commands
```

## 🏭 Build Process

### Backend Build Stages
1. **Base**: Runtime image (mcr.microsoft.com/dotnet/aspnet:8.0)
2. **Build**: SDK image for compilation
3. **Publish**: Optimized release build
4. **Final**: Runtime with published app

### Frontend Build Stages  
1. **Base**: Node.js 20 Alpine
2. **Dependencies**: npm install
3. **Build**: Production React build
4. **Production**: Nginx Alpine with static files

## 🔒 Security

### Container Security
- Non-root users in containers
- Minimal base images (Alpine Linux)
- Security headers in Nginx
- Health checks for monitoring

### Network Security
- Internal Docker network
- No exposed database ports
- API proxying through Nginx
- CORS properly configured

## 📊 Monitoring

### Health Checks
- Backend: `GET /health`
- Frontend: Nginx status
- Database: SQLite file accessibility

### Logging
```bash
# Development logs
docker-compose logs -f

# Production logs  
docker-compose -f docker-compose.prod.yml logs -f

# Specific service logs
docker-compose logs -f backend
docker-compose logs -f frontend
```

## 🔄 CI/CD Integration

### GitHub Actions
```yaml
# Build and test Docker images
- name: Build Docker Images
  run: |
    docker-compose build
    docker-compose up -d
    docker-compose exec backend dotnet test
```

### Registry Push
```bash
# Tag and push images
docker tag alexlee-backend:latest registry.com/alexlee-backend:v1.0
docker push registry.com/alexlee-backend:v1.0
```

## 🐛 Troubleshooting

### Common Issues

**Port Conflicts:**
```bash
# Check port usage
sudo netstat -tlnp | grep :5000
sudo netstat -tlnp | grep :3000

# Kill processes if needed
sudo kill -9 <PID>
```

**Permission Issues:**
```bash
# Fix file permissions
chmod +x docker.sh
chmod 644 docker-compose*.yml
```

**Database Issues:**
```bash
# Reset database volume
docker-compose down -v
docker volume rm alexlee_backend-data
docker-compose up --build
```

**Build Cache Issues:**
```bash
# Force rebuild without cache
docker-compose build --no-cache
docker system prune -f
```

### Log Analysis
```bash
# Container status
docker ps -a --filter "name=alexlee"

# Resource usage
docker stats --format "table {{.Container}}\t{{.CPUPerc}}\t{{.MemUsage}}"

# Network inspection
docker network inspect alexlee_alexlee-network
```

## 🚀 Production Deployment

### Cloud Deployment
```bash
# AWS ECS/Fargate
aws ecs create-service --service-definition service.json

# Azure Container Instances  
az container create --resource-group myResourceGroup --file deploy.yaml

# Google Cloud Run
gcloud run deploy alexlee --image gcr.io/project/alexlee
```

### Environment Variables
```bash
# Production environment
export ASPNETCORE_ENVIRONMENT=Production
export ConnectionStrings__DefaultConnection="Production DB String"
export JWT_SECRET="Production Secret Key"
```

## 📈 Performance Optimization

### Image Size Optimization
- Multi-stage builds reduce final image size
- Alpine Linux base images
- .dockerignore excludes unnecessary files
- Minimal runtime dependencies

### Runtime Performance
- SQLite for smaller deployments
- Nginx for static file serving
- Docker health checks
- Resource limits in production

---

For additional Docker help, run:
```bash
./docker.sh      # See all available commands
make help        # Cross-platform command reference
```
