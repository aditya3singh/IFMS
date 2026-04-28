-- =============================================
-- Bharat Kinetic IFMS - Rollback Script
-- Version: 1.0.0
-- Description: Drops all databases (USE WITH CAUTION!)
-- =============================================

USE master;
GO

PRINT 'WARNING: This script will DROP all IFMS databases!';
PRINT 'Press Ctrl+C to cancel or wait 5 seconds to continue...';
WAITFOR DELAY '00:00:05';
GO

-- Close all connections to the databases
DECLARE @kill varchar(8000) = '';
SELECT @kill = @kill + 'KILL ' + CONVERT(varchar(5), session_id) + ';'
FROM sys.dm_exec_sessions
WHERE database_id IN (
    DB_ID('IFMS_IdentityDB'),
    DB_ID('IFMS_StationDB'),
    DB_ID('IFMS_InventoryDB'),
    DB_ID('IFMS_SalesDB'),
    DB_ID('IFMS_BookingDB')
);

IF @kill != ''
BEGIN
    PRINT 'Closing active connections...';
    EXEC(@kill);
END
GO

-- Drop databases
IF EXISTS (SELECT name FROM sys.databases WHERE name = 'IFMS_BookingDB')
BEGIN
    DROP DATABASE IFMS_BookingDB;
    PRINT 'Database IFMS_BookingDB dropped.';
END
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'IFMS_SalesDB')
BEGIN
    DROP DATABASE IFMS_SalesDB;
    PRINT 'Database IFMS_SalesDB dropped.';
END
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'IFMS_InventoryDB')
BEGIN
    DROP DATABASE IFMS_InventoryDB;
    PRINT 'Database IFMS_InventoryDB dropped.';
END
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'IFMS_StationDB')
BEGIN
    DROP DATABASE IFMS_StationDB;
    PRINT 'Database IFMS_StationDB dropped.';
END
GO

IF EXISTS (SELECT name FROM sys.databases WHERE name = 'IFMS_IdentityDB')
BEGIN
    DROP DATABASE IFMS_IdentityDB;
    PRINT 'Database IFMS_IdentityDB dropped.';
END
GO

PRINT 'All IFMS databases have been dropped successfully.';
GO
