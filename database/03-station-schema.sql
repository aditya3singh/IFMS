-- =============================================
-- Bharat Kinetic IFMS - Station Database Schema
-- Version: 1.0.0
-- Description: Creates Station database tables and indexes
-- =============================================

USE IFMS_StationDB;
GO

-- Stations Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Stations')
BEGIN
    CREATE TABLE Stations (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(200) NOT NULL,
        LicenseNumber NVARCHAR(50) NOT NULL UNIQUE,
        City NVARCHAR(100) NOT NULL,
        State NVARCHAR(100) NOT NULL,
        Latitude DECIMAL(10, 7) NOT NULL,
        Longitude DECIMAL(10, 7) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT CK_Stations_Latitude CHECK (Latitude BETWEEN -90 AND 90),
        CONSTRAINT CK_Stations_Longitude CHECK (Longitude BETWEEN -180 AND 180)
    );
    
    CREATE INDEX IX_Stations_LicenseNumber ON Stations(LicenseNumber);
    CREATE INDEX IX_Stations_City ON Stations(City);
    CREATE INDEX IX_Stations_State ON Stations(State);
    CREATE INDEX IX_Stations_IsActive ON Stations(IsActive);
    CREATE INDEX IX_Stations_Location ON Stations(Latitude, Longitude);
    
    PRINT 'Table Stations created successfully.';
END
GO

-- Dealer Assignments Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DealerAssignments')
BEGIN
    CREATE TABLE DealerAssignments (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        StationId UNIQUEIDENTIFIER NOT NULL,
        UserId UNIQUEIDENTIFIER NOT NULL,
        AssignedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_DealerAssignments_Stations FOREIGN KEY (StationId) REFERENCES Stations(Id) ON DELETE CASCADE,
        CONSTRAINT UQ_DealerAssignments_StationId UNIQUE (StationId)
    );
    
    CREATE INDEX IX_DealerAssignments_StationId ON DealerAssignments(StationId);
    CREATE INDEX IX_DealerAssignments_UserId ON DealerAssignments(UserId);
    
    PRINT 'Table DealerAssignments created successfully.';
END
GO

-- Station Pricing Table (for dynamic pricing per station)
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'StationPricing')
BEGIN
    CREATE TABLE StationPricing (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        StationId UNIQUEIDENTIFIER NOT NULL,
        FuelType NVARCHAR(20) NOT NULL CHECK (FuelType IN ('Petrol', 'Diesel', 'CNG', 'Electric')),
        PricePerLitre DECIMAL(10, 2) NOT NULL,
        EffectiveFrom DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        EffectiveTo DATETIME2 NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_StationPricing_Stations FOREIGN KEY (StationId) REFERENCES Stations(Id) ON DELETE CASCADE,
        CONSTRAINT CK_StationPricing_Price CHECK (PricePerLitre > 0)
    );
    
    CREATE INDEX IX_StationPricing_StationId ON StationPricing(StationId);
    CREATE INDEX IX_StationPricing_FuelType ON StationPricing(FuelType);
    CREATE INDEX IX_StationPricing_IsActive ON StationPricing(IsActive);
    
    PRINT 'Table StationPricing created successfully.';
END
GO

PRINT 'Station database schema created successfully.';
GO
