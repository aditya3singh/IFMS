-- =============================================
-- Bharat Kinetic IFMS - Sales Database Schema
-- Version: 1.0.0
-- Description: Creates Sales database tables and indexes
-- =============================================

USE IFMS_SalesDB;
GO

-- Transactions Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Transactions')
BEGIN
    CREATE TABLE Transactions (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        StationId UNIQUEIDENTIFIER NOT NULL,
        FuelType NVARCHAR(20) NOT NULL,
        Quantity DECIMAL(10, 2) NOT NULL,
        PricePerLitre DECIMAL(10, 2) NOT NULL,
        TotalAmount DECIMAL(12, 2) NOT NULL,
        PaymentMethod NVARCHAR(50) NOT NULL CHECK (PaymentMethod IN ('Cash', 'Card', 'UPI', 'Wallet', 'Token')),
        Status NVARCHAR(20) NOT NULL DEFAULT 'Completed' CHECK (Status IN ('Pending', 'Completed', 'Failed', 'Refunded')),
        TransactionDate DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CustomerName NVARCHAR(200) NULL,
        CustomerId UNIQUEIDENTIFIER NULL,
        BookingId UNIQUEIDENTIFIER NULL,
        TokenCode NVARCHAR(20) NULL,
        ReferenceNumber NVARCHAR(100) NULL,
        Notes NVARCHAR(500) NULL,
        CONSTRAINT CK_Transactions_Quantity CHECK (Quantity > 0),
        CONSTRAINT CK_Transactions_Price CHECK (PricePerLitre > 0),
        CONSTRAINT CK_Transactions_Amount CHECK (TotalAmount > 0)
    );
    
    CREATE INDEX IX_Transactions_StationId ON Transactions(StationId);
    CREATE INDEX IX_Transactions_FuelType ON Transactions(FuelType);
    CREATE INDEX IX_Transactions_PaymentMethod ON Transactions(PaymentMethod);
    CREATE INDEX IX_Transactions_Status ON Transactions(Status);
    CREATE INDEX IX_Transactions_TransactionDate ON Transactions(TransactionDate);
    CREATE INDEX IX_Transactions_CustomerId ON Transactions(CustomerId);
    CREATE INDEX IX_Transactions_BookingId ON Transactions(BookingId);
    CREATE INDEX IX_Transactions_TokenCode ON Transactions(TokenCode);
    
    PRINT 'Table Transactions created successfully.';
END
GO

-- Daily Sales Summary Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'DailySalesSummary')
BEGIN
    CREATE TABLE DailySalesSummary (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        StationId UNIQUEIDENTIFIER NOT NULL,
        SaleDate DATE NOT NULL,
        FuelType NVARCHAR(20) NOT NULL,
        TotalQuantity DECIMAL(12, 2) NOT NULL DEFAULT 0,
        TotalRevenue DECIMAL(15, 2) NOT NULL DEFAULT 0,
        TransactionCount INT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_DailySalesSummary UNIQUE (StationId, SaleDate, FuelType)
    );
    
    CREATE INDEX IX_DailySalesSummary_StationId ON DailySalesSummary(StationId);
    CREATE INDEX IX_DailySalesSummary_SaleDate ON DailySalesSummary(SaleDate);
    CREATE INDEX IX_DailySalesSummary_FuelType ON DailySalesSummary(FuelType);
    
    PRINT 'Table DailySalesSummary created successfully.';
END
GO

-- Payment Methods Reference Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'PaymentMethods')
BEGIN
    CREATE TABLE PaymentMethods (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        Name NVARCHAR(50) NOT NULL UNIQUE,
        DisplayName NVARCHAR(100) NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        ProcessingFeePercent DECIMAL(5, 2) NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE()
    );
    
    CREATE INDEX IX_PaymentMethods_Name ON PaymentMethods(Name);
    CREATE INDEX IX_PaymentMethods_IsActive ON PaymentMethods(IsActive);
    
    PRINT 'Table PaymentMethods created successfully.';
END
GO

PRINT 'Sales database schema created successfully.';
GO
