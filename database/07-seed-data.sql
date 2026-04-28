-- =============================================
-- Bharat Kinetic IFMS - Seed Data Script
-- Version: 1.0.0
-- Description: Inserts initial seed data for all databases
-- =============================================

-- =============================================
-- IDENTITY DATABASE SEED DATA
-- =============================================
USE IFMS_IdentityDB;
GO

-- Seed Admin User (Password: Admin@123)
-- NOTE:
-- Users are created via the Identity API (`/gateway/auth/register`) so their password hashes
-- are generated correctly (BCrypt). We intentionally do not seed users here.
PRINT 'Skipping user seeding (use API registration).';
GO

-- =============================================
-- STATION DATABASE SEED DATA
-- =============================================
USE IFMS_StationDB;
GO

-- Seed Stations
IF NOT EXISTS (SELECT * FROM Stations)
BEGIN
    DECLARE @Station1 UNIQUEIDENTIFIER = NEWID();
    DECLARE @Station2 UNIQUEIDENTIFIER = NEWID();
    DECLARE @Station3 UNIQUEIDENTIFIER = NEWID();
    DECLARE @Station4 UNIQUEIDENTIFIER = NEWID();
    DECLARE @Station5 UNIQUEIDENTIFIER = NEWID();

    INSERT INTO Stations (Id, Name, LicenseNumber, City, State, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt)
    VALUES
        (@Station1, 'Bharat Kinetic Fuel Station - MG Road', 'BK-BLR-001', 'Bangalore', 'Karnataka', 12.9716, 77.5946, 1, GETUTCDATE(), GETUTCDATE()),
        (@Station2, 'Bharat Kinetic Fuel Station - Whitefield', 'BK-BLR-002', 'Bangalore', 'Karnataka', 12.9698, 77.7499, 1, GETUTCDATE(), GETUTCDATE()),
        (@Station3, 'Bharat Kinetic Fuel Station - Electronic City', 'BK-BLR-003', 'Bangalore', 'Karnataka', 12.8456, 77.6603, 1, GETUTCDATE(), GETUTCDATE()),
        (@Station4, 'Bharat Kinetic Fuel Station - Koramangala', 'BK-BLR-004', 'Bangalore', 'Karnataka', 12.9352, 77.6245, 1, GETUTCDATE(), GETUTCDATE()),
        (@Station5, 'Bharat Kinetic Fuel Station - Indiranagar', 'BK-BLR-005', 'Bangalore', 'Karnataka', 12.9784, 77.6408, 1, GETUTCDATE(), GETUTCDATE());

    PRINT 'Stations seeded successfully.';

    -- Seed Station Pricing
    INSERT INTO StationPricing (Id, StationId, FuelType, PricePerLitre, EffectiveFrom, IsActive, UpdatedAt)
    VALUES
        (NEWID(), @Station1, 'Petrol', 102.50, GETUTCDATE(), 1, GETUTCDATE()),
        (NEWID(), @Station1, 'Diesel', 89.75, GETUTCDATE(), 1, GETUTCDATE()),
        (NEWID(), @Station1, 'CNG', 75.00, GETUTCDATE(), 1, GETUTCDATE()),
        (NEWID(), @Station2, 'Petrol', 103.00, GETUTCDATE(), 1, GETUTCDATE()),
        (NEWID(), @Station2, 'Diesel', 90.00, GETUTCDATE(), 1, GETUTCDATE()),
        (NEWID(), @Station2, 'CNG', 75.50, GETUTCDATE(), 1, GETUTCDATE()),
        (NEWID(), @Station3, 'Petrol', 102.75, GETUTCDATE(), 1, GETUTCDATE()),
        (NEWID(), @Station3, 'Diesel', 89.50, GETUTCDATE(), 1, GETUTCDATE()),
        (NEWID(), @Station4, 'Petrol', 102.25, GETUTCDATE(), 1, GETUTCDATE()),
        (NEWID(), @Station4, 'Diesel', 89.25, GETUTCDATE(), 1, GETUTCDATE()),
        (NEWID(), @Station4, 'CNG', 74.75, GETUTCDATE(), 1, GETUTCDATE()),
        (NEWID(), @Station5, 'Petrol', 103.25, GETUTCDATE(), 1, GETUTCDATE()),
        (NEWID(), @Station5, 'Diesel', 90.25, GETUTCDATE(), 1, GETUTCDATE());

    PRINT 'Station pricing seeded successfully.';
END
GO

-- =============================================
-- INVENTORY DATABASE SEED DATA
-- =============================================
USE IFMS_InventoryDB;
GO

-- Seed Fuel Types
IF NOT EXISTS (SELECT * FROM FuelTypes)
BEGIN
    INSERT INTO FuelTypes (Id, Name, DisplayName, Unit, IsActive, CreatedAt)
    VALUES
        (NEWID(), 'Petrol', 'Petrol (Gasoline)', 'Litre', 1, GETUTCDATE()),
        (NEWID(), 'Diesel', 'Diesel', 'Litre', 1, GETUTCDATE()),
        (NEWID(), 'CNG', 'Compressed Natural Gas', 'Kg', 1, GETUTCDATE()),
        (NEWID(), 'Electric', 'Electric Charging', 'kWh', 1, GETUTCDATE());

    PRINT 'Fuel types seeded successfully.';
END
GO

-- Seed Initial Fuel Stock (using Station IDs from Station DB)
-- Note: In production, you would need to fetch actual Station IDs
DECLARE @StationIds TABLE (Id UNIQUEIDENTIFIER);
INSERT INTO @StationIds
SELECT TOP 5 Id FROM IFMS_StationDB.dbo.Stations ORDER BY CreatedAt;

IF NOT EXISTS (SELECT * FROM FuelStocks)
BEGIN
    DECLARE @StationId UNIQUEIDENTIFIER;
    DECLARE station_cursor CURSOR FOR SELECT Id FROM @StationIds;
    
    OPEN station_cursor;
    FETCH NEXT FROM station_cursor INTO @StationId;
    
    WHILE @@FETCH_STATUS = 0
    BEGIN
        INSERT INTO FuelStocks (Id, StationId, FuelType, Quantity, PricePerLitre, Status, LastUpdated)
        VALUES
            (NEWID(), @StationId, 'Petrol', 5000.00, 102.50, 'Available', GETUTCDATE()),
            (NEWID(), @StationId, 'Diesel', 7500.00, 89.75, 'Available', GETUTCDATE()),
            (NEWID(), @StationId, 'CNG', 3000.00, 75.00, 'Available', GETUTCDATE());
        
        FETCH NEXT FROM station_cursor INTO @StationId;
    END
    
    CLOSE station_cursor;
    DEALLOCATE station_cursor;
    
    PRINT 'Fuel stocks seeded successfully.';
END
GO

-- =============================================
-- SALES DATABASE SEED DATA
-- =============================================
USE IFMS_SalesDB;
GO

-- Seed Payment Methods
IF NOT EXISTS (SELECT * FROM PaymentMethods)
BEGIN
    INSERT INTO PaymentMethods (Id, Name, DisplayName, IsActive, ProcessingFeePercent, CreatedAt)
    VALUES
        (NEWID(), 'Cash', 'Cash Payment', 1, 0.00, GETUTCDATE()),
        (NEWID(), 'Card', 'Credit/Debit Card', 1, 1.50, GETUTCDATE()),
        (NEWID(), 'UPI', 'UPI Payment', 1, 0.50, GETUTCDATE()),
        (NEWID(), 'Wallet', 'Digital Wallet', 1, 1.00, GETUTCDATE()),
        (NEWID(), 'Token', 'Pre-booked Token', 1, 0.00, GETUTCDATE());

    PRINT 'Payment methods seeded successfully.';
END
GO

PRINT 'All seed data inserted successfully.';
GO
