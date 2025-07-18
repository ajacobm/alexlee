#!/bin/bash

# ===============================================
# Alex Lee SQL Server Express Docker Management Script
# This script provides comprehensive SQL Server management for the Alex Lee exercise
# ===============================================

set -e

SCRIPT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT_NAME="alexlee"
SQL_PASSWORD="P@ssw0rd123!"
SQL_SERVER_CONTAINER="alexlee-sqlserver"
BACKEND_CONTAINER="alexlee-backend-dev"

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
BLUE='\033[0;34m'
NC='\033[0m' # No Color

print_header() {
    echo -e "${BLUE}===============================================${NC}"
    echo -e "${BLUE} Alex Lee SQL Server Express Management${NC}"
    echo -e "${BLUE}===============================================${NC}"
}

print_success() {
    echo -e "${GREEN}✅ $1${NC}"
}

print_warning() {
    echo -e "${YELLOW}⚠️  $1${NC}"
}

print_error() {
    echo -e "${RED}❌ $1${NC}"
}

print_info() {
    echo -e "${BLUE}ℹ️  $1${NC}"
}

show_help() {
    print_header
    echo "Usage: $0 [COMMAND]"
    echo ""
    echo "Commands:"
    echo "  dev             Start development environment (with SQL Server Express)"
    echo "  prod            Start production environment"
    echo "  stop            Stop all containers"
    echo "  down            Stop and remove all containers and networks"
    echo "  clean           Full cleanup (containers, volumes, images)"
    echo "  logs            Show application logs"
    echo "  sql-logs        Show SQL Server logs"
    echo "  sql-shell       Connect to SQL Server shell"
    echo "  sql-init        Initialize database manually" 
    echo "  sql-status      Check SQL Server status"
    echo "  test-api        Test the API endpoints"
    echo "  file-search     Test file search functionality"
    echo "  backup-db       Backup the SQL Server database"
    echo "  restore-db      Restore the SQL Server database"
    echo "  help            Show this help message"
    echo ""
    echo "Examples:"
    echo "  $0 dev          Start development environment"
    echo "  $0 sql-shell    Connect to SQL Server for manual queries"
    echo "  $0 test-api     Test all API endpoints"
}

wait_for_sql_server() {
    print_info "Waiting for SQL Server to be ready..."
    local max_attempts=30
    local attempt=1
    
    while [ $attempt -le $max_attempts ]; do
        if docker exec -t $SQL_SERVER_CONTAINER /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SQL_PASSWORD" -Q "SELECT 1" >/dev/null 2>&1; then
            print_success "SQL Server is ready!"
            return 0
        fi
        
        echo -n "."
        sleep 2
        ((attempt++))
    done
    
    print_error "SQL Server failed to start after $max_attempts attempts"
    return 1
}

start_dev() {
    print_header
    print_info "Starting Alex Lee development environment with SQL Server Express..."
    
    # Start the services
    docker-compose -f docker-compose.yml up -d
    
    # Wait for SQL Server
    if wait_for_sql_server; then
        print_success "Development environment started successfully!"
        print_info "Services available:"
        echo "  🌐 Frontend: http://localhost:3000"
        echo "  🔗 API: http://localhost:5000"
        echo "  📊 Swagger: http://localhost:5000/swagger"
        echo "  🗄️  SQL Server: localhost:1433"
        echo ""
        print_info "Default SQL Server credentials:"
        echo "  Username: SA"
        echo "  Password: $SQL_PASSWORD"
        echo "  Database: AlexLeeDB"
        
        # Test the API
        sleep 5
        test_basic_api
    else
        print_error "Failed to start development environment"
        return 1
    fi
}

start_prod() {
    print_header
    print_info "Starting Alex Lee production environment..."
    
    docker-compose -f docker-compose.prod.yml up -d
    
    if wait_for_sql_server; then
        print_success "Production environment started successfully!"
        print_info "Services available:"
        echo "  🌐 Application: http://localhost:80"
        print_info "Use 'docker ps' to check container status"
    else
        print_error "Failed to start production environment"
        return 1
    fi
}

stop_containers() {
    print_info "Stopping all containers..."
    docker-compose -f docker-compose.yml stop 2>/dev/null || true
    docker-compose -f docker-compose.prod.yml stop 2>/dev/null || true
    print_success "All containers stopped"
}

down_containers() {
    print_info "Stopping and removing containers and networks..."
    docker-compose -f docker-compose.yml down 2>/dev/null || true
    docker-compose -f docker-compose.prod.yml down 2>/dev/null || true
    print_success "Containers and networks removed"
}

clean_all() {
    print_warning "This will remove all containers, volumes, and images for this project!"
    read -p "Are you sure? (y/N): " -n 1 -r
    echo
    
    if [[ $REPLY =~ ^[Yy]$ ]]; then
        print_info "Performing full cleanup..."
        
        # Stop and remove containers
        down_containers
        
        # Remove volumes
        docker volume rm alexlee_backend-data alexlee_sqlserver-data 2>/dev/null || true
        
        # Remove images
        docker rmi alexlee-backend alexlee-frontend 2>/dev/null || true
        
        print_success "Full cleanup completed!"
    else
        print_info "Cleanup cancelled"
    fi
}

show_logs() {
    print_info "Showing application logs..."
    docker-compose -f docker-compose.yml logs -f backend frontend 2>/dev/null || \
    docker-compose -f docker-compose.prod.yml logs -f backend frontend
}

show_sql_logs() {
    print_info "Showing SQL Server logs..."
    docker logs -f $SQL_SERVER_CONTAINER
}

sql_shell() {
    print_info "Connecting to SQL Server shell..."
    print_info "Use 'exit' or Ctrl+C to quit"
    docker exec -it $SQL_SERVER_CONTAINER /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SQL_PASSWORD"
}

sql_init() {
    print_info "Manually initializing database..."
    
    # Check if SQL Server is running
    if ! docker ps | grep -q $SQL_SERVER_CONTAINER; then
        print_error "SQL Server container is not running. Start it first with '$0 dev'"
        return 1
    fi
    
    # Run initialization
    if docker exec $SQL_SERVER_CONTAINER /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SQL_PASSWORD" -Q "SELECT 1" >/dev/null 2>&1; then
        print_info "Running database initialization scripts..."
        
        # The application will handle initialization automatically, but we can trigger it via API
        curl -s "http://localhost:5000/health" >/dev/null && print_success "Database initialization triggered via API"
    else
        print_error "Cannot connect to SQL Server"
        return 1
    fi
}

sql_status() {
    print_info "Checking SQL Server status..."
    
    if docker ps | grep -q $SQL_SERVER_CONTAINER; then
        print_success "SQL Server container is running"
        
        if docker exec $SQL_SERVER_CONTAINER /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SQL_PASSWORD" -Q "SELECT @@VERSION" 2>/dev/null; then
            print_success "SQL Server is accepting connections"
            
            # Check database
            if docker exec $SQL_SERVER_CONTAINER /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SQL_PASSWORD" -d AlexLeeDB -Q "SELECT COUNT(*) as RecordCount FROM PurchaseDetailItem" 2>/dev/null; then
                print_success "AlexLeeDB database is ready"
            else
                print_warning "AlexLeeDB database may not be initialized yet"
            fi
        else
            print_error "SQL Server is not accepting connections"
        fi
    else
        print_error "SQL Server container is not running"
    fi
}

test_basic_api() {
    print_info "Testing basic API functionality..."
    
    # Test health endpoint
    if curl -s -f "http://localhost:5000/health" >/dev/null 2>&1; then
        print_success "Health endpoint responding"
    else
        print_warning "Health endpoint not ready yet"
    fi
    
    # Test purchase details endpoint
    if curl -s -f "http://localhost:5000/api/purchasedetails" >/dev/null 2>&1; then
        print_success "Purchase details API responding"
    else
        print_warning "Purchase details API not ready yet"
    fi
}

test_api() {
    print_info "Testing Alex Lee API endpoints..."
    
    local base_url="http://localhost:5000"
    
    # Test health
    echo "Testing health endpoint..."
    curl -s "$base_url/health" || print_warning "Health check failed"
    
    # Test algorithm endpoints
    echo -e "\nTesting string interleave..."
    curl -s -X POST "$base_url/api/algorithms/string-interleave" \
         -H "Content-Type: application/json" \
         -d '{"first":"abc","second":"123"}' | jq . 2>/dev/null || echo "Raw response"
    
    # Test purchase details
    echo -e "\nTesting purchase details..."
    curl -s "$base_url/api/purchasedetails" | jq . 2>/dev/null || echo "Raw response"
    
    # Test stored procedures
    echo -e "\nTesting stored procedure - line numbers..."
    curl -s "$base_url/api/purchasedetails/with-line-numbers" | jq . 2>/dev/null || echo "Raw response"
    
    echo -e "\nTesting stored procedure - duplicates..."
    curl -s "$base_url/api/purchasedetails/duplicates" | jq . 2>/dev/null || echo "Raw response"
    
    print_success "API tests completed!"
}

test_file_search() {
    print_info "Testing file search functionality..."
    
    # Create test files in mounted directory (if possible)
    local test_dir="/tmp/alexlee-test"
    mkdir -p "$test_dir"
    echo "This is a test file with TODO items" > "$test_dir/test1.txt"
    echo "Another file with TODO and FIXME" > "$test_dir/test2.txt"
    echo "No keywords here" > "$test_dir/test3.txt"
    
    # Test file search API
    echo "Testing file search API..."
    curl -s -X POST "http://localhost:5000/api/algorithms/file-search" \
         -H "Content-Type: application/json" \
         -d "{\"searchTerm\":\"TODO\",\"directoryPath\":\"$test_dir\"}" | jq . 2>/dev/null || echo "Raw response"
    
    # Get available paths
    echo -e "\nGetting available file search paths..."
    curl -s "http://localhost:5000/api/algorithms/file-search/available-paths" | jq . 2>/dev/null || echo "Raw response"
    
    # Cleanup
    rm -rf "$test_dir"
    print_success "File search tests completed!"
}

backup_db() {
    print_info "Backing up SQL Server database..."
    local backup_file="alexlee_backup_$(date +%Y%m%d_%H%M%S).bak"
    
    if docker exec $SQL_SERVER_CONTAINER /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SQL_PASSWORD" \
       -Q "BACKUP DATABASE AlexLeeDB TO DISK = '/var/opt/mssql/data/$backup_file'"; then
        print_success "Database backed up to: $backup_file"
        print_info "Backup location in container: /var/opt/mssql/data/$backup_file"
    else
        print_error "Database backup failed"
    fi
}

restore_db() {
    print_warning "Database restore requires a backup file in the SQL Server container"
    print_info "Available backup files:"
    docker exec $SQL_SERVER_CONTAINER ls -la /var/opt/mssql/data/*.bak 2>/dev/null || print_info "No backup files found"
    
    read -p "Enter backup filename (without path): " backup_file
    
    if [ -n "$backup_file" ]; then
        print_info "Restoring database from: $backup_file"
        docker exec $SQL_SERVER_CONTAINER /opt/mssql-tools/bin/sqlcmd -S localhost -U SA -P "$SQL_PASSWORD" \
            -Q "RESTORE DATABASE AlexLeeDB FROM DISK = '/var/opt/mssql/data/$backup_file' WITH REPLACE"
    fi
}

# Main script logic
case "${1:-help}" in
    "dev")
        start_dev
        ;;
    "prod")
        start_prod
        ;;
    "stop")
        stop_containers
        ;;
    "down")
        down_containers
        ;;
    "clean")
        clean_all
        ;;
    "logs")
        show_logs
        ;;
    "sql-logs")
        show_sql_logs
        ;;
    "sql-shell")
        sql_shell
        ;;
    "sql-init")
        sql_init
        ;;
    "sql-status")
        sql_status
        ;;
    "test-api")
        test_api
        ;;
    "file-search")
        test_file_search
        ;;
    "backup-db")
        backup_db
        ;;
    "restore-db")
        restore_db
        ;;
    "help"|*)
        show_help
        ;;
esac