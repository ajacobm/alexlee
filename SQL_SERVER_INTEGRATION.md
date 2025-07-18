# 🗄️ SQL Server Express Integration Guide

This document describes the SQL Server Express integration for the Alex Lee Developer Exercise, replacing the SQLite implementation with a production-ready SQL Server setup.

## 🎯 Overview

The Alex Lee exercise has been enhanced with:
- **SQL Server Express** running in Docker containers  
- **Stored Procedures** implementing SQL Exercise questions #4-6
- **Cross-platform file search** with volume mounting for Windows/Docker environments
- **Database initialization** using the provided SQLExerciseScript.sql
- **Production-ready deployment** with Docker Compose orchestration

## 🏗️ Architecture Changes

### Previous: SQLite Architecture
```
┌─────────────────┐    ┌─────────────────┐
│   React Frontend │────│   .NET 8 API    │
│                 │    │                 │
│                 │    │   + EF Core     │
│                 │    │   + SQLite      │
└─────────────────┘    └─────────────────┘
```

### New: SQL Server Express Architecture
```
┌─────────────────┐    ┌─────────────────┐    ┌─────────────────┐
│   React Frontend │────│   .NET 8 API    │────│ SQL Server      │
│                 │    │                 │    │ Express         │
│                 │    │   + EF Core     │    │                 │
│                 │    │   + Stored      │    │ + Stored Procs  │
│                 │    │     Procedures  │    │ + SQLScript.sql │
└─────────────────┘    └─────────────────┘    └─────────────────┘
                                               
                                               ┌─────────────────┐
                                               │ File Search     │
                                               │ Volume Mount    │
                                               │ C:/Users → Linux│
                                               └─────────────────┘
```

## 🚀 Quick Start

### Method 1: Using Helper Script (Recommended)
```bash
# Make script executable
chmod +x sql-server.sh

# Start development environment
./sql-server.sh dev

# Check SQL Server status  
./sql-server.sh sql-status

# Test API endpoints
./sql-server.sh test-api

# Access SQL Server shell
./sql-server.sh sql-shell
```

### Method 2: Direct Docker Compose
```bash
# Development environment
docker-compose up -d

# Production environment
docker-compose -f docker-compose.prod.yml up -d

# Check logs
docker-compose logs -f
```

## 🔧 SQL Server Configuration

### Connection Details
- **Server**: `localhost:1433` (external) / `sqlserver:1433` (container)
- **Database**: `AlexLeeDB`
- **Username**: `SA`
- **Password**: `P@ssw0rd123!`
- **Connection String**: `Server=sqlserver,1433;Database=AlexLeeDB;User Id=SA;Password=P@ssw0rd123!;TrustServerCertificate=true`

### Container Configuration
```yaml
sqlserver:
  image: mcr.microsoft.com/mssql/server:2022-latest
  environment:
    - ACCEPT_EULA=Y
    - SA_PASSWORD=P@ssw0rd123!
    - MSSQL_PID=Express  # Use SQL Server Express edition
  healthcheck:
    test: ["CMD-SHELL", "/opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P 'P@ssw0rd123!' -Q 'SELECT 1'"]
```

## 📊 SQL Exercise Implementation

### Question #4: Line Numbers per Item per Purchase Order
```sql
-- Implemented in stored procedure: GetPurchaseDetailsWithLineNumbers
SELECT 
    PurchaseDetailItemAutoId,
    PurchaseOrderNumber,
    ItemNumber,
    ItemName,
    PurchasePrice,
    PurchaseQuantity,
    ROW_NUMBER() OVER (
        PARTITION BY PurchaseOrderNumber, ItemNumber 
        ORDER BY PurchaseDetailItemAutoId
    ) AS LineNumber
FROM dbo.PurchaseDetailItem
ORDER BY PurchaseOrderNumber, ItemNumber, LineNumber;
```

#### API Endpoint
```
GET /api/purchasedetails/with-line-numbers
```

#### Expected Output
```
PO Number | Item | Line | Name      | Price  | Qty
----------|------|------|-----------|--------|----
112334    | 4011 |  1   | Banana    | 112.19 | 50
112334    | 4011 |  2   | Banana    | 112.19 | 50  
112334    | 4011 |  3   | Banana    | 112.19 | 50
112334    | 4030 |  1   | Kiwis     | 153.88 | 100
```

### Question #5: Duplicate Detection
```sql
-- Implemented in stored procedure: GetDuplicatePurchaseDetails
SELECT 
    PurchaseDetailItemAutoId,
    PurchaseOrderNumber,
    ItemNumber,
    ItemName,
    PurchasePrice,
    PurchaseQuantity,
    COUNT(*) OVER (
        PARTITION BY PurchaseOrderNumber, ItemNumber, PurchasePrice, PurchaseQuantity
    ) AS DuplicateCount
FROM dbo.PurchaseDetailItem
WHERE EXISTS (
    -- Only include records that have duplicates
    SELECT 1 FROM dbo.PurchaseDetailItem AS pdi2
    WHERE pdi2.PurchaseOrderNumber = PurchaseDetailItem.PurchaseOrderNumber
      AND pdi2.ItemNumber = PurchaseDetailItem.ItemNumber
      AND pdi2.PurchasePrice = PurchaseDetailItem.PurchasePrice
      AND pdi2.PurchaseQuantity = PurchaseDetailItem.PurchaseQuantity
    GROUP BY pdi2.PurchaseOrderNumber, pdi2.ItemNumber, pdi2.PurchasePrice, pdi2.PurchaseQuantity
    HAVING COUNT(*) > 1
)
ORDER BY PurchaseOrderNumber, ItemNumber, PurchaseDetailItemAutoId;
```

#### API Endpoint
```
GET /api/purchasedetails/duplicates
```

### Question #6: Stored Procedure Implementation
Both queries above are implemented as stored procedures:

- `dbo.GetPurchaseDetailsWithLineNumbers`
- `dbo.GetDuplicatePurchaseDetails`
- `dbo.CheckDatabaseReady` (utility)

## 📁 File Search Enhancement (Problem #3)

The file search functionality has been enhanced to handle cross-platform scenarios:

### Volume Mounting Strategy
```yaml
# In docker-compose.yml
volumes:
  - C:/Users:/app/search-files/windows:ro  # Windows user files (read-only)
```

### Cross-Platform Path Resolution
The enhanced `FileSearchUtilities` class handles:

1. **Docker Container Detection**
   ```csharp
   private static bool IsRunningInDockerContainer()
   {
       return Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") != null ||
              File.Exists("/.dockerenv");
   }
   ```

2. **Windows Development Paths**
   ```csharp
   var windowsPaths = new[]
   {
       Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
       @"C:\temp",
       Directory.GetCurrentDirectory()
   };
   ```

3. **Docker Mounted Paths**
   ```csharp
   var dockerPaths = new[]
   {
       "/app/search-files/windows",  // Mounted Windows C:/Users
       "/app/search-files",
       "/tmp"
   };
   ```

### API Endpoints
```
POST /api/algorithms/file-search
GET  /api/algorithms/file-search/available-paths
```

### Usage Examples

#### Windows PowerShell Development
```powershell
# Search in user profile
curl -X POST "http://localhost:5000/api/algorithms/file-search" `
     -H "Content-Type: application/json" `
     -d '{"searchTerm":"TODO","directoryPath":"C:/Users/YourUser/Documents"}'
```

#### Docker Container Environment
```bash
# Search in mounted Windows directory
curl -X POST "http://localhost:5000/api/algorithms/file-search" \
     -H "Content-Type: application/json" \
     -d '{"searchTerm":"TODO","directoryPath":"/app/search-files/windows"}'

# Get available paths
curl "http://localhost:5000/api/algorithms/file-search/available-paths"
```

## 🗃️ Database Initialization

### Automatic Initialization
The `AlexLeeDbContext` automatically:

1. **Detects missing database/table**
2. **Runs init-database.sql** (creates table and data)
3. **Executes stored-procedures.sql** (creates stored procedures)
4. **Handles multiple startup scenarios**

### Manual Initialization
```bash
# Using helper script
./sql-server.sh sql-init

# Direct SQL Server access
./sql-server.sh sql-shell

# Check database status
./sql-server.sh sql-status
```

### SQL Script Integration
The system loads SQL scripts from multiple paths:
```csharp
var scriptPaths = new[]
{
    Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "scripts", "init-database.sql"),
    Path.Combine(Directory.GetCurrentDirectory(), "scripts", "init-database.sql"),
    "/app/scripts/init-database.sql"  // Docker path
};
```

## 🔍 Testing & Verification

### API Testing
```bash
# Test all endpoints
./sql-server.sh test-api

# Test file search specifically  
./sql-server.sh file-search

# Manual API tests
curl "http://localhost:5000/api/purchasedetails/with-line-numbers"
curl "http://localhost:5000/api/purchasedetails/duplicates"
curl "http://localhost:5000/api/purchasedetails/summary"
```

### Database Verification
```sql
-- Connect via helper script
./sql-server.sh sql-shell

-- Check table and data
USE AlexLeeDB;
SELECT COUNT(*) FROM PurchaseDetailItem;

-- Test line numbers manually
EXEC dbo.GetPurchaseDetailsWithLineNumbers;

-- Test duplicates manually  
EXEC dbo.GetDuplicatePurchaseDetails;
```

## 💾 Backup & Recovery

### Database Backup
```bash
# Automated backup
./sql-server.sh backup-db

# Manual backup (from SQL shell)
BACKUP DATABASE AlexLeeDB TO DISK = '/var/opt/mssql/data/alexlee_manual.bak';
```

### Database Restore
```bash
# Interactive restore
./sql-server.sh restore-db

# Manual restore (from SQL shell)
RESTORE DATABASE AlexLeeDB FROM DISK = '/var/opt/mssql/data/backup.bak' WITH REPLACE;
```

## 🏭 Production Deployment

### Production Configuration
```bash
# Start production environment
./sql-server.sh prod

# Or directly
docker-compose -f docker-compose.prod.yml up -d
```

### Production Features
- **Health checks** for all services
- **Optimized container images** with multi-stage builds
- **Persistent volumes** for SQL Server data
- **Security hardening** (non-root containers when possible)
- **Resource constraints** and restart policies

### Environment Variables
```bash
# Production settings
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=AlexLeeDB;User Id=SA;Password=P@ssw0rd123!;TrustServerCertificate=true
FileSearchPath=/app/search-files
```

## 📋 Available Commands

| Command | Purpose |
|---------|---------|
| `./sql-server.sh dev` | Start development environment |
| `./sql-server.sh prod` | Start production environment |
| `./sql-server.sh sql-status` | Check SQL Server health |
| `./sql-server.sh sql-shell` | Interactive SQL Server shell |
| `./sql-server.sh test-api` | Test all API endpoints |
| `./sql-server.sh file-search` | Test file search functionality |
| `./sql-server.sh backup-db` | Backup the database |
| `./sql-server.sh logs` | Show application logs |
| `./sql-server.sh clean` | Full cleanup (destructive) |

## 🔧 Troubleshooting

### SQL Server Won't Start
```bash
# Check SQL Server logs
./sql-server.sh sql-logs

# Verify Docker resources
docker system df
docker system prune  # If low on space

# Check port conflicts
netstat -an | grep 1433
```

### Database Connection Issues
```bash
# Test connection manually
./sql-server.sh sql-status

# Check connection string in API
curl "http://localhost:5000/health"

# Restart with fresh database
./sql-server.sh down
./sql-server.sh dev
```

### File Search Not Working
```bash
# Check available paths
curl "http://localhost:5000/api/algorithms/file-search/available-paths"

# Verify volume mounts
docker inspect alexlee-backend-dev | grep -A5 -B5 "Mounts"

# Test with known directory
curl -X POST "http://localhost:5000/api/algorithms/file-search" \
     -H "Content-Type: application/json" \
     -d '{"searchTerm":"test","directoryPath":"/tmp"}'
```

## 🏆 Success Verification

The SQL Server Express integration is successful when:

✅ **SQL Server Express** runs in Docker container  
✅ **Database auto-initializes** with SQLExerciseScript.sql data  
✅ **Stored procedures** are created and working  
✅ **API endpoints** respond with line numbers and duplicates  
✅ **File search** works with mounted Windows volumes  
✅ **Cross-platform paths** resolve correctly  
✅ **Production deployment** starts without errors  

This completes the Alex Lee Developer Exercise with enterprise-grade SQL Server Express implementation! 🎯