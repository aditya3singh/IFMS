# Database Migration Testing Guide

## Pre-Migration Testing

### 1. Verify Prerequisites

```bash
# Check SQL Server is running
docker ps | grep sqlserver
# OR
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "SELECT @@VERSION"

# Check sqlcmd is installed
sqlcmd -?

# Check script files exist
ls -la IFMS/database/*.sql
```

### 2. Backup Existing Databases (if any)

```bash
# Create backup directory
mkdir -p ~/ifms-backups

# Backup each database
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "BACKUP DATABASE IFMS_IdentityDB TO DISK = '/var/opt/mssql/backup/IFMS_IdentityDB_$(date +%Y%m%d).bak'"
```

## Migration Testing Scenarios

### Scenario 1: Fresh Installation Test

**Objective:** Test complete database creation from scratch

```bash
# 1. Ensure no existing databases
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -i 09-rollback.sql

# 2. Run full migration
./run-migrations.sh

# 3. Verify databases created
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "
SELECT name, create_date, state_desc 
FROM sys.databases 
WHERE name LIKE 'IFMS_%'
ORDER BY name"

# Expected output: 5 databases (IdentityDB, StationDB, InventoryDB, SalesDB, BookingDB)
```

**Verification Queries:**

```sql
-- Check Identity DB
USE IFMS_IdentityDB;
SELECT 'Users' AS TableName, COUNT(*) AS RowCount FROM Users
UNION ALL
SELECT 'Otps', COUNT(*) FROM Otps
UNION ALL
SELECT 'RefreshTokens', COUNT(*) FROM RefreshTokens
UNION ALL
SELECT 'UserSessions', COUNT(*) FROM UserSessions
UNION ALL
SELECT 'AuditLogs', COUNT(*) FROM AuditLogs;

-- Check Station DB
USE IFMS_StationDB;
SELECT 'Stations' AS TableName, COUNT(*) AS RowCount FROM Stations
UNION ALL
SELECT 'DealerAssignments', COUNT(*) FROM DealerAssignments
UNION ALL
SELECT 'StationPricing', COUNT(*) FROM StationPricing;

-- Check Inventory DB
USE IFMS_InventoryDB;
SELECT 'FuelStocks' AS TableName, COUNT(*) AS RowCount FROM FuelStocks
UNION ALL
SELECT 'StockMovements', COUNT(*) FROM StockMovements
UNION ALL
SELECT 'FuelTypes', COUNT(*) FROM FuelTypes;

-- Check Sales DB
USE IFMS_SalesDB;
SELECT 'Transactions' AS TableName, COUNT(*) AS RowCount FROM Transactions
UNION ALL
SELECT 'DailySalesSummary', COUNT(*) FROM DailySalesSummary
UNION ALL
SELECT 'PaymentMethods', COUNT(*) FROM PaymentMethods;

-- Check Booking DB
USE IFMS_BookingDB;
SELECT 'Bookings' AS TableName, COUNT(*) AS RowCount FROM Bookings
UNION ALL
SELECT 'KycVerifications', COUNT(*) FROM KycVerifications;
```

**Expected Results:**
- Users: 1 (admin user)
- Stations: 5 (Bangalore stations)
- StationPricing: 13 (pricing records)
- FuelStocks: 15 (5 stations × 3 fuel types)
- FuelTypes: 4 (Petrol, Diesel, CNG, Electric)
- PaymentMethods: 5 (Cash, Card, UPI, Wallet, Token)

### Scenario 2: Update Existing Database Test

**Objective:** Test schema updates on existing databases

```bash
# 1. Create initial schema (simulate old version)
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -i 01-create-databases.sql
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "
USE IFMS_IdentityDB;
CREATE TABLE Users (
    Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
    FullName NVARCHAR(100) NOT NULL,
    Email NVARCHAR(150) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(MAX) NOT NULL,
    Role NVARCHAR(20) NOT NULL,
    IsActive BIT NOT NULL,
    CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
);
INSERT INTO Users (FullName, Email, PasswordHash, Role, IsActive)
VALUES ('Test User', 'test@example.com', 'hash123', 'Customer', 1);
"

# 2. Run update script
./run-migrations.sh --update-only

# 3. Verify columns added
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "
USE IFMS_IdentityDB;
SELECT COLUMN_NAME, DATA_TYPE, IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS
WHERE TABLE_NAME = 'Users'
ORDER BY ORDINAL_POSITION"

# 4. Verify data preserved
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "
USE IFMS_IdentityDB;
SELECT Id, Email, FirstName, LastName FROM Users WHERE Email = 'test@example.com'"
```

**Expected Results:**
- All new columns added (FirstName, LastName, PhoneNumber, etc.)
- Existing data preserved
- FullName migrated to FirstName/LastName
- No data loss

### Scenario 3: Idempotency Test

**Objective:** Verify scripts can run multiple times safely

```bash
# 1. Run migration
./run-migrations.sh

# 2. Run again (should not fail)
./run-migrations.sh

# 3. Verify no duplicates
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "
USE IFMS_StationDB;
SELECT LicenseNumber, COUNT(*) AS Count
FROM Stations
GROUP BY LicenseNumber
HAVING COUNT(*) > 1"

# Expected: No results (no duplicates)
```

### Scenario 4: Rollback Test

**Objective:** Test database cleanup

```bash
# 1. Create databases
./run-migrations.sh

# 2. Verify databases exist
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "
SELECT name FROM sys.databases WHERE name LIKE 'IFMS_%'"

# 3. Run rollback
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -i 09-rollback.sql

# 4. Verify databases dropped
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "
SELECT name FROM sys.databases WHERE name LIKE 'IFMS_%'"

# Expected: No results
```

## Data Integrity Testing

### Test 1: Foreign Key Constraints

```sql
-- Test cascade delete
USE IFMS_IdentityDB;

-- Create test user
DECLARE @UserId UNIQUEIDENTIFIER = NEWID();
INSERT INTO Users (Id, Email, PasswordHash, FirstName, LastName, Role, IsActive, CreatedAt, UpdatedAt)
VALUES (@UserId, 'test@test.com', 'hash', 'Test', 'User', 'Customer', 1, GETUTCDATE(), GETUTCDATE());

-- Create related OTP
INSERT INTO Otps (UserId, OtpCode, Purpose, ExpiresAt, CreatedAt)
VALUES (@UserId, '123456', 'Login', DATEADD(MINUTE, 10, GETUTCDATE()), GETUTCDATE());

-- Verify OTP exists
SELECT COUNT(*) AS OtpCount FROM Otps WHERE UserId = @UserId;

-- Delete user (should cascade to OTP)
DELETE FROM Users WHERE Id = @UserId;

-- Verify OTP deleted
SELECT COUNT(*) AS OtpCount FROM Otps WHERE UserId = @UserId;
-- Expected: 0
```

### Test 2: Check Constraints

```sql
-- Test invalid role
USE IFMS_IdentityDB;
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, IsActive, CreatedAt, UpdatedAt)
VALUES ('invalid@test.com', 'hash', 'Test', 'User', 'InvalidRole', 1, GETUTCDATE(), GETUTCDATE());
-- Expected: Error (constraint violation)

-- Test invalid fuel type
USE IFMS_InventoryDB;
INSERT INTO FuelStocks (StationId, FuelType, Quantity, PricePerLitre, LastUpdated)
VALUES (NEWID(), 'InvalidFuel', 100, 50, GETUTCDATE());
-- Expected: Error (constraint violation)

-- Test invalid coordinates
USE IFMS_StationDB;
INSERT INTO Stations (Name, LicenseNumber, City, State, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt)
VALUES ('Test Station', 'TEST-001', 'Test City', 'Test State', 100, 200, 1, GETUTCDATE(), GETUTCDATE());
-- Expected: Error (latitude/longitude out of range)
```

### Test 3: Unique Constraints

```sql
-- Test duplicate email
USE IFMS_IdentityDB;
INSERT INTO Users (Email, PasswordHash, FirstName, LastName, Role, IsActive, CreatedAt, UpdatedAt)
VALUES ('admin@bharatkinetic.com', 'hash', 'Test', 'User', 'Customer', 1, GETUTCDATE(), GETUTCDATE());
-- Expected: Error (duplicate email)

-- Test duplicate license number
USE IFMS_StationDB;
INSERT INTO Stations (Name, LicenseNumber, City, State, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt)
VALUES ('Duplicate Station', 'BK-BLR-001', 'Bangalore', 'Karnataka', 12.9716, 77.5946, 1, GETUTCDATE(), GETUTCDATE());
-- Expected: Error (duplicate license number)
```

## Performance Testing

### Test 1: Index Effectiveness

```sql
-- Test email lookup (should use index)
USE IFMS_IdentityDB;
SET STATISTICS IO ON;
SET STATISTICS TIME ON;

SELECT * FROM Users WHERE Email = 'admin@bharatkinetic.com';

SET STATISTICS IO OFF;
SET STATISTICS TIME OFF;
-- Check execution plan - should show Index Seek on IX_Users_Email
```

### Test 2: Query Performance

```sql
-- Test station search by city (should use index)
USE IFMS_StationDB;
SET STATISTICS IO ON;

SELECT * FROM Stations WHERE City = 'Bangalore' AND IsActive = 1;

SET STATISTICS IO OFF;
-- Should show efficient index usage
```

## Integration Testing with APIs

### Test 1: Identity API Connection

```bash
# Update connection string in appsettings.json
cd IFMS/IFMS.Identity.API

# Run API
dotnet run

# Test endpoint
curl -X POST http://localhost:5001/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "email": "newuser@test.com",
    "password": "Test@123",
    "firstName": "New",
    "lastName": "User",
    "phoneNumber": "+919876543210"
  }'
```

### Test 2: Station API Connection

```bash
cd IFMS/IFMS.Station.API
dotnet run

# Test endpoint
curl http://localhost:5006/api/stations
```

### Test 3: Booking API Connection

```bash
cd IFMS/IFMS.Booking.API
dotnet run

# Test endpoint (requires auth token)
curl http://localhost:5007/api/bookings \
  -H "Authorization: Bearer YOUR_JWT_TOKEN"
```

## Automated Test Script

Create a test script `test-migrations.sh`:

```bash
#!/bin/bash

echo "Starting migration tests..."

# Test 1: Fresh installation
echo "Test 1: Fresh installation"
./run-migrations.sh
if [ $? -eq 0 ]; then
    echo "✓ Fresh installation passed"
else
    echo "✗ Fresh installation failed"
    exit 1
fi

# Test 2: Verify database count
DB_COUNT=$(sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "SELECT COUNT(*) FROM sys.databases WHERE name LIKE 'IFMS_%'" -h -1 | tr -d ' ')
if [ "$DB_COUNT" -eq "5" ]; then
    echo "✓ Database count correct (5)"
else
    echo "✗ Database count incorrect (expected 5, got $DB_COUNT)"
    exit 1
fi

# Test 3: Verify seed data
STATION_COUNT=$(sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "USE IFMS_StationDB; SELECT COUNT(*) FROM Stations" -h -1 | tr -d ' ')
if [ "$STATION_COUNT" -eq "5" ]; then
    echo "✓ Seed data correct (5 stations)"
else
    echo "✗ Seed data incorrect (expected 5 stations, got $STATION_COUNT)"
    exit 1
fi

# Test 4: Idempotency
echo "Test 4: Idempotency"
./run-migrations.sh
if [ $? -eq 0 ]; then
    echo "✓ Idempotency test passed"
else
    echo "✗ Idempotency test failed"
    exit 1
fi

echo "All tests passed!"
```

## Troubleshooting Tests

### Test Connection Issues

```bash
# Test basic connectivity
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "SELECT 1"

# Test with verbose output
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "SELECT @@VERSION" -o output.txt
cat output.txt
```

### Test Permission Issues

```sql
-- Check user permissions
SELECT 
    dp.name AS UserName,
    dp.type_desc AS UserType,
    o.permission_name,
    o.state_desc
FROM sys.database_permissions o
LEFT JOIN sys.database_principals dp ON o.grantee_principal_id = dp.principal_id
WHERE dp.name = 'sa';
```

## Test Checklist

- [ ] Fresh installation completes without errors
- [ ] All 5 databases created
- [ ] All tables created with correct schema
- [ ] All indexes created
- [ ] All constraints working (FK, CHECK, UNIQUE)
- [ ] Seed data inserted correctly
- [ ] Update script works on existing databases
- [ ] Scripts are idempotent (can run multiple times)
- [ ] Rollback script works correctly
- [ ] Foreign key cascades work
- [ ] Check constraints prevent invalid data
- [ ] Unique constraints prevent duplicates
- [ ] Indexes improve query performance
- [ ] APIs can connect to databases
- [ ] CRUD operations work through APIs

## Reporting Issues

If tests fail, collect this information:

```bash
# SQL Server version
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "SELECT @@VERSION"

# Database list
sqlcmd -S localhost,1433 -U sa -P Admin@12345 -Q "SELECT name, state_desc FROM sys.databases WHERE name LIKE 'IFMS_%'"

# Error logs
docker logs ifms-sqlserver

# Script output
./run-migrations.sh > migration-output.log 2>&1
cat migration-output.log
```

---

**Note:** Always test migrations in a development environment before applying to production!
