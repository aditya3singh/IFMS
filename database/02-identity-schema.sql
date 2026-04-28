-- =============================================
-- Bharat Kinetic IFMS - Identity Database Schema
-- Version: 1.0.0
-- Description: Creates Identity database tables and indexes
-- =============================================

USE IFMS_IdentityDB;
GO

-- Users Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Users')
BEGIN
    CREATE TABLE Users (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        FullName NVARCHAR(100) NOT NULL,
        Email NVARCHAR(150) NOT NULL UNIQUE,
        PhoneNumber NVARCHAR(20) NULL,
        PasswordHash NVARCHAR(MAX) NULL,
        GoogleSubjectId NVARCHAR(255) NULL,
        Role NVARCHAR(50) NOT NULL CHECK (Role IN ('Customer', 'Dealer', 'Admin')),
        IsActive BIT NOT NULL DEFAULT 1,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT CK_Users_Role CHECK (Role IN ('Customer', 'Dealer', 'Admin'))
    );
    
    CREATE INDEX IX_Users_Email ON Users(Email);
    CREATE INDEX IX_Users_Role ON Users(Role);
    CREATE UNIQUE INDEX IX_Users_PhoneNumber ON Users(PhoneNumber) WHERE PhoneNumber IS NOT NULL;
    CREATE UNIQUE INDEX IX_Users_GoogleSubjectId ON Users(GoogleSubjectId) WHERE GoogleSubjectId IS NOT NULL;
    
    PRINT 'Table Users created successfully.';
END
GO

-- OTP Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Otps')
BEGIN
    CREATE TABLE Otps (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        OtpCode NVARCHAR(10) NOT NULL,
        Purpose NVARCHAR(50) NOT NULL CHECK (Purpose IN ('Registration', 'Login', 'PasswordReset', 'PhoneVerification')),
        ExpiresAt DATETIME2 NOT NULL,
        IsUsed BIT NOT NULL DEFAULT 0,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        UsedAt DATETIME2 NULL,
        CONSTRAINT FK_Otps_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_Otps_UserId ON Otps(UserId);
    CREATE INDEX IX_Otps_OtpCode ON Otps(OtpCode);
    CREATE INDEX IX_Otps_ExpiresAt ON Otps(ExpiresAt);
    
    PRINT 'Table Otps created successfully.';
END
GO

-- Refresh Tokens Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'RefreshTokens')
BEGIN
    CREATE TABLE RefreshTokens (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        Token NVARCHAR(500) NOT NULL UNIQUE,
        ExpiresAt DATETIME2 NOT NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        RevokedAt DATETIME2 NULL,
        IsRevoked BIT NOT NULL DEFAULT 0,
        DeviceInfo NVARCHAR(500) NULL,
        IpAddress NVARCHAR(50) NULL,
        CONSTRAINT FK_RefreshTokens_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_RefreshTokens_UserId ON RefreshTokens(UserId);
    CREATE INDEX IX_RefreshTokens_Token ON RefreshTokens(Token);
    
    PRINT 'Table RefreshTokens created successfully.';
END
GO

-- User Sessions Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'UserSessions')
BEGIN
    CREATE TABLE UserSessions (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NOT NULL,
        SessionToken NVARCHAR(500) NOT NULL UNIQUE,
        DeviceType NVARCHAR(50) NULL,
        DeviceName NVARCHAR(200) NULL,
        Browser NVARCHAR(100) NULL,
        IpAddress NVARCHAR(50) NULL,
        Location NVARCHAR(200) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        LastActivityAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        ExpiresAt DATETIME2 NOT NULL,
        IsActive BIT NOT NULL DEFAULT 1,
        CONSTRAINT FK_UserSessions_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE CASCADE
    );
    
    CREATE INDEX IX_UserSessions_UserId ON UserSessions(UserId);
    CREATE INDEX IX_UserSessions_SessionToken ON UserSessions(SessionToken);
    
    PRINT 'Table UserSessions created successfully.';
END
GO

-- Audit Log Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'AuditLogs')
BEGIN
    CREATE TABLE AuditLogs (
        Id UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
        UserId UNIQUEIDENTIFIER NULL,
        Action NVARCHAR(100) NOT NULL,
        EntityType NVARCHAR(100) NULL,
        EntityId NVARCHAR(100) NULL,
        OldValues NVARCHAR(MAX) NULL,
        NewValues NVARCHAR(MAX) NULL,
        IpAddress NVARCHAR(50) NULL,
        UserAgent NVARCHAR(500) NULL,
        CreatedAt DATETIME2 NOT NULL DEFAULT GETUTCDATE(),
        CONSTRAINT FK_AuditLogs_Users FOREIGN KEY (UserId) REFERENCES Users(Id) ON DELETE SET NULL
    );
    
    CREATE INDEX IX_AuditLogs_UserId ON AuditLogs(UserId);
    CREATE INDEX IX_AuditLogs_Action ON AuditLogs(Action);
    CREATE INDEX IX_AuditLogs_CreatedAt ON AuditLogs(CreatedAt);
    
    PRINT 'Table AuditLogs created successfully.';
END
GO

PRINT 'Identity database schema created successfully.';
GO
