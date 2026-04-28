#!/bin/bash

# =============================================
# Bharat Kinetic IFMS - Database Migration Runner (Bash)
# Version: 1.0.0
# Description: Executes all database migration scripts in order
# =============================================

set -e

# Default values
SERVER="localhost,1433"
USERNAME="sa"
PASSWORD="Admin@12345"
UPDATE_ONLY=false
SEED_ONLY=false
REFERENCE_DATA_ONLY=false

# Colors
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
CYAN='\033[0;36m'
GRAY='\033[0;37m'
NC='\033[0m' # No Color

# Parse arguments
while [[ $# -gt 0 ]]; do
    case $1 in
        --server)
            SERVER="$2"
            shift 2
            ;;
        --username)
            USERNAME="$2"
            shift 2
            ;;
        --password)
            PASSWORD="$2"
            shift 2
            ;;
        --update-only)
            UPDATE_ONLY=true
            shift
            ;;
        --seed-only)
            SEED_ONLY=true
            shift
            ;;
        --reference-data)
            REFERENCE_DATA_ONLY=true
            shift
            ;;
        *)
            echo -e "${RED}Unknown option: $1${NC}"
            exit 1
            ;;
    esac
done

echo -e "${CYAN}========================================${NC}"
echo -e "${CYAN}Bharat Kinetic IFMS - Database Migration${NC}"
echo -e "${CYAN}========================================${NC}"
echo ""

# Check if sqlcmd is available
if ! command -v sqlcmd &> /dev/null; then
    echo -e "${RED}ERROR: sqlcmd is not installed or not in PATH${NC}"
    echo -e "${YELLOW}Please install SQL Server Command Line Tools${NC}"
    echo -e "${GRAY}For macOS: brew install mssql-tools${NC}"
    echo -e "${GRAY}For Linux: https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-setup-tools${NC}"
    exit 1
fi

# Function to execute SQL script
execute_sql_script() {
    local script_path=$1
    local description=$2
    
    echo -e "${YELLOW}Executing: $description${NC}"
    echo -e "${GRAY}Script: $script_path${NC}"
    
    if sqlcmd -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -i "$script_path" -b; then
        echo -e "${GREEN}✓ Success: $description${NC}"
        echo ""
    else
        echo -e "${RED}✗ Failed: $description${NC}"
        exit 1
    fi
}

# Get script directory
SCRIPT_DIR="$( cd "$( dirname "${BASH_SOURCE[0]}" )" && pwd )"

# Test connection
echo -e "${YELLOW}Testing database connection...${NC}"
if sqlcmd -S "$SERVER" -U "$USERNAME" -P "$PASSWORD" -Q "SELECT @@VERSION" -b > /dev/null 2>&1; then
    echo -e "${GREEN}✓ Database connection successful${NC}"
    echo ""
else
    echo -e "${RED}✗ Failed to connect to database${NC}"
    echo -e "${GRAY}Server: $SERVER${NC}"
    echo -e "${GRAY}Username: $USERNAME${NC}"
    exit 1
fi

# Execute migration scripts
if [ "$REFERENCE_DATA_ONLY" = true ]; then
    echo -e "${CYAN}Running BULK REFERENCE DATA (sales transactions)...${NC}"
    echo ""

    execute_sql_script "$SCRIPT_DIR/10-bulk-reference-data.sql" "Bulk Reference Data"
elif [ "$UPDATE_ONLY" = true ]; then
    echo -e "${CYAN}Running UPDATE ONLY...${NC}"
    echo ""

    execute_sql_script "$SCRIPT_DIR/08-update-existing.sql" "Update Existing Databases"
elif [ "$SEED_ONLY" = true ]; then
    echo -e "${CYAN}Running SEED DATA ONLY...${NC}"
    echo ""

    execute_sql_script "$SCRIPT_DIR/07-seed-data.sql" "Seed Data"
elif [ "$UPDATE_ONLY" = false ] && [ "$SEED_ONLY" = false ]; then
    echo -e "${CYAN}Running FULL MIGRATION (Create + Seed)...${NC}"
    echo ""

    execute_sql_script "$SCRIPT_DIR/01-create-databases.sql" "Create Databases"
    execute_sql_script "$SCRIPT_DIR/02-identity-schema.sql" "Identity Schema"
    execute_sql_script "$SCRIPT_DIR/03-station-schema.sql" "Station Schema"
    execute_sql_script "$SCRIPT_DIR/04-inventory-schema.sql" "Inventory Schema"
    execute_sql_script "$SCRIPT_DIR/05-sales-schema.sql" "Sales Schema"
    execute_sql_script "$SCRIPT_DIR/06-booking-schema.sql" "Booking Schema"
    execute_sql_script "$SCRIPT_DIR/07-seed-data.sql" "Seed Data"
fi

echo -e "${CYAN}========================================${NC}"
echo -e "${GREEN}Migration completed successfully!${NC}"
echo -e "${CYAN}========================================${NC}"
echo ""
echo -e "${YELLOW}Next steps:${NC}"
echo -e "${GRAY}1. Verify databases using Azure Data Studio or sqlcmd${NC}"
echo -e "${GRAY}2. Update connection strings in appsettings.json${NC}"
echo -e "${GRAY}3. Run Entity Framework migrations if needed${NC}"
echo ""
