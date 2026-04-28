-- =============================================
-- Bharat Kinetic IFMS - Update Existing Database Script
-- Version: 1.0.0
-- Description: Updates existing databases with new schema changes
-- =============================================

PRINT 'Starting database update process...';
GO

-- =============================================
-- UPDATE IDENTITY DATABASE
-- =============================================
USE IFMS_IdentityDB;
GO

PRINT 'Updating Identity database...';

-- Align with current Identity service model (FullName + optional GoogleSubjectId)
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'FullName')
BEGIN
    ALTER TABLE Users ADD FullName NVARCHAR(100) NOT NULL CONSTRAINT DF_Users_FullName DEFAULT('');
    PRINT 'Added FullName column to Users table.';

    -- Best-effort migration from FirstName/LastName (older schema) -> FullName
    IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'FirstName')
       AND EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'LastName')
    BEGIN
        UPDATE Users
        SET FullName = LTRIM(RTRIM(COALESCE(FirstName, '') + ' ' + COALESCE(LastName, '')))
        WHERE FullName = '';
        PRINT 'Migrated FirstName/LastName to FullName.';
    END
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'PhoneNumber')
BEGIN
    ALTER TABLE Users ADD PhoneNumber NVARCHAR(20) NULL;
    PRINT 'Added PhoneNumber column to Users table.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'GoogleSubjectId')
BEGIN
    ALTER TABLE Users ADD GoogleSubjectId NVARCHAR(255) NULL;
    PRINT 'Added GoogleSubjectId column to Users table.';
END

-- If legacy FirstName/LastName exist, ensure they are nullable (Identity service doesn't write them).
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'FirstName')
BEGIN
    ALTER TABLE Users ALTER COLUMN FirstName NVARCHAR(100) NULL;
END
IF EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Users') AND name = 'LastName')
BEGIN
    ALTER TABLE Users ALTER COLUMN LastName NVARCHAR(100) NULL;
END

-- Create missing unique filtered indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_PhoneNumber')
BEGIN
    CREATE UNIQUE INDEX IX_Users_PhoneNumber ON Users(PhoneNumber) WHERE PhoneNumber IS NOT NULL;
    PRINT 'Created index IX_Users_PhoneNumber.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Users_GoogleSubjectId')
BEGIN
    CREATE UNIQUE INDEX IX_Users_GoogleSubjectId ON Users(GoogleSubjectId) WHERE GoogleSubjectId IS NOT NULL;
    PRINT 'Created index IX_Users_GoogleSubjectId.';
END

PRINT 'Identity database updated successfully.';
GO

-- =============================================
-- UPDATE BOOKING DATABASE
-- =============================================
USE IFMS_BookingDB;
GO

PRINT 'Updating Booking database...';

-- Add new columns to Bookings table if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Bookings') AND name = 'TokenStatus')
BEGIN
    ALTER TABLE Bookings ADD TokenStatus NVARCHAR(20) NOT NULL DEFAULT 'PENDING';
    PRINT 'Added TokenStatus column to Bookings table.';
END

-- Update TokenStatus constraint if it exists
IF EXISTS (SELECT * FROM sys.check_constraints WHERE name = 'CK_Bookings_TokenStatus')
BEGIN
    ALTER TABLE Bookings DROP CONSTRAINT CK_Bookings_TokenStatus;
END

ALTER TABLE Bookings ADD CONSTRAINT CK_Bookings_TokenStatus 
    CHECK (TokenStatus IN ('PENDING', 'USED', 'EXPIRED', 'CANCELLED'));
PRINT 'Updated TokenStatus constraint.';

-- Create missing indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bookings_CustomerId')
BEGIN
    CREATE INDEX IX_Bookings_CustomerId ON Bookings(CustomerId);
    PRINT 'Created index IX_Bookings_CustomerId.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bookings_StationId')
BEGIN
    CREATE INDEX IX_Bookings_StationId ON Bookings(StationId);
    PRINT 'Created index IX_Bookings_StationId.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bookings_TokenStatus')
BEGIN
    CREATE INDEX IX_Bookings_TokenStatus ON Bookings(TokenStatus);
    PRINT 'Created index IX_Bookings_TokenStatus.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Bookings_ExpiresAt')
BEGIN
    CREATE INDEX IX_Bookings_ExpiresAt ON Bookings(ExpiresAt);
    PRINT 'Created index IX_Bookings_ExpiresAt.';
END

PRINT 'Booking database updated successfully.';
GO

-- =============================================
-- UPDATE STATION DATABASE
-- =============================================
USE IFMS_StationDB;
GO

PRINT 'Updating Station database...';

-- Add UpdatedAt column if it doesn't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Stations') AND name = 'UpdatedAt')
BEGIN
    ALTER TABLE Stations ADD UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE();
    PRINT 'Added UpdatedAt column to Stations table.';
END

-- Create missing indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Stations_City')
BEGIN
    CREATE INDEX IX_Stations_City ON Stations(City);
    PRINT 'Created index IX_Stations_City.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Stations_State')
BEGIN
    CREATE INDEX IX_Stations_State ON Stations(State);
    PRINT 'Created index IX_Stations_State.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Stations_Location')
BEGIN
    CREATE INDEX IX_Stations_Location ON Stations(Latitude, Longitude);
    PRINT 'Created index IX_Stations_Location.';
END

PRINT 'Station database updated successfully.';
GO

-- =============================================
-- UPDATE INVENTORY DATABASE
-- =============================================
USE IFMS_InventoryDB;
GO

PRINT 'Updating Inventory database...';

-- Rename table if old name exists
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'Inventory') 
   AND NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FuelStocks')
BEGIN
    EXEC sp_rename 'Inventory', 'FuelStocks';
    PRINT 'Renamed Inventory table to FuelStocks.';
END

-- Add Status column if it doesn't exist
IF EXISTS (SELECT * FROM sys.tables WHERE name = 'FuelStocks')
   AND NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('FuelStocks') AND name = 'Status')
BEGIN
    ALTER TABLE FuelStocks ADD Status NVARCHAR(20) NOT NULL DEFAULT 'Available';
    PRINT 'Added Status column to FuelStocks table.';
END

PRINT 'Inventory database updated successfully.';
GO

-- =============================================
-- UPDATE SALES DATABASE
-- =============================================
USE IFMS_SalesDB;
GO

PRINT 'Updating Sales database...';

-- Add new columns to Transactions table if they don't exist
IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Transactions') AND name = 'CustomerId')
BEGIN
    ALTER TABLE Transactions ADD CustomerId UNIQUEIDENTIFIER NULL;
    PRINT 'Added CustomerId column to Transactions table.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Transactions') AND name = 'BookingId')
BEGIN
    ALTER TABLE Transactions ADD BookingId UNIQUEIDENTIFIER NULL;
    PRINT 'Added BookingId column to Transactions table.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Transactions') AND name = 'TokenCode')
BEGIN
    ALTER TABLE Transactions ADD TokenCode NVARCHAR(20) NULL;
    PRINT 'Added TokenCode column to Transactions table.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Transactions') AND name = 'ReferenceNumber')
BEGIN
    ALTER TABLE Transactions ADD ReferenceNumber NVARCHAR(100) NULL;
    PRINT 'Added ReferenceNumber column to Transactions table.';
END

IF NOT EXISTS (SELECT * FROM sys.columns WHERE object_id = OBJECT_ID('Transactions') AND name = 'Notes')
BEGIN
    ALTER TABLE Transactions ADD Notes NVARCHAR(500) NULL;
    PRINT 'Added Notes column to Transactions table.';
END

-- Create missing indexes
IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Transactions_CustomerId')
BEGIN
    CREATE INDEX IX_Transactions_CustomerId ON Transactions(CustomerId);
    PRINT 'Created index IX_Transactions_CustomerId.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Transactions_BookingId')
BEGIN
    CREATE INDEX IX_Transactions_BookingId ON Transactions(BookingId);
    PRINT 'Created index IX_Transactions_BookingId.';
END

IF NOT EXISTS (SELECT * FROM sys.indexes WHERE name = 'IX_Transactions_TokenCode')
BEGIN
    CREATE INDEX IX_Transactions_TokenCode ON Transactions(TokenCode);
    PRINT 'Created index IX_Transactions_TokenCode.';
END

PRINT 'Sales database updated successfully.';
GO

PRINT 'All databases updated successfully.';
GO
