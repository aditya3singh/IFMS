-- =============================================
-- Bharat Kinetic IFMS - Fix Dealer Assignments
-- File   : 12-fix-dealer-assignments.sql
-- Purpose: Correctly assign each dealer to the station in their matching city.
--          Dealer names follow the pattern "Dealer <City>" so we match on city.
--          Safe to re-run: clears all assignments first, then re-inserts.
-- Prerequisites: Run 11-bulk-seed-full.sql first (dealers + stations must exist).
-- =============================================

SET NOCOUNT ON;
GO

USE IFMS_StationDB;
GO

-- ── Step 1: Remove ALL existing dealer assignments ────────────────────────────
DELETE FROM DealerAssignments;
PRINT CONCAT('Cleared existing dealer assignments. Rows deleted: ', @@ROWCOUNT);
GO

-- ── Step 2: Re-assign each dealer to the station in their city ────────────────
-- Dealer names are "Dealer <City>" (e.g. "Dealer Mumbai" → City = 'Mumbai').
-- We extract the city from the dealer's FullName and match it to Stations.City.
-- Dealers that don't match any station city are assigned to remaining stations.

USE IFMS_StationDB;
GO

-- Build the matched pairs using a cross-database join
IF OBJECT_ID('tempdb..#DealerCityMatch') IS NOT NULL DROP TABLE #DealerCityMatch;

SELECT
    u.Id        AS DealerId,
    u.FullName  AS DealerName,
    -- Extract city: everything after the first space in FullName (e.g. "Dealer Mumbai" → "Mumbai")
    LTRIM(SUBSTRING(u.FullName, CHARINDEX(' ', u.FullName) + 1, LEN(u.FullName))) AS DealerCity,
    s.Id        AS StationId,
    s.Name      AS StationName,
    s.City      AS StationCity
INTO #DealerCityMatch
FROM IFMS_IdentityDB.dbo.Users u
INNER JOIN IFMS_StationDB.dbo.Stations s
    ON s.City = LTRIM(SUBSTRING(u.FullName, CHARINDEX(' ', u.FullName) + 1, LEN(u.FullName)))
WHERE u.Role = 'Dealer'
  AND u.IsActive = 1
  AND s.IsActive = 1;

-- Verify matches before inserting
SELECT DealerName, DealerCity, StationName, StationCity FROM #DealerCityMatch ORDER BY DealerCity;
GO

-- Insert matched assignments
INSERT INTO DealerAssignments (Id, StationId, UserId, AssignedAt)
SELECT
    NEWID(),
    StationId,
    DealerId,
    GETUTCDATE()
FROM #DealerCityMatch;

PRINT CONCAT('Dealer assignments inserted (city-matched): ', @@ROWCOUNT);
GO

DROP TABLE IF EXISTS #DealerCityMatch;
GO

-- ── Step 3: Handle any stations still without a dealer ────────────────────────
-- Assign remaining unassigned dealers (if any) to unassigned stations in order.
IF OBJECT_ID('tempdb..#UnassignedStations') IS NOT NULL DROP TABLE #UnassignedStations;
IF OBJECT_ID('tempdb..#UnassignedDealers')  IS NOT NULL DROP TABLE #UnassignedDealers;

SELECT s.Id AS StationId, ROW_NUMBER() OVER (ORDER BY s.LicenseNumber) AS rn
INTO #UnassignedStations
FROM IFMS_StationDB.dbo.Stations s
WHERE s.IsActive = 1
  AND NOT EXISTS (SELECT 1 FROM DealerAssignments da WHERE da.StationId = s.Id);

SELECT u.Id AS DealerId, ROW_NUMBER() OVER (ORDER BY u.Email) AS rn
INTO #UnassignedDealers
FROM IFMS_IdentityDB.dbo.Users u
WHERE u.Role = 'Dealer'
  AND u.IsActive = 1
  AND NOT EXISTS (SELECT 1 FROM DealerAssignments da WHERE da.UserId = u.Id);

INSERT INTO DealerAssignments (Id, StationId, UserId, AssignedAt)
SELECT NEWID(), us.StationId, ud.DealerId, GETUTCDATE()
FROM #UnassignedStations us
INNER JOIN #UnassignedDealers ud ON ud.rn = us.rn;

PRINT CONCAT('Fallback assignments inserted (unmatched): ', @@ROWCOUNT);
GO

DROP TABLE IF EXISTS #UnassignedStations;
DROP TABLE IF EXISTS #UnassignedDealers;
GO

-- ── Step 4: Verification ──────────────────────────────────────────────────────
PRINT '=== DEALER ASSIGNMENT VERIFICATION ===';
SELECT
    s.Name      AS StationName,
    s.City      AS StationCity,
    s.State     AS StationState,
    u.FullName  AS DealerName,
    u.Email     AS DealerEmail,
    da.AssignedAt
FROM DealerAssignments da
INNER JOIN IFMS_StationDB.dbo.Stations s ON s.Id = da.StationId
INNER JOIN IFMS_IdentityDB.dbo.Users   u ON u.Id = da.UserId
ORDER BY s.City;
GO

-- Stations without a dealer
SELECT s.Name AS UnassignedStation, s.City
FROM IFMS_StationDB.dbo.Stations s
WHERE s.IsActive = 1
  AND NOT EXISTS (SELECT 1 FROM DealerAssignments da WHERE da.StationId = s.Id)
ORDER BY s.City;
GO

PRINT '12-fix-dealer-assignments.sql completed.';
GO
