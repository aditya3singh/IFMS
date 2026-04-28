# Bharat Kinetic IFMS - Database Migration Guide

This directory contains all database migration scripts for the Industrial Fuel Management System (IFMS).

## Overview

The IFMS uses a microservices architecture with 5 separate databases:

- **IFMS_IdentityDB** - User authentication and authorization
- **IFMS_StationDB** - Fuel station information and dealer assignments
- **IFMS_InventoryDB** - Fuel stock and inventory management
- **IFMS_SalesDB** - Transaction records and sales data
- **IFMS_BookingDB** - Fuel booking and token management

## Prerequisites

### Required Tools

1. **SQL Server 2022** (or compatible version)
2. **sqlcmd** - SQL Server Command Line Tools
   - Windows: Included with SQL Server installation
   - macOS: `brew install mssql-tools`
   - Linux: [Installation Guide](https://docs.microsoft.com/en-us/sql/linux/sql-server-linux-setup-tools)

### Connection Requirements

- Server: `localhost,1433` (default)
- Username: `sa`
- Password: `Admin@12345` (default for development)

## Migration Scripts

### Core Schema Scripts (Execute in Order)

1. **01-create-databases.sql** - Creates all 5 databases
2. **02-identity-schema.sql** - Identity database schema (Users, OTPs, RefreshTokens, etc.)
3. **03-station-schema.sql** - Station database schema (Stations, DealerAssignments, Pricing)
4. **04-inventory-schema.sql** - Inventory database schema (FuelStocks, StockMovements, FuelTypes)
5. **05-sales-schema.sql** - Sales database schema (Transactions, DailySalesSummary, PaymentMethods)
6. **06-booking-schema.sql** - Booking database schema (Bookings, KycVerifications)
7. **07-seed-data.sql** - Initial seed data for all databases
8. **08-update-existing.sql** - Updates existing databases with schema changes
9. **09-rollback.sql** - Drops all databases (USE WITH CAUTION!)

## Quick Start

### Option 1: Using Migration Runner Scripts (Recommended)

#### For Linux/macOS:

```bash
# Make script executable
chmod +x run-migrations.sh

# Run full migration (create + seed)
./run-migrations.sh

# Run with custom connection
./run-migrations.sh --server "localhost,1433" --username "sa" --password "YourPassword"

# Update existing databases only
./run-migrations.sh --update-only

# Seed data only
./run-migrations.sh --seed-only
```

#### For Windows (PowerShell):

```powershell
# Run full migration (create + seed)
.\run-migrations.ps1

# Run with custom connection
.\run-migrations.ps1 -Server "localhost,1433" -Username "sa" -Password "YourPassword"

# Update existing databases only
.\run-migrations.ps1 -UpdateOnly

# Seed data only
.\run-migrations.ps1 -SeedOnly
```

### Option 2: Using Docker Compose

If you're using the provided `docker-compose.yml`:

```bash
# Start SQL Server container
cd IFMS
docker-compose up -d sqlserver

# Wait for SQL Server to be ready (about 30 seconds)
sleep 30

# Run migrations
cd database
./run-migrations.sh
```

### Option 3: Manual Execution

Execute each script manually using sqlcmd:

```bash
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -i 01-create-databases.sql
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -i 02-identity-schema.sql
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -i 03-station-schema.sql
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -i 04-inventory-schema.sql
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -i 05-sales-schema.sql
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -i 06-booking-schema.sql
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -i 07-seed-data.sql
```

## Migration Scenarios

### Scenario 1: Fresh Installation

For a brand new installation:

```bash
./run-migrations.sh
```

This will:
1. Create all 5 databases
2. Create all tables, indexes, and constraints
3. Insert seed data

### Scenario 2: Update Existing Databases

If you already have databases and want to apply schema updates:

```bash
./run-migrations.sh --update-only
```

This will:
1. Add missing columns to existing tables
2. Create missing indexes
3. Update constraints
4. Preserve existing data

### Scenario 3: Reset Seed Data

To re-insert seed data without recreating schemas:

```bash
./run-migrations.sh --seed-only
```

### Scenario 4: Complete Reset

To completely drop and recreate all databases:

```bash
# Drop all databases
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -i 09-rollback.sql

# Run full migration
./run-migrations.sh
```

## Database Schema Details

### IFMS_IdentityDB

- **Users** - User accounts (Customer, Dealer, Admin)
- **Otps** - One-time passwords for authentication
- **RefreshTokens** - JWT refresh tokens
- **UserSessions** - Active user sessions
- **AuditLogs** - Audit trail for security

### IFMS_StationDB

- **Stations** - Fuel station master data
- **DealerAssignments** - Station-to-dealer mappings
- **StationPricing** - Dynamic fuel pricing per station

### IFMS_InventoryDB

- **FuelStocks** - Current fuel inventory levels
- **StockMovements** - Inventory transaction history
- **FuelTypes** - Fuel type reference data

### IFMS_SalesDB

- **Transactions** - Sales transaction records
- **DailySalesSummary** - Aggregated daily sales
- **PaymentMethods** - Payment method reference data

### IFMS_BookingDB

- **Bookings** - Fuel booking records
- **KycVerifications** - KYC verification status
- **vw_BookingHistory** - Booking history view

## Seed Data

The seed data includes:

- 1 Admin user (admin@bharatkinetic.com)
- 5 Fuel stations in Bangalore
- Station pricing for Petrol, Diesel, CNG
- Initial fuel stock for all stations
- 4 Fuel types (Petrol, Diesel, CNG, Electric)
- 5 Payment methods (Cash, Card, UPI, Wallet, Token)

## Verification

After running migrations, verify the setup:

```bash
# Check databases exist
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "SELECT name FROM sys.databases WHERE name LIKE 'IFMS_%'"

# Check table counts
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "USE IFMS_IdentityDB; SELECT COUNT(*) AS TableCount FROM sys.tables"

# Check seed data
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "USE IFMS_StationDB; SELECT COUNT(*) AS StationCount FROM Stations"
```

## Troubleshooting

### Connection Failed

```
Error: Cannot connect to server
```

**Solution:**
- Verify SQL Server is running
- Check server address and port
- Verify credentials
- Check firewall settings

### Database Already Exists

```
Error: Database 'IFMS_IdentityDB' already exists
```

**Solution:**
- Use `--update-only` flag to update existing databases
- Or run rollback script first: `sqlcmd -i 09-rollback.sql`

### Permission Denied

```
Error: CREATE DATABASE permission denied
```

**Solution:**
- Ensure user has `sysadmin` or `dbcreator` role
- Use `sa` account for initial setup

### Script Execution Failed

```
Error: Incorrect syntax near...
```

**Solution:**
- Ensure you're using SQL Server 2022 or compatible version
- Check script file encoding (should be UTF-8)
- Verify no manual edits broke SQL syntax

## Entity Framework Integration

After running SQL migrations, sync with Entity Framework:

```bash
# Navigate to each infrastructure project
cd IFMS.Identity.Infrastructure

# Add migration (if needed)
dotnet ef migrations add SyncWithSqlMigration

# Update database
dotnet ef database update
```

## Backup and Restore

### Create Backup

```bash
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "BACKUP DATABASE IFMS_IdentityDB TO DISK = '/var/opt/mssql/backup/IFMS_IdentityDB.bak'"
```

### Restore Backup

```bash
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "RESTORE DATABASE IFMS_IdentityDB FROM DISK = '/var/opt/mssql/backup/IFMS_IdentityDB.bak' WITH REPLACE"
```

## Production Deployment

For production environments:

1. **Review and customize seed data** - Update admin credentials, station data
2. **Use secure passwords** - Change default SA password
3. **Enable SSL/TLS** - Use encrypted connections
4. **Set up backups** - Configure automated backup strategy
5. **Apply security policies** - Implement least privilege access
6. **Monitor performance** - Set up database monitoring
7. **Test rollback procedures** - Verify backup/restore works

## Support

For issues or questions:
- Check troubleshooting section above
- Review script comments for detailed information
- Contact: dev@bharatkinetic.com

## Version History

- **1.0.0** (2026-04-07) - Initial migration scripts
  - Created all 5 database schemas
  - Added seed data
  - Created migration runner scripts
  - Added rollback capability

---

**⚠️ WARNING:** Always backup your databases before running migrations in production!
