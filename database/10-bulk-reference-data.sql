-- =============================================
-- Bharat Kinetic IFMS - Bulk reference data
-- Description: Inserts ~150 completed sales transactions (REF-BULK-*) for
--              dashboards, lists, and API testing. Safe to re-run: skips rows
--              that already exist (ReferenceNumber).
-- Prerequisites: IFMS_StationDB has stations (07-seed-data.sql), IFMS_SalesDB schema.
-- =============================================

USE IFMS_SalesDB;
GO

SET NOCOUNT ON;

IF OBJECT_ID('tempdb..#BulkRefStations') IS NOT NULL
    DROP TABLE #BulkRefStations;

SELECT Id,
       ROW_NUMBER() OVER (ORDER BY LicenseNumber) AS ix
INTO #BulkRefStations
FROM IFMS_StationDB.dbo.Stations;

DECLARE @cnt INT = (SELECT COUNT(*) FROM #BulkRefStations);

IF @cnt = 0
BEGIN
    PRINT 'ERROR: No stations in IFMS_StationDB. Run 07-seed-data.sql first.';
END
ELSE
BEGIN
    ;WITH N AS (
        SELECT TOP 150
               ROW_NUMBER() OVER (ORDER BY (SELECT NULL)) AS n
        FROM sys.all_objects a
        CROSS JOIN sys.all_objects b
    ),
    R AS (
        SELECT
            N.n,
            S.Id AS StationId,
            CASE (N.n % 3)
                WHEN 1 THEN 'Petrol'
                WHEN 2 THEN 'Diesel'
                ELSE 'CNG'
            END AS FuelType,
            CAST(5.0 + (N.n % 40) + (N.n % 7) * 0.25 AS DECIMAL(10, 2)) AS Quantity,
            CASE (N.n % 3)
                WHEN 1 THEN CAST(102.0 + (N.n % 25) * 0.05 AS DECIMAL(10, 2))
                WHEN 2 THEN CAST(89.0 + (N.n % 20) * 0.05 AS DECIMAL(10, 2))
                ELSE CAST(74.5 + (N.n % 15) * 0.05 AS DECIMAL(10, 2))
            END AS PricePerLitre
        FROM N
        INNER JOIN #BulkRefStations S ON S.ix = ((N.n - 1) % @cnt) + 1
    )
    INSERT INTO dbo.Transactions (
        Id,
        StationId,
        FuelType,
        Quantity,
        PricePerLitre,
        TotalAmount,
        PaymentMethod,
        Status,
        TransactionDate,
        CustomerName,
        ReferenceNumber,
        Notes
    )
    SELECT
        NEWID(),
        R.StationId,
        R.FuelType,
        R.Quantity,
        R.PricePerLitre,
        CAST(R.Quantity * R.PricePerLitre AS DECIMAL(12, 2)) AS TotalAmount,
        CASE (R.n % 5)
            WHEN 0 THEN 'Cash'
            WHEN 1 THEN 'Card'
            WHEN 2 THEN 'UPI'
            WHEN 3 THEN 'Wallet'
            ELSE 'Token'
        END AS PaymentMethod,
        'Completed' AS Status,
        DATEADD(
            MINUTE,
            (R.n * 17) % 1440,
            DATEADD(DAY, -((R.n) % 90), CAST(CONVERT(date, DATEADD(DAY, -1, GETUTCDATE())) AS DATETIME2))
        ) AS TransactionDate,
        CONCAT('Ref Customer ', R.n) AS CustomerName,
        CONCAT('REF-BULK-', RIGHT(CONCAT('000', CAST(R.n AS VARCHAR(10))), 3)) AS ReferenceNumber,
        'Bulk seed reference transaction' AS Notes
    FROM R
    WHERE NOT EXISTS (
        SELECT 1
        FROM dbo.Transactions t
        WHERE t.ReferenceNumber = CONCAT('REF-BULK-', RIGHT(CONCAT('000', CAST(R.n AS VARCHAR(10))), 3))
    );

    PRINT CONCAT('Bulk reference transactions inserted: ', @@ROWCOUNT);
END;
GO

DROP TABLE IF EXISTS #BulkRefStations;
GO

PRINT '10-bulk-reference-data.sql completed.';
GO
