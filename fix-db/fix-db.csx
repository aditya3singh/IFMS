#!/usr/bin/env dotnet-script
#r "nuget: Microsoft.Data.SqlClient, 5.2.0"

using Microsoft.Data.SqlClient;

var connStr = "Server=localhost,1433;Database=IFMS_StationDB;User Id=sa;Password=Admin@12345;TrustServerCertificate=True";

using var conn = new SqlConnection(connStr);
await conn.OpenAsync();
Console.WriteLine("Connected to IFMS_StationDB");

// Fix dealer assignment
var sql = @"
DELETE FROM DealerAssignments WHERE StationId = '44444444-4444-4444-4444-444444444444';
INSERT INTO DealerAssignments (Id, StationId, UserId, AssignedAt) 
VALUES (NEWID(), '44444444-4444-4444-4444-444444444444', '24265265-964e-4b2d-824e-7edee476e621', GETUTCDATE());
SELECT COUNT(*) FROM DealerAssignments WHERE StationId = '44444444-4444-4444-4444-444444444444';
";

using var cmd = new SqlCommand(sql, conn);
var result = await cmd.ExecuteScalarAsync();
Console.WriteLine($"Dealer assignment fixed. Rows: {result}");
