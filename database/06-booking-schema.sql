-- =============================================
-- Bharat Kinetic IFMS - Booking Database Schema
-- Version: 1.0.0
-- Description: Creates Booking database tables and indexes
-- =============================================

USE IFMS_BookingDB;
GO

-- Bookings Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Bookings')
BEGIN
    CREATE TABLE Bookings (
        BookingId UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CustomerId UNIQUEIDENTIFIER NOT NULL,
        StationId UNIQUEIDENTIFIER NOT NULL,
        FuelType NVARCHAR(20) NOT NULL CHECK (FuelType IN ('Petrol', 'Diesel', 'CNG', 'Electric')),
        QuantityLiters DECIMAL(10, 2) NOT NULL,
        TotalPaid DECIMAL(12, 2) NOT NULL,
        -- IFM-{stationId:D2}-{8 random digits} => 15 chars (e.g., IFM-00-12345678)
        TokenCode NVARCHAR(20) NOT NULL UNIQUE,
        TokenStatus NVARCHAR(20) NOT NULL DEFAULT 'PENDING' CHECK (TokenStatus IN ('PENDING', 'USED', 'EXPIRED', 'CANCELLED')),
        PaymentId NVARCHAR(100) NOT NULL,
        BookedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME2 NOT NULL,
        UsedAt DATETIME2 NULL,
        CONSTRAINT CK_Bookings_Quantity CHECK (QuantityLiters > 0),
        CONSTRAINT CK_Bookings_Amount CHECK (TotalPaid > 0)
    );
    
    CREATE INDEX IX_Bookings_CustomerId ON Bookings(CustomerId);
    CREATE INDEX IX_Bookings_StationId ON Bookings(StationId);
    CREATE INDEX IX_Bookings_TokenCode ON Bookings(TokenCode);
    CREATE INDEX IX_Bookings_TokenStatus ON Bookings(TokenStatus);
    CREATE INDEX IX_Bookings_BookedAt ON Bookings(BookedAt);
    CREATE INDEX IX_Bookings_ExpiresAt ON Bookings(ExpiresAt);
    
    PRINT 'Table Bookings created successfully.';
END
GO

-- KYC Verifications Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'KycVerifications')
BEGIN
    CREATE TABLE KycVerifications (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        CustomerId UNIQUEIDENTIFIER NOT NULL,
        DocumentType NVARCHAR(20) NOT NULL CHECK (DocumentType IN ('PAN', 'Aadhaar', 'DrivingLicense', 'Passport')),
        DocumentNumber NVARCHAR(50) NOT NULL,
        VerificationStatus NVARCHAR(20) NOT NULL DEFAULT 'Pending' CHECK (VerificationStatus IN ('Pending', 'Verified', 'Rejected', 'Expired')),
        VerifiedAt DATETIME2 NULL,
        ExpiresAt DATETIME2 NULL,
        RejectionReason NVARCHAR(500) NULL,
        ProviderResponse NVARCHAR(MAX) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UpdatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT UQ_KycVerifications_Customer UNIQUE (CustomerId, DocumentType)
    );
    
    CREATE INDEX IX_KycVerifications_CustomerId ON KycVerifications(CustomerId);
    CREATE INDEX IX_KycVerifications_DocumentNumber ON KycVerifications(DocumentNumber);
    CREATE INDEX IX_KycVerifications_VerificationStatus ON KycVerifications(VerificationStatus);
    
    PRINT 'Table KycVerifications created successfully.';
END
GO

-- Booking History View (for reporting)
IF NOT EXISTS (SELECT * FROM sys.views WHERE name = 'vw_BookingHistory')
BEGIN
    EXEC('
    CREATE VIEW vw_BookingHistory AS
    SELECT 
        BookingId,
        CustomerId,
        StationId,
        FuelType,
        QuantityLiters,
        TotalPaid,
        TokenCode,
        TokenStatus,
        PaymentId,
        BookedAt,
        ExpiresAt,
        UsedAt,
        CASE 
            WHEN TokenStatus = ''USED'' THEN ''Completed''
            WHEN TokenStatus = ''EXPIRED'' THEN ''Expired''
            WHEN TokenStatus = ''CANCELLED'' THEN ''Cancelled''
            WHEN ExpiresAt < GETUTCDATE() THEN ''Expired''
            ELSE ''Active''
        END AS DisplayStatus,
        DATEDIFF(HOUR, BookedAt, ISNULL(UsedAt, GETUTCDATE())) AS HoursToUse
    FROM Bookings
    ');
    
    PRINT 'View vw_BookingHistory created successfully.';
END
GO

PRINT 'Booking database schema created successfully.';
GO
