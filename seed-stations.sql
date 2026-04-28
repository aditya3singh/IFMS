-- Seed / refresh IFMS station rows (Indian outlets — matches RegionalFuelPriceQuote states)
-- Adjust USE to your Station DB name (e.g. IFMS_StationDb from appsettings).

-- USE IFMS_StationDb;
-- GO

IF NOT EXISTS (SELECT 1 FROM Stations WHERE LicenseNumber IN ('IND-LIC-001', 'IND-LIC-002', 'IND-LIC-003', 'IND-LIC-004', 'IND-LIC-005'))
BEGIN
    INSERT INTO Stations (Id, Name, LicenseNumber, City, State, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt)
    VALUES
        ('11111111-1111-1111-1111-111111111111', N'Western Express Fuel Point', N'IND-LIC-001', N'Mumbai', N'Maharashtra', 19.076000, 72.877700, 1, '2026-01-01 00:00:00', '2026-04-05 00:00:00'),
        ('22222222-2222-2222-2222-222222222222', N'Silicon Corridor Pump', N'IND-LIC-002', N'Bengaluru', N'Karnataka', 12.971600, 77.594600, 1, '2026-01-01 00:00:00', '2026-04-05 00:00:00'),
        ('33333333-3333-3333-3333-333333333333', N'NCR Central Energy', N'IND-LIC-003', N'New Delhi', N'Delhi', 28.613900, 77.209000, 1, '2026-01-01 00:00:00', '2026-04-05 00:00:00'),
        ('44444444-4444-4444-4444-444444444444', N'HITEC City Fuels', N'IND-LIC-004', N'Hyderabad', N'Telangana', 17.385000, 78.486700, 1, '2026-01-01 00:00:00', '2026-04-05 00:00:00'),
        ('55555555-5555-5555-5555-555555555555', N'Sabarmati Retail Outlet', N'IND-LIC-005', N'Ahmedabad', N'Gujarat', 23.022500, 72.571400, 1, '2026-01-01 00:00:00', '2026-04-05 00:00:00');
    PRINT 'Inserted 5 Indian seed stations';
END
ELSE
BEGIN
    UPDATE Stations SET Name=N'Western Express Fuel Point', LicenseNumber=N'IND-LIC-001', City=N'Mumbai', State=N'Maharashtra', Latitude=19.076000, Longitude=72.877700, UpdatedAt=GETUTCDATE() WHERE Id='11111111-1111-1111-1111-111111111111';
    UPDATE Stations SET Name=N'Silicon Corridor Pump', LicenseNumber=N'IND-LIC-002', City=N'Bengaluru', State=N'Karnataka', Latitude=12.971600, Longitude=77.594600, UpdatedAt=GETUTCDATE() WHERE Id='22222222-2222-2222-2222-222222222222';
    UPDATE Stations SET Name=N'NCR Central Energy', LicenseNumber=N'IND-LIC-003', City=N'New Delhi', State=N'Delhi', Latitude=28.613900, Longitude=77.209000, UpdatedAt=GETUTCDATE() WHERE Id='33333333-3333-3333-3333-333333333333';
    UPDATE Stations SET Name=N'HITEC City Fuels', LicenseNumber=N'IND-LIC-004', City=N'Hyderabad', State=N'Telangana', Latitude=17.385000, Longitude=78.486700, UpdatedAt=GETUTCDATE() WHERE Id='44444444-4444-4444-4444-444444444444';
    UPDATE Stations SET Name=N'Sabarmati Retail Outlet', LicenseNumber=N'IND-LIC-005', City=N'Ahmedabad', State=N'Gujarat', Latitude=23.022500, Longitude=72.571400, UpdatedAt=GETUTCDATE() WHERE Id='55555555-5555-5555-5555-555555555555';
    PRINT 'Updated existing rows to Indian outlets';
END
GO

SELECT Id, Name, LicenseNumber, City, State, IsActive FROM Stations WHERE IsActive = 1 ORDER BY Name;
GO
