# Alex Lee Developer Exercise

A full-stack application built with .NET 8 and React.js, showcasing technical skills through algorithm implementation, SQL Server database management, and modern web development practices with enterprise-ready deployments.

## 🎯 Project Overview

This project addresses the Alex Lee Developer Technical Screening requirements with a comprehensive solution featuring:

- **Backend**: .NET 8 WebAPI with CQRS pattern, EF Core, and **SQL Server Express**
- **Frontend**: React.js SPA with modern state management and Alex Lee branding
- **Algorithms**: C# extension methods with **cross-platform file search** 
- **Database**: **SQL Server Express** in Docker with stored procedures and original SQLExerciseScript.sql data
- **DevOps**: Docker containers with volume mounting for Windows/Linux compatibility

## 🗄️ SQL Server Express Integration

**NEW**: This project now uses SQL Server Express instead of SQLite, providing:
- ✅ **Enterprise-grade database** running in Docker containers
- ✅ **Stored procedures** implementing SQL Exercise questions #4-6  
- ✅ **Automatic database initialization** using provided SQLExerciseScript.sql
- ✅ **Cross-platform file search** with Windows volume mounting
- ✅ **Production-ready deployment** with health checks and monitoring

📖 **[Complete SQL Server Integration Guide](./SQL_SERVER_INTEGRATION.md)**

## 🏗️ Architecture

```
alex-lee/
├── backend/                    # .NET 8 WebAPI
│   ├── src/
│   │   ├── AlexLee.Api/        # WebAPI controllers & middleware
│   │   ├── AlexLee.Application/ # CQRS commands/queries  
│   │   ├── AlexLee.Domain/     # Domain models (records)
│   │   ├── AlexLee.Infrastructure/ # EF Core & SQL Server
│   │   └── AlexLee.Algorithms/ # Enhanced file search utilities
│   ├── scripts/                # SQL Server initialization scripts
│   └── Dockerfile              # Multi-stage build with SQL Server support
├── frontend/                   # React.js SPA with Alex Lee branding
├── SQLExerciseScript.sql       # Original SQL Server data script
├── sql-server.sh               # SQL Server management helper script
└── docker-compose.yml          # SQL Server Express environment
```

## ✅ Solved Problems

### C# Problems (Completed ✅)
1. **String Interleaving** - Extension method with 8 unit tests
2. **Palindrome Checker** - Extension method with case/punctuation handling (8 tests)
3. **Parallel File Search** - Enhanced with Windows/Docker cross-platform support (5 tests)

### SQL Problems (Implemented with Stored Procedures ✅)
4. **Line Numbering** - `GetPurchaseDetailsWithLineNumbers` stored procedure
5. **Duplicate Detection** - `GetDuplicatePurchaseDetails` stored procedure  
6. **Stored Procedure Integration** - Direct EF Core stored procedure calls

### React Problems (Completed ✅)
7. **Purchase Detail Grid** - Professional UI with Alex Lee styling and filtering
8. **Create/Update Modal** - Form validation with CRUD operations

## 🚀 Quick Start

### SQL Server Express Environment (Recommended)

**Using Helper Script (Linux/macOS/WSL):**
```bash
# Make script executable
chmod +x sql-server.sh

# Start full SQL Server Express environment  
./sql-server.sh dev

# Check SQL Server status
./sql-server.sh sql-status

# Test API endpoints including stored procedures
./sql-server.sh test-api

# Access SQL Server shell for manual queries
./sql-server.sh sql-shell
```

**Using Docker Compose Directly:**
```bash
# Development with SQL Server Express
docker-compose up --build

# Production environment
./sql-server.sh prod
# OR: docker-compose -f docker-compose.prod.yml up -d
```

### Access Points
- **Frontend**: `http://localhost:3000` (Alex Lee styled React app)
- **Backend API**: `http://localhost:5000`
- **Swagger Documentation**: `http://localhost:5000/swagger`
- **SQL Server**: `localhost:1433` (SA/P@ssw0rd123!)

## 🗃️ SQL Server Features

### Database Structure
```sql
-- Automatically created from SQLExerciseScript.sql
CREATE TABLE dbo.PurchaseDetailItem (
    PurchaseDetailItemAutoId BIGINT IDENTITY(1,1) NOT NULL,
    PurchaseOrderNumber VARCHAR(20) NOT NULL,
    ItemNumber INT NOT NULL,
    ItemName VARCHAR(50) NOT NULL,
    ItemDescription VARCHAR(250),
    PurchasePrice DECIMAL(10,2) NOT NULL,
    PurchaseQuantity INT NOT NULL,
    LastModifiedByUser VARCHAR(50) NOT NULL,
    LastModifiedDateTime DATETIME NOT NULL
);
```

### API Endpoints for SQL Exercises
```bash
# Question #6: Purchase details with line numbers
curl "http://localhost:5000/api/purchasedetails/with-line-numbers"

# Question #5: Duplicate detection
curl "http://localhost:5000/api/purchasedetails/duplicates"

# Summary with stored procedure info
curl "http://localhost:5000/api/purchasedetails/summary"
```

### Stored Procedures
- `GetPurchaseDetailsWithLineNumbers` - Implements ROW_NUMBER() for Question #4
- `GetDuplicatePurchaseDetails` - Identifies duplicates for Question #5  
- `CheckDatabaseReady` - Health check utility

## 📁 Enhanced File Search (Problem #3)

**Cross-platform file search** with Windows/Docker support:

### Volume Mounting Strategy
```yaml
# Mounts Windows C:/Users to /app/search-files/windows in container
volumes:
  - C:/Users:/app/search-files/windows:ro
```

### API Usage
```bash
# Get available search paths (Windows/Docker aware)
curl "http://localhost:5000/api/algorithms/file-search/available-paths"

# Search in mounted Windows directory
curl -X POST "http://localhost:5000/api/algorithms/file-search" \
     -H "Content-Type: application/json" \
     -d '{"searchTerm":"TODO","directoryPath":"/app/search-files/windows/Documents"}'
```

## 🧪 Testing

### Backend Tests (29 passing)
```bash
# Using helper script
./sql-server.sh dev
sleep 10  # Wait for SQL Server
./sql-server.sh test-api

# Manual testing  
cd backend && dotnet test --verbosity normal
```

### Test Categories
- **String Extensions**: 8 test cases
- **Palindrome Logic**: 8 test cases  
- **File Search**: 5 test cases with cross-platform support
- **Integration Tests**: 8 test cases

### SQL Server Verification
```bash
# Manual SQL Server testing
./sql-server.sh sql-shell

# In SQL shell:
USE AlexLeeDB;
EXEC dbo.GetPurchaseDetailsWithLineNumbers;
EXEC dbo.GetDuplicatePurchaseDetails;
```

## 📋 Complete Requirements Checklist

### ✅ C# Problems (Completed)
- [x] **Problem 1**: String interleaving with unit tests
- [x] **Problem 2**: Palindrome checker with comprehensive test cases  
- [x] **Problem 3**: Parallel file search with Windows/Docker volume support

### ✅ SQL Problems (Implemented)  
- [x] **Problem 4**: ROW_NUMBER() line numbering via stored procedure
- [x] **Problem 5**: Duplicate detection via stored procedure
- [x] **Problem 6**: Stored procedure integration with EF Core

### ✅ React Problems (Completed)
- [x] **Problem 7**: Purchase detail grid with Alex Lee styling and filtering
- [x] **Problem 8**: Modal CRUD operations with form validation

### ✅ Technical Implementation
- [x] **SQL Server Express**: Docker containerized with health checks
- [x] **Database Initialization**: Automatic SQLExerciseScript.sql loading
- [x] **CQRS Pattern**: Commands/Queries with MediatR
- [x] **Domain Models**: Immutable records pattern
- [x] **Cross-platform Support**: Windows volume mounting in Docker
- [x] **Production Deployment**: Docker Compose orchestration
- [x] **Comprehensive Testing**: 29 unit tests + API integration tests

## 🛠️ Technology Stack

**Backend:**
- .NET 8 WebAPI
- **SQL Server Express 2022** 
- Entity Framework Core 9
- MediatR (CQRS pattern)
- xUnit with 29 test cases

**Frontend:**
- React 18 + TypeScript
- **Alex Lee Corporate Branding**
- React Query for state management
- Professional UI components

**Infrastructure:**
- **Docker + SQL Server Express**
- Cross-platform volume mounting
- Health checks and monitoring
- Production-ready orchestration

## 📊 API Documentation

### Core Endpoints
- **Swagger**: `http://localhost:5000/swagger`
- **Health**: `http://localhost:5000/health` 
- **Purchase Details**: `http://localhost:5000/api/purchasedetails`

### SQL Exercise Endpoints
- **Line Numbers**: `GET /api/purchasedetails/with-line-numbers`
- **Duplicates**: `GET /api/purchasedetails/duplicates` 
- **Summary**: `GET /api/purchasedetails/summary`

### Algorithm Endpoints  
- **String Interleave**: `POST /api/algorithms/string-interleave`
- **Palindrome Check**: `POST /api/algorithms/palindrome-check`
- **File Search**: `POST /api/algorithms/file-search`
- **Available Paths**: `GET /api/algorithms/file-search/available-paths`

## 🎨 Alex Lee Branding Integration

The frontend features authentic Alex Lee corporate styling:
- **Professional Typography**: Corporate font family
- **Brand Colors**: Blue (#0074bc) and gold (#e8b441) color scheme  
- **Triangular Design Elements**: Inspired by Alex Lee visual identity
- **Responsive Layout**: Desktop and mobile optimized
- **Production Polish**: Professional UI/UX with loading states

## 🔧 Development & Management

### SQL Server Management Commands
```bash
./sql-server.sh dev         # Start development environment
./sql-server.sh sql-status  # Check SQL Server health
./sql-server.sh sql-shell   # Interactive SQL Server shell  
./sql-server.sh backup-db   # Backup database
./sql-server.sh logs        # Show application logs
./sql-server.sh clean       # Full cleanup
```

### Development Workflow
```bash
# Start complete environment
./sql-server.sh dev

# Make changes to code...

# Test changes
./sql-server.sh test-api

# View logs  
./sql-server.sh logs

# SQL debugging
./sql-server.sh sql-shell
```

## 📈 Production Deployment

**Production-ready features:**
- SQL Server Express with persistent volumes
- Health checks for all services  
- Multi-stage Docker builds for optimal image sizes
- Windows file system volume mounting
- Comprehensive logging and monitoring

```bash
# Start production environment
./sql-server.sh prod

# Check production status
docker-compose -f docker-compose.prod.yml ps
```

## 🏆 Project Completion Status

This Alex Lee Developer Exercise is **COMPLETE** with enterprise-grade implementation:

✅ **All 8 Problems Solved** - C#, SQL, and React requirements met  
✅ **SQL Server Express Integration** - Production database with stored procedures  
✅ **Cross-platform File Search** - Windows/Docker volume mounting  
✅ **Professional UI** - Alex Lee corporate branding and styling  
✅ **Production Deployment** - Docker orchestration with health monitoring  
✅ **Comprehensive Testing** - 29 unit tests + API integration verification  

## 📚 Documentation

- **[SQL Server Integration Guide](./SQL_SERVER_INTEGRATION.md)** - Complete SQL Server setup and usage
- **[Docker Documentation](./DOCKER.md)** - Container deployment guide  
- **[Progress Tracking](./PROGRESS.md)** - Development history and milestones

---

**Author**: Adam Jacob Meyer  
**Repository**: [github.com/ajacobm/alex-lee](https://github.com/ajacobm/alex-lee)  
**Exercise**: Alex Lee Developer Technical Screening - **SQL Server Express Edition**