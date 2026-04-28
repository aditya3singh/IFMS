-- =============================================
-- Bharat Kinetic IFMS  ·  Full Bulk Seed Script
-- File   : 11-bulk-seed-full.sql
-- Author : Antigravity (auto-generated)
-- Purpose: Insert ~130 users, full station inventory (max stock),
--          ~130 bookings, and ~130 sales transactions.
--          Safe to re-run:  each section skips rows that already exist.
-- Prerequisites: Run 01 → 10 first (schemas + base seed).
-- =============================================

SET NOCOUNT ON;
GO

-- =============================================
-- SECTION 1 · IDENTITY DB — 50 Customers + 10 Dealers
-- =============================================
USE IFMS_IdentityDB;
GO

-- BCrypt hash for "Pass@1234"  (cost 10, pre-computed fixed hash for seeded accounts)
DECLARE @PwdHash NVARCHAR(MAX) = N'$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lkii';

-- 50 Customers
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'customer001@ifms.in')
BEGIN
    INSERT INTO Users (Id, FullName, Email, PhoneNumber, PasswordHash, Role, IsActive, CreatedAt)
    VALUES
      ('A0000001-0000-0000-0000-000000000001', N'Aditya Sharma',      'customer001@ifms.in', '9800000001', @PwdHash, 'Customer', 1, '2026-01-02 08:00:00'),
      ('A0000001-0000-0000-0000-000000000002', N'Bhavna Patel',       'customer002@ifms.in', '9800000002', @PwdHash, 'Customer', 1, '2026-01-03 09:00:00'),
      ('A0000001-0000-0000-0000-000000000003', N'Chetan Mehta',       'customer003@ifms.in', '9800000003', @PwdHash, 'Customer', 1, '2026-01-04 10:00:00'),
      ('A0000001-0000-0000-0000-000000000004', N'Divya Nair',         'customer004@ifms.in', '9800000004', @PwdHash, 'Customer', 1, '2026-01-05 08:30:00'),
      ('A0000001-0000-0000-0000-000000000005', N'Eshan Rao',          'customer005@ifms.in', '9800000005', @PwdHash, 'Customer', 1, '2026-01-06 11:00:00'),
      ('A0000001-0000-0000-0000-000000000006', N'Falguni Desai',      'customer006@ifms.in', '9800000006', @PwdHash, 'Customer', 1, '2026-01-07 07:00:00'),
      ('A0000001-0000-0000-0000-000000000007', N'Gaurav Singh',       'customer007@ifms.in', '9800000007', @PwdHash, 'Customer', 1, '2026-01-08 13:00:00'),
      ('A0000001-0000-0000-0000-000000000008', N'Harini Krishnan',    'customer008@ifms.in', '9800000008', @PwdHash, 'Customer', 1, '2026-01-09 14:00:00'),
      ('A0000001-0000-0000-0000-000000000009', N'Ishaan Verma',       'customer009@ifms.in', '9800000009', @PwdHash, 'Customer', 1, '2026-01-10 15:00:00'),
      ('A0000001-0000-0000-0000-000000000010', N'Jaya Iyer',          'customer010@ifms.in', '9800000010', @PwdHash, 'Customer', 1, '2026-01-11 16:00:00'),
      ('A0000001-0000-0000-0000-000000000011', N'Kiran Malhotra',     'customer011@ifms.in', '9800000011', @PwdHash, 'Customer', 1, '2026-01-12 08:00:00'),
      ('A0000001-0000-0000-0000-000000000012', N'Lavanya Reddy',      'customer012@ifms.in', '9800000012', @PwdHash, 'Customer', 1, '2026-01-13 09:00:00'),
      ('A0000001-0000-0000-0000-000000000013', N'Manish Gupta',       'customer013@ifms.in', '9800000013', @PwdHash, 'Customer', 1, '2026-01-14 10:00:00'),
      ('A0000001-0000-0000-0000-000000000014', N'Nalini Joshi',       'customer014@ifms.in', '9800000014', @PwdHash, 'Customer', 1, '2026-01-15 11:00:00'),
      ('A0000001-0000-0000-0000-000000000015', N'Om Prakash',         'customer015@ifms.in', '9800000015', @PwdHash, 'Customer', 1, '2026-01-16 12:00:00'),
      ('A0000001-0000-0000-0000-000000000016', N'Priya Saxena',       'customer016@ifms.in', '9800000016', @PwdHash, 'Customer', 1, '2026-01-17 07:30:00'),
      ('A0000001-0000-0000-0000-000000000017', N'Qasim Khan',         'customer017@ifms.in', '9800000017', @PwdHash, 'Customer', 1, '2026-01-18 08:45:00'),
      ('A0000001-0000-0000-0000-000000000018', N'Rashmi Tiwari',      'customer018@ifms.in', '9800000018', @PwdHash, 'Customer', 1, '2026-01-19 09:15:00'),
      ('A0000001-0000-0000-0000-000000000019', N'Siddharth Pillai',   'customer019@ifms.in', '9800000019', @PwdHash, 'Customer', 1, '2026-01-20 10:30:00'),
      ('A0000001-0000-0000-0000-000000000020', N'Tanvi Bhatt',        'customer020@ifms.in', '9800000020', @PwdHash, 'Customer', 1, '2026-01-21 11:45:00'),
      ('A0000001-0000-0000-0000-000000000021', N'Uday Kumar',         'customer021@ifms.in', '9800000021', @PwdHash, 'Customer', 1, '2026-01-22 12:00:00'),
      ('A0000001-0000-0000-0000-000000000022', N'Vandana Mishra',     'customer022@ifms.in', '9800000022', @PwdHash, 'Customer', 1, '2026-01-23 13:00:00'),
      ('A0000001-0000-0000-0000-000000000023', N'Waqar Ahmed',        'customer023@ifms.in', '9800000023', @PwdHash, 'Customer', 1, '2026-01-24 14:00:00'),
      ('A0000001-0000-0000-0000-000000000024', N'Xena D''souza',      'customer024@ifms.in', '9800000024', @PwdHash, 'Customer', 1, '2026-01-25 15:00:00'),
      ('A0000001-0000-0000-0000-000000000025', N'Yashraj Thakur',     'customer025@ifms.in', '9800000025', @PwdHash, 'Customer', 1, '2026-01-26 16:00:00'),
      ('A0000001-0000-0000-0000-000000000026', N'Zara Siddiqui',      'customer026@ifms.in', '9800000026', @PwdHash, 'Customer', 1, '2026-01-27 08:00:00'),
      ('A0000001-0000-0000-0000-000000000027', N'Arjun Bose',         'customer027@ifms.in', '9800000027', @PwdHash, 'Customer', 1, '2026-01-28 09:00:00'),
      ('A0000001-0000-0000-0000-000000000028', N'Bhumika Kaur',       'customer028@ifms.in', '9800000028', @PwdHash, 'Customer', 1, '2026-01-29 10:00:00'),
      ('A0000001-0000-0000-0000-000000000029', N'Chirag Menon',       'customer029@ifms.in', '9800000029', @PwdHash, 'Customer', 1, '2026-01-30 11:00:00'),
      ('A0000001-0000-0000-0000-000000000030', N'Deepal Shah',        'customer030@ifms.in', '9800000030', @PwdHash, 'Customer', 1, '2026-01-31 12:00:00'),
      ('A0000001-0000-0000-0000-000000000031', N'Esha Pandey',        'customer031@ifms.in', '9800000031', @PwdHash, 'Customer', 1, '2026-02-01 08:00:00'),
      ('A0000001-0000-0000-0000-000000000032', N'Farhan Mirza',       'customer032@ifms.in', '9800000032', @PwdHash, 'Customer', 1, '2026-02-02 09:00:00'),
      ('A0000001-0000-0000-0000-000000000033', N'Gargi Chatterjee',   'customer033@ifms.in', '9800000033', @PwdHash, 'Customer', 1, '2026-02-03 10:00:00'),
      ('A0000001-0000-0000-0000-000000000034', N'Hemant Shukla',      'customer034@ifms.in', '9800000034', @PwdHash, 'Customer', 1, '2026-02-04 11:00:00'),
      ('A0000001-0000-0000-0000-000000000035', N'Isha Kapoor',        'customer035@ifms.in', '9800000035', @PwdHash, 'Customer', 1, '2026-02-05 12:00:00'),
      ('A0000001-0000-0000-0000-000000000036', N'Jai Prakash',        'customer036@ifms.in', '9800000036', @PwdHash, 'Customer', 1, '2026-02-06 08:30:00'),
      ('A0000001-0000-0000-0000-000000000037', N'Kavya Nambiar',      'customer037@ifms.in', '9800000037', @PwdHash, 'Customer', 1, '2026-02-07 09:30:00'),
      ('A0000001-0000-0000-0000-000000000038', N'Lalit Dubey',        'customer038@ifms.in', '9800000038', @PwdHash, 'Customer', 1, '2026-02-08 10:30:00'),
      ('A0000001-0000-0000-0000-000000000039', N'Meena Pillai',       'customer039@ifms.in', '9800000039', @PwdHash, 'Customer', 1, '2026-02-09 11:30:00'),
      ('A0000001-0000-0000-0000-000000000040', N'Nikhil Agarwal',     'customer040@ifms.in', '9800000040', @PwdHash, 'Customer', 1, '2026-02-10 12:30:00'),
      ('A0000001-0000-0000-0000-000000000041', N'Ojasvi Bajaj',       'customer041@ifms.in', '9800000041', @PwdHash, 'Customer', 1, '2026-02-11 08:00:00'),
      ('A0000001-0000-0000-0000-000000000042', N'Pallavi Ghosh',      'customer042@ifms.in', '9800000042', @PwdHash, 'Customer', 1, '2026-02-12 09:00:00'),
      ('A0000001-0000-0000-0000-000000000043', N'Rahul Anand',        'customer043@ifms.in', '9800000043', @PwdHash, 'Customer', 1, '2026-02-13 10:00:00'),
      ('A0000001-0000-0000-0000-000000000044', N'Shruti Kulkarni',    'customer044@ifms.in', '9800000044', @PwdHash, 'Customer', 1, '2026-02-14 11:00:00'),
      ('A0000001-0000-0000-0000-000000000045', N'Tarun Soni',         'customer045@ifms.in', '9800000045', @PwdHash, 'Customer', 1, '2026-02-15 12:00:00'),
      ('A0000001-0000-0000-0000-000000000046', N'Umesh Patil',        'customer046@ifms.in', '9800000046', @PwdHash, 'Customer', 1, '2026-02-16 13:00:00'),
      ('A0000001-0000-0000-0000-000000000047', N'Veena Rajan',        'customer047@ifms.in', '9800000047', @PwdHash, 'Customer', 1, '2026-02-17 14:00:00'),
      ('A0000001-0000-0000-0000-000000000048', N'Wasim Qureshi',      'customer048@ifms.in', '9800000048', @PwdHash, 'Customer', 1, '2026-02-18 15:00:00'),
      ('A0000001-0000-0000-0000-000000000049', N'Yogesh Tomar',       'customer049@ifms.in', '9800000049', @PwdHash, 'Customer', 1, '2026-02-19 16:00:00'),
      ('A0000001-0000-0000-0000-000000000050', N'Zoya Begum',         'customer050@ifms.in', '9800000050', @PwdHash, 'Customer', 1, '2026-02-20 08:00:00');
    PRINT 'Inserted 50 customer users.';
END
GO

-- 10 Dealers (one per station — 5 existing + 5 new)
IF NOT EXISTS (SELECT 1 FROM Users WHERE Email = 'dealer001@ifms.in')
BEGIN
    DECLARE @PwdHash2 NVARCHAR(MAX) = N'$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lkii';
    INSERT INTO Users (Id, FullName, Email, PhoneNumber, PasswordHash, Role, IsActive, CreatedAt)
    VALUES
      ('B0000001-0000-0000-0000-000000000001', N'Dealer Mumbai',      'dealer001@ifms.in', '9900000001', @PwdHash2, 'Dealer', 1, '2026-01-01 06:00:00'),
      ('B0000001-0000-0000-0000-000000000002', N'Dealer Bengaluru',   'dealer002@ifms.in', '9900000002', @PwdHash2, 'Dealer', 1, '2026-01-01 06:10:00'),
      ('B0000001-0000-0000-0000-000000000003', N'Dealer New Delhi',   'dealer003@ifms.in', '9900000003', @PwdHash2, 'Dealer', 1, '2026-01-01 06:20:00'),
      ('B0000001-0000-0000-0000-000000000004', N'Dealer Hyderabad',   'dealer004@ifms.in', '9900000004', @PwdHash2, 'Dealer', 1, '2026-01-01 06:30:00'),
      ('B0000001-0000-0000-0000-000000000005', N'Dealer Ahmedabad',   'dealer005@ifms.in', '9900000005', @PwdHash2, 'Dealer', 1, '2026-01-01 06:40:00'),
      ('B0000001-0000-0000-0000-000000000006', N'Dealer Chennai',     'dealer006@ifms.in', '9900000006', @PwdHash2, 'Dealer', 1, '2026-01-01 06:50:00'),
      ('B0000001-0000-0000-0000-000000000007', N'Dealer Pune',        'dealer007@ifms.in', '9900000007', @PwdHash2, 'Dealer', 1, '2026-01-01 07:00:00'),
      ('B0000001-0000-0000-0000-000000000008', N'Dealer Kolkata',     'dealer008@ifms.in', '9900000008', @PwdHash2, 'Dealer', 1, '2026-01-01 07:10:00'),
      ('B0000001-0000-0000-0000-000000000009', N'Dealer Jaipur',      'dealer009@ifms.in', '9900000009', @PwdHash2, 'Dealer', 1, '2026-01-01 07:20:00'),
      ('B0000001-0000-0000-0000-000000000010', N'Dealer Lucknow',     'dealer010@ifms.in', '9900000010', @PwdHash2, 'Dealer', 1, '2026-01-01 07:30:00');
    PRINT 'Inserted 10 dealer users.';
END
GO

-- =============================================
-- SECTION 2 · STATION DB — 5 extra stations + pricing
-- =============================================
USE IFMS_StationDB;
GO

IF NOT EXISTS (SELECT 1 FROM Stations WHERE LicenseNumber = 'IND-LIC-006')
BEGIN
    INSERT INTO Stations (Id, Name, LicenseNumber, City, State, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt)
    VALUES
      ('66666666-6666-6666-6666-666666666666', N'Marina Fuel Hub',         N'IND-LIC-006', N'Chennai',   N'Tamil Nadu',       13.082700, 80.270700, 1, '2026-01-01', '2026-04-05'),
      ('77777777-7777-7777-7777-777777777777', N'Deccan Energy Point',      N'IND-LIC-007', N'Pune',      N'Maharashtra',      18.520400, 73.856700, 1, '2026-01-01', '2026-04-05'),
      ('88888888-8888-8888-8888-888888888888', N'Hooghly River Fuels',      N'IND-LIC-008', N'Kolkata',   N'West Bengal',      22.572600, 88.363900, 1, '2026-01-01', '2026-04-05'),
      ('99999999-9999-9999-9999-999999999999', N'Pink City Petroleum',      N'IND-LIC-009', N'Jaipur',    N'Rajasthan',        26.912400, 75.787300, 1, '2026-01-01', '2026-04-05'),
      ('AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', N'Nawabi Fuel Station',     N'IND-LIC-010', N'Lucknow',   N'Uttar Pradesh',    26.846900, 80.946200, 1, '2026-01-01', '2026-04-05');
    PRINT 'Inserted 5 additional stations.';

    -- Pricing for new stations
    INSERT INTO StationPricing (Id, StationId, FuelType, PricePerLitre, EffectiveFrom, IsActive, UpdatedAt)
    VALUES
      (NEWID(), '66666666-6666-6666-6666-666666666666', 'Petrol', 101.85, GETUTCDATE(), 1, GETUTCDATE()),
      (NEWID(), '66666666-6666-6666-6666-666666666666', 'Diesel', 88.90, GETUTCDATE(), 1, GETUTCDATE()),
      (NEWID(), '66666666-6666-6666-6666-666666666666', 'CNG',    74.20, GETUTCDATE(), 1, GETUTCDATE()),
      (NEWID(), '77777777-7777-7777-7777-777777777777', 'Petrol', 102.10, GETUTCDATE(), 1, GETUTCDATE()),
      (NEWID(), '77777777-7777-7777-7777-777777777777', 'Diesel', 89.10, GETUTCDATE(), 1, GETUTCDATE()),
      (NEWID(), '77777777-7777-7777-7777-777777777777', 'CNG',    74.55, GETUTCDATE(), 1, GETUTCDATE()),
      (NEWID(), '88888888-8888-8888-8888-888888888888', 'Petrol', 101.60, GETUTCDATE(), 1, GETUTCDATE()),
      (NEWID(), '88888888-8888-8888-8888-888888888888', 'Diesel', 88.70, GETUTCDATE(), 1, GETUTCDATE()),
      (NEWID(), '99999999-9999-9999-9999-999999999999', 'Petrol', 103.50, GETUTCDATE(), 1, GETUTCDATE()),
      (NEWID(), '99999999-9999-9999-9999-999999999999', 'Diesel', 90.50, GETUTCDATE(), 1, GETUTCDATE()),
      (NEWID(), '99999999-9999-9999-9999-999999999999', 'CNG',    75.80, GETUTCDATE(), 1, GETUTCDATE()),
      (NEWID(), 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 'Petrol', 102.75, GETUTCDATE(), 1, GETUTCDATE()),
      (NEWID(), 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 'Diesel', 89.30, GETUTCDATE(), 1, GETUTCDATE()),
      (NEWID(), 'AAAAAAAA-AAAA-AAAA-AAAA-AAAAAAAAAAAA', 'CNG',    75.10, GETUTCDATE(), 1, GETUTCDATE());
    PRINT 'Inserted pricing for new stations.';
END
GO

-- =============================================
-- SECTION 3 · INVENTORY DB — Full Stock Fill
--   For EVERY station (existing + new):
--   · Petrol  = 10 000 L  (max capacity)
--   · Diesel  = 15 000 L  (max capacity)
--   · CNG     =  5 000 Kg (max capacity)
--   · Electric=  2 000 kWh (max capacity)
--   Uses MERGE so existing rows are updated to full, new rows are inserted.
-- =============================================
USE IFMS_InventoryDB;
GO

-- Ensure FuelTypes reference rows exist
IF NOT EXISTS (SELECT 1 FROM FuelTypes WHERE Name = 'Petrol')
    INSERT INTO FuelTypes (Id, Name, DisplayName, Unit, IsActive, CreatedAt)
    VALUES
      (NEWID(), 'Petrol',   'Petrol (Gasoline)',      'Litre', 1, GETUTCDATE()),
      (NEWID(), 'Diesel',   'Diesel',                 'Litre', 1, GETUTCDATE()),
      (NEWID(), 'CNG',      'Compressed Natural Gas', 'Kg',    1, GETUTCDATE()),
      (NEWID(), 'Electric', 'Electric Charging',      'kWh',   1, GETUTCDATE());
GO

-- Helper: build a station→fuel cross-join with desired max quantities
DECLARE @MaxStock TABLE (
    FuelType       NVARCHAR(20),
    MaxQty         DECIMAL(12,2),
    PricePerLitre  DECIMAL(10,2)
);
INSERT INTO @MaxStock VALUES
  ('Petrol',    10000.00, 102.50),
  ('Diesel',    15000.00,  89.75),
  ('CNG',        5000.00,  75.00),
  ('Electric',   2000.00,   8.50);

-- Cross-join all stations from StationDB with all fuel types
IF OBJECT_ID('tempdb..#InventoryTarget') IS NOT NULL DROP TABLE #InventoryTarget;
SELECT
    s.Id   AS StationId,
    m.FuelType,
    m.MaxQty,
    m.PricePerLitre
INTO #InventoryTarget
FROM IFMS_StationDB.dbo.Stations s
CROSS JOIN @MaxStock m
WHERE s.IsActive = 1;

-- UPSERT: update existing rows to full; insert missing rows
MERGE INTO FuelStocks AS tgt
USING #InventoryTarget AS src
   ON tgt.StationId = src.StationId AND tgt.FuelType = src.FuelType
WHEN MATCHED THEN
    UPDATE SET
        tgt.Quantity      = src.MaxQty,
        tgt.PricePerLitre = src.PricePerLitre,
        tgt.Status        = 'Available',
        tgt.LastUpdated   = GETUTCDATE()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, StationId, FuelType, Quantity, PricePerLitre, Status, LastUpdated)
    VALUES (NEWID(), src.StationId, src.FuelType, src.MaxQty, src.PricePerLitre, 'Available', GETUTCDATE());

PRINT CONCAT('FuelStocks upserted for all stations. Rows affected: ', @@ROWCOUNT);
GO

DROP TABLE IF EXISTS #InventoryTarget;
GO

-- Stock Movements: record a "Purchase / Full Refill" event for each station×fuel
-- Skip if movements already seeded for this run (check by Reason prefix)
IF NOT EXISTS (SELECT 1 FROM StockMovements WHERE Reason LIKE 'Initial full refill%')
BEGIN
    INSERT INTO StockMovements
        (Id, StationId, FuelType, MovementType, Quantity, PreviousQuantity, NewQuantity, Reason, ReferenceId, CreatedAt)
    SELECT
        NEWID(),
        fs.StationId,
        fs.FuelType,
        'Purchase',
        fs.Quantity,       -- Quantity moved  = full stock
        0,                 -- PreviousQuantity = 0 (start)
        fs.Quantity,       -- NewQuantity
        CONCAT('Initial full refill – station ', fs.StationId, ' – ', fs.FuelType),
        CONCAT('REFILL-', LEFT(CAST(fs.StationId AS VARCHAR(36)), 8), '-', fs.FuelType),
        GETUTCDATE()
    FROM FuelStocks fs;
    PRINT CONCAT('StockMovements inserted: ', @@ROWCOUNT);
END
GO

-- =============================================
-- SECTION 4 · BOOKING DB — 130 Bookings
-- =============================================
USE IFMS_BookingDB;
GO

IF NOT EXISTS (SELECT 1 FROM Bookings WHERE TokenCode LIKE 'IFM-SEED-%')
BEGIN
    -- Build a numbered list of (Station, Customer, FuelType) combinations
    IF OBJECT_ID('tempdb..#BookingSeeds') IS NOT NULL DROP TABLE #BookingSeeds;
    WITH Nums AS (
        SELECT TOP 130
               ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
        FROM sys.all_objects a CROSS JOIN sys.all_objects b
    ),
    Stations AS (
        SELECT Id, ROW_NUMBER() OVER (ORDER BY LicenseNumber) AS ix
        FROM IFMS_StationDB.dbo.Stations WHERE IsActive = 1
    ),
    Customers AS (
        SELECT Id, ROW_NUMBER() OVER (ORDER BY Email) AS ix
        FROM IFMS_IdentityDB.dbo.Users WHERE Role = 'Customer'
    )
    SELECT
        NEWID()                                                   AS BookingId,
        c.Id                                                      AS CustomerId,
        s.Id                                                      AS StationId,
        CASE (n.n % 3)
            WHEN 1 THEN 'Petrol'
            WHEN 2 THEN 'Diesel'
            ELSE 'CNG'
        END                                                       AS FuelType,
        CAST(5.0 + (n.n % 35) + (n.n % 5) * 0.5 AS DECIMAL(10,2)) AS QuantityLiters,
        CASE (n.n % 3)
            WHEN 1 THEN CAST((5.0 + (n.n % 35) + (n.n % 5)*0.5) * 102.50 AS DECIMAL(12,2))
            WHEN 2 THEN CAST((5.0 + (n.n % 35) + (n.n % 5)*0.5) *  89.75 AS DECIMAL(12,2))
            ELSE       CAST((5.0 + (n.n % 35) + (n.n % 5)*0.5) *  75.00 AS DECIMAL(12,2))
        END                                                       AS TotalPaid,
        CONCAT('IFM-SEED-', RIGHT(CONCAT('00000', CAST(n.n AS VARCHAR)), 5)) AS TokenCode,
        CASE
            WHEN n.n % 10 = 0 THEN 'EXPIRED'
            WHEN n.n % 7  = 0 THEN 'USED'
            WHEN n.n % 13 = 0 THEN 'CANCELLED'
            ELSE 'PENDING'
        END                                                       AS TokenStatus,
        CONCAT('PAY-SEED-', RIGHT(CONCAT('00000', CAST(n.n AS VARCHAR)), 5)) AS PaymentId,
        DATEADD(MINUTE, (n.n * 23) % 1440,
            DATEADD(DAY, -((n.n) % 60),
                CAST(CONVERT(date, DATEADD(DAY, -1, GETUTCDATE())) AS DATETIME2)))
                                                                  AS BookedAt,
        DATEADD(HOUR, 24,
            DATEADD(MINUTE, (n.n * 23) % 1440,
                DATEADD(DAY, -((n.n) % 60),
                    CAST(CONVERT(date, DATEADD(DAY, -1, GETUTCDATE())) AS DATETIME2))))
                                                                  AS ExpiresAt,
        CASE WHEN n.n % 7 = 0  -- USED bookings get a UsedAt
            THEN DATEADD(HOUR, 2,
                DATEADD(MINUTE, (n.n * 23) % 1440,
                    DATEADD(DAY, -((n.n) % 60),
                        CAST(CONVERT(date, DATEADD(DAY, -1, GETUTCDATE())) AS DATETIME2))))
            ELSE NULL
        END                                                       AS UsedAt
    INTO #BookingSeeds
    FROM Nums n
    INNER JOIN Stations  s ON s.ix = ((n.n - 1) % (SELECT COUNT(*) FROM Stations)) + 1
    INNER JOIN Customers c ON c.ix = ((n.n - 1) % (SELECT COUNT(*) FROM Customers)) + 1;

    INSERT INTO Bookings
        (BookingId, CustomerId, StationId, FuelType, QuantityLiters, TotalPaid,
         TokenCode, TokenStatus, PaymentId, BookedAt, ExpiresAt, UsedAt)
    SELECT
        BookingId, CustomerId, StationId, FuelType, QuantityLiters, TotalPaid,
        TokenCode, TokenStatus, PaymentId, BookedAt, ExpiresAt, UsedAt
    FROM #BookingSeeds;

    PRINT CONCAT('Bookings inserted: ', @@ROWCOUNT);
    DROP TABLE #BookingSeeds;
END
GO

-- =============================================
-- SECTION 5 · SALES DB — 130 Transactions + Daily Summary
-- =============================================
USE IFMS_SalesDB;
GO

IF NOT EXISTS (SELECT 1 FROM Transactions WHERE ReferenceNumber LIKE 'TXN-SEED-%')
BEGIN
    IF OBJECT_ID('tempdb..#TxnSeeds') IS NOT NULL DROP TABLE #TxnSeeds;
    WITH Nums AS (
        SELECT TOP 130
               ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
        FROM sys.all_objects a CROSS JOIN sys.all_objects b
    ),
    Stations AS (
        SELECT Id, ROW_NUMBER() OVER (ORDER BY LicenseNumber) AS ix
        FROM IFMS_StationDB.dbo.Stations WHERE IsActive = 1
    ),
    Customers AS (
        SELECT Id, FullName, ROW_NUMBER() OVER (ORDER BY Email) AS ix
        FROM IFMS_IdentityDB.dbo.Users WHERE Role = 'Customer'
    )
    SELECT
        NEWID()                                                    AS Id,
        s.Id                                                       AS StationId,
        CASE (n.n % 3)
            WHEN 1 THEN 'Petrol'
            WHEN 2 THEN 'Diesel'
            ELSE 'CNG'
        END                                                        AS FuelType,
        CAST(5.0 + (n.n % 40) + (n.n % 6) * 0.25 AS DECIMAL(10,2)) AS Quantity,
        CASE (n.n % 3)
            WHEN 1 THEN CAST(102.0 + (n.n % 20) * 0.05 AS DECIMAL(10,2))
            WHEN 2 THEN CAST( 89.0 + (n.n % 15) * 0.05 AS DECIMAL(10,2))
            ELSE       CAST( 74.5 + (n.n % 10) * 0.05 AS DECIMAL(10,2))
        END                                                        AS PricePerLitre,
        CASE (n.n % 5)
            WHEN 0 THEN 'Cash'
            WHEN 1 THEN 'Card'
            WHEN 2 THEN 'UPI'
            WHEN 3 THEN 'Wallet'
            ELSE 'Token'
        END                                                        AS PaymentMethod,
        CASE WHEN n.n % 20 = 0 THEN 'Failed'
             WHEN n.n % 15 = 0 THEN 'Refunded'
             ELSE 'Completed'
        END                                                        AS Status,
        DATEADD(MINUTE, (n.n * 17) % 1440,
            DATEADD(DAY, -((n.n) % 90),
                CAST(CONVERT(date, DATEADD(DAY, -1, GETUTCDATE())) AS DATETIME2)))
                                                                   AS TransactionDate,
        c.FullName                                                 AS CustomerName,
        c.Id                                                       AS CustomerId,
        CONCAT('TXN-SEED-', RIGHT(CONCAT('00000', CAST(n.n AS VARCHAR)), 5)) AS ReferenceNumber,
        'Seeded bulk transaction'                                  AS Notes
    INTO #TxnSeeds
    FROM Nums n
    INNER JOIN Stations  s ON s.ix = ((n.n - 1) % (SELECT COUNT(*) FROM Stations)) + 1
    INNER JOIN Customers c ON c.ix = ((n.n - 1) % (SELECT COUNT(*) FROM Customers)) + 1;

    -- Compute TotalAmount properly
    INSERT INTO Transactions
        (Id, StationId, FuelType, Quantity, PricePerLitre, TotalAmount,
         PaymentMethod, Status, TransactionDate, CustomerName, CustomerId, ReferenceNumber, Notes)
    SELECT
        Id, StationId, FuelType, Quantity, PricePerLitre,
        CAST(Quantity * PricePerLitre AS DECIMAL(12,2)),
        PaymentMethod, Status, TransactionDate, CustomerName, CustomerId, ReferenceNumber, Notes
    FROM #TxnSeeds;

    PRINT CONCAT('Sales transactions inserted: ', @@ROWCOUNT);
    DROP TABLE #TxnSeeds;
END
GO

-- Refresh DailySalesSummary for seeded data
MERGE INTO DailySalesSummary AS tgt
USING (
    SELECT
        StationId,
        CAST(TransactionDate AS DATE) AS SaleDate,
        FuelType,
        SUM(Quantity)      AS TotalQuantity,
        SUM(TotalAmount)   AS TotalRevenue,
        COUNT(*)           AS TransactionCount
    FROM Transactions
    WHERE ReferenceNumber LIKE 'TXN-SEED-%' OR ReferenceNumber LIKE 'REF-BULK-%'
    GROUP BY StationId, CAST(TransactionDate AS DATE), FuelType
) AS src
   ON tgt.StationId = src.StationId
  AND tgt.SaleDate  = src.SaleDate
  AND tgt.FuelType  = src.FuelType
WHEN MATCHED THEN
    UPDATE SET
        tgt.TotalQuantity    = src.TotalQuantity,
        tgt.TotalRevenue     = src.TotalRevenue,
        tgt.TransactionCount = src.TransactionCount,
        tgt.UpdatedAt        = GETUTCDATE()
WHEN NOT MATCHED BY TARGET THEN
    INSERT (Id, StationId, SaleDate, FuelType, TotalQuantity, TotalRevenue, TransactionCount, CreatedAt, UpdatedAt)
    VALUES (NEWID(), src.StationId, src.SaleDate, src.FuelType, src.TotalQuantity, src.TotalRevenue, src.TransactionCount, GETUTCDATE(), GETUTCDATE());

PRINT CONCAT('DailySalesSummary upserted. Rows affected: ', @@ROWCOUNT);
GO

-- =============================================
-- VERIFICATION SUMMARY
-- =============================================
PRINT '=== VERIFICATION ===';
GO
USE IFMS_IdentityDB;  SELECT 'Users'        AS [Table], COUNT(*) AS [Rows] FROM Users;         GO
USE IFMS_StationDB;   SELECT 'Stations'     AS [Table], COUNT(*) AS [Rows] FROM Stations;      GO
USE IFMS_StationDB;   SELECT 'Pricing'      AS [Table], COUNT(*) AS [Rows] FROM StationPricing; GO
USE IFMS_InventoryDB; SELECT 'FuelStocks'   AS [Table], COUNT(*) AS [Rows] FROM FuelStocks;    GO
USE IFMS_InventoryDB; SELECT 'StockMovements' AS [Table], COUNT(*) AS [Rows] FROM StockMovements; GO
USE IFMS_BookingDB;   SELECT 'Bookings'     AS [Table], COUNT(*) AS [Rows] FROM Bookings;      GO
USE IFMS_SalesDB;     SELECT 'Transactions' AS [Table], COUNT(*) AS [Rows] FROM Transactions;   GO
USE IFMS_SalesDB;     SELECT 'DailySummary' AS [Table], COUNT(*) AS [Rows] FROM DailySalesSummary; GO

PRINT '11-bulk-seed-full.sql completed successfully.';
GO
