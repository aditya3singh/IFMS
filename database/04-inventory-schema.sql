-- =============================================
-- Bharat Kinetic IFMS - Inventory Database Schema
-- Version: 1.0.0
-- Description: Creates Inventory database tables and indexes
-- =============================================

USE IFMS_InventoryDB;
GO

-- Fuel Stock Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FuelStocks')
BEGIN
    CREATE TABLE FuelStocks (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        StationId UNIQUEIDENTIFIER NOT NULL,
        FuelType NVARCHAR(20) NOT NULL CHECK (FuelType IN ('Petrol', 'Diesel', 'CNG', 'Electric')),
        Quantity DECIMAL(12, 2) NOT NULL DEFAULT 0,
        PricePerLitre DECIMAL(10, 2) NOT NULL,
        Status NVARCHAR(20) NOT NULL DEFAULT 'Available' CHECK (Status IN ('Available', 'OutOfStock', 'LowStock')),
        LastUpdated DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT CK_FuelStocks_Quantity CHECK (Quantity >= 0),
        CONSTRAINT CK_FuelStocks_Price CHECK (PricePerLitre > 0),
        CONSTRAINT UQ_FuelStocks_StationFuel UNIQUE (StationId, FuelType)
    );
    
    CREATE INDEX IX_FuelStocks_StationId ON FuelStocks(StationId);
    CREATE INDEX IX_FuelStocks_FuelType ON FuelStocks(FuelType);
    CREATE INDEX IX_FuelStocks_Status ON FuelStocks(Status);
    
    PRINT 'Table FuelStocks created successfully.';
END
GO

-- Stock Movements Table (for tracking inventory changes)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StockMovements')
BEGIN
    CREATE TABLE StockMovements (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        StationId UNIQUEIDENTIFIER NOT NULL,
        FuelType NVARCHAR(20) NOT NULL,
        MovementType NVARCHAR(20) NOT NULL CHECK (MovementType IN ('Purchase', 'Sale', 'Adjustment', 'Transfer')),
        Quantity DECIMAL(12, 2) NOT NULL,
        PreviousQuantity DECIMAL(12, 2) NOT NULL,
        NewQuantity DECIMAL(12, 2) NOT NULL,
        Reason NVARCHAR(500) NULL,
        ReferenceId NVARCHAR(100) NULL,
        CreatedBy UNIQUEIDENTIFIER NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT CK_StockMovements_Quantity CHECK (Quantity != 0)
    );
    
    CREATE INDEX IX_StockMovements_StationId ON StockMovements(StationId);
    CREATE INDEX IX_StockMovements_FuelType ON StockMovements(FuelType);
    CREATE INDEX IX_StockMovements_MovementType ON StockMovements(MovementType);
    CREATE INDEX IX_StockMovements_CreatedAt ON StockMovements(CreatedAt);
    
    PRINT 'Table StockMovements created successfully.';
END
GO

-- Fuel Types Reference Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'FuelTypes')
BEGIN
    CREATE TABLE FuelTypes (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(20) NOT NULL UNIQUE,
        DisplayName NVARCHAR(50) NOT NULL,
        Unit NVARCHAR(10) NOT NULL DEFAULT 'Litre',
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX IX_FuelTypes_Name ON FuelTypes(Name);
    CREATE INDEX IX_FuelTypes_IsActive ON FuelTypes(IsActive);
    
    PRINT 'Table FuelTypes created successfully.';
END
GO

PRINT 'Inventory database schema created successfully.';
GO
