-- =============================================
-- Bharat Kinetic IFMS - Database Creation Script
-- Version: 1.0.0
-- Description: Creates all required databases for IFMS
-- =============================================

USE master;
GO

-- Create databases if they don't exist
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'IFMS_IdentityDB')
BEGIN
    CREATE DATABASE IFMS_IdentityDB;
    PRINT 'Database IFMS_IdentityDB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database IFMS_IdentityDB already exists.';
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'IFMS_StationDB')
BEGIN
    CREATE DATABASE IFMS_StationDB;
    PRINT 'Database IFMS_StationDB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database IFMS_StationDB already exists.';
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'IFMS_InventoryDB')
BEGIN
    CREATE DATABASE IFMS_InventoryDB;
    PRINT 'Database IFMS_InventoryDB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database IFMS_InventoryDB already exists.';
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'IFMS_SalesDB')
BEGIN
    CREATE DATABASE IFMS_SalesDB;
    PRINT 'Database IFMS_SalesDB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database IFMS_SalesDB already exists.';
END
GO

IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'IFMS_BookingDB')
BEGIN
    CREATE DATABASE IFMS_BookingDB;
    PRINT 'Database IFMS_BookingDB created successfully.';
END
ELSE
BEGIN
    PRINT 'Database IFMS_BookingDB already exists.';
END
GO

PRINT 'All databases created/verified successfully.';
GO
