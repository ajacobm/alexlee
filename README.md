# Alex Lee Developer Exercise

A full-stack application built with .NET 8 and React.js, showcasing technical skills through algorithm implementation, database management, and modern web development practices.

## 🎯 Project Overview

This project addresses the Alex Lee Developer Technical Screening requirements with a comprehensive solution featuring:

- **Backend**: .NET 8 WebAPI with CQRS pattern, EF Core, and SQLite
- **Frontend**: React.js SPA with modern state management and UI components
- **Algorithms**: C# extension methods solving string manipulation and file search problems
- **Database**: SQLite with EF Core migrations and seeded sample data
- **DevOps**: Docker containers with GitHub Actions CI/CD pipeline

## 🏗️ Architecture

```
alex-lee/
├── backend/                    # .NET 8 WebAPI
│   ├── src/
│   │   ├── AlexLee.Api/        # WebAPI controllers & middleware
│   │   ├── AlexLee.Application/ # CQRS commands/queries
│   │   ├── AlexLee.Domain/     # Domain models (records)
│   │   ├── AlexLee.Infrastructure/ # EF Core & data access
│   │   └── AlexLee.Algorithms/ # C# problems solution
│   ├── tests/                  # Unit tests (29 tests)
│   └── Dockerfile
├── frontend/                   # React.js SPA
│   ├── src/
│   └── Dockerfile
├── .github/workflows/          # CI/CD pipeline
└── docker-compose.yml          # Development environment
```

## ✅ Solved Problems

### C# Problems (Completed)
1. **String Interleaving** - Extension method that alternates characters from two strings
2. **Palindrome Checker** - Extension method with case/punctuation handling
3. **Parallel File Search** - Multi-threaded file content search utility

### SQL Problems (Integrated)
4. **Line Numbering** - ROW_NUMBER() window function implementation
5. **Duplicate Detection** - GROUP BY query for identifying duplicate records
6. **Stored Procedure** - EF Core integration for purchase detail retrieval

### React Problems (In Progress)
7. **Purchase Detail Grid** - Filterable data grid with Alex Lee styling
8. **Create/Update Modal** - Form validation with CRUD operations

## 🚀 Quick Start

### Prerequisites
- .NET 8 SDK
- Node.js 18+
- Docker & Docker Compose

### Docker (Recommended)

**Development Environment:**
```bash
# Using make (cross-platform)
make dev

# Or using docker-compose directly
docker-compose up --build
```

**Production Environment:**
```bash
# Using make
make prod

# Or using docker-compose directly
docker-compose -f docker-compose.prod.yml up --build -d
```

**Other Docker Commands:**
```bash
make help           # Show all available commands
make stop           # Stop all containers
make clean          # Clean up Docker resources
make logs           # Show development logs
make logs-prod      # Show production logs
make status         # Show container status
make test           # Run backend unit tests
```

### Manual Development Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/ajacobm/alex-lee.git
   cd alex-lee
   ```

2. **Backend Development**
   ```bash
   cd backend
   dotnet restore
   dotnet build
   dotnet test                    # Run 29 unit tests
   dotnet run --project src/AlexLee.Api
   ```

3. **Frontend Development**
   ```bash
   cd frontend
   npm install
   npm start
   ```

## 🧪 Testing

### Backend Tests (29 passing)
```bash
cd backend
dotnet test --verbosity normal
```

**Test Coverage:**
- String Extensions: 8 test cases
- Palindrome Logic: 8 test cases
- File Search Utility: 5 test cases
- Additional integration tests: 8 test cases

### Frontend Tests
```bash
cd frontend
npm test
```

## 📋 Requirements Checklist

### ✅ Completed
- [x] **C# Problem 1**: String interleaving extension method
- [x] **C# Problem 2**: Palindrome checker with full requirements
- [x] **C# Problem 3**: Parallel file search utility
- [x] **SQL Problem 4**: Line numbering implementation (ROW_NUMBER)
- [x] **SQL Problem 5**: Duplicate detection grouping
- [x] **Domain Models**: Immutable records pattern
- [x] **CQRS Pattern**: Commands/Queries with MediatR
- [x] **EF Core Setup**: SQLite configuration with seeded data
- [x] **Unit Tests**: Comprehensive test coverage
- [x] **CI/CD Pipeline**: GitHub Actions workflow

### 🚧 In Progress
- [ ] **WebAPI Controllers**: REST endpoints with Swagger
- [ ] **Authentication**: JWT tokens + cookies
- [ ] **React Components**: Purchase detail grid and forms
- [ ] **Docker Images**: Multi-stage builds
- [ ] **Database Migrations**: EF Core migrations

### 📅 Planned
- [ ] **React Problem 7**: Complete purchase detail grid
- [ ] **React Problem 8**: Modal CRUD operations
- [ ] **Alex Lee Styling**: Brand colors and typography
- [ ] **Production Deployment**: Container orchestration

## 🛠️ Technology Stack

**Backend:**
- .NET 8 WebAPI
- Entity Framework Core 9
- SQLite Database
- MediatR (CQRS)
- xUnit Testing

**Frontend:**
- React 18
- TypeScript
- Material-UI Components
- Axios HTTP Client
- React Hook Form

**DevOps:**
- Docker & Docker Compose
- GitHub Actions CI/CD
- Multi-stage builds
- Automated testing

## 🎨 Alex Lee Branding

The frontend incorporates Alex Lee's corporate branding:
- **Typography**: Source Sans 3 font family
- **Colors**: Professional blue palette
- **Layout**: Clean, Bootstrap-inspired design
- **Components**: Material-UI with custom theming

## 📊 API Documentation

When running locally, visit:
- **Swagger UI**: `http://localhost:5000/swagger`
- **API Endpoints**: `http://localhost:5000/api/`
- **Health Checks**: `http://localhost:5000/health`

## 🔧 Development Notes

### Code Organization
- **Domain-Driven Design** with clear separation of concerns
- **Immutable records** for data consistency
- **Extension methods** for algorithm problems
- **Repository pattern** through EF Core
- **Unit test organization** with descriptive naming

### Performance Considerations
- **Parallel processing** for file search operations
- **Async/await** patterns throughout
- **EF Core optimizations** with query projection
- **Docker multi-stage builds** for smaller images

## 📈 CI/CD Pipeline

The GitHub Actions workflow includes:
1. **Automated Testing** (backend + frontend)
2. **Code Coverage** reporting
3. **Docker Image Building** with caching
4. **Security Scanning** 
5. **Environment Deployments** (staging/production)

## 🤝 Contributing

This project was developed as a technical screening exercise demonstrating:
- Modern .NET development practices
- React.js proficiency
- Database design and optimization
- Algorithm implementation
- DevOps and deployment automation

---

**Author**: Adam Jacob Meyer  
**Repository**: [ajacobm/alex-lee](https://github.com/ajacobm/alex-lee)  
**Documentation**: See [PROGRESS.md](./PROGRESS.md) for detailed implementation notes