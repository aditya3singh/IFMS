using Microsoft.Data.SqlClient;

var connStr = "Server=localhost,1433;Database=IFMS_StationDB;User Id=sa;Password=Admin@12345;TrustServerCertificate=True";
using var conn = new SqlConnection(connStr);
await conn.OpenAsync();
Console.WriteLine("Connected to IFMS_StationDB");

using (var cmd = new SqlCommand("DELETE FROM DealerAssignments", conn))
    await cmd.ExecuteNonQueryAsync();
Console.WriteLine("Cleared existing assignments");

var assignments = new[]
{
    ("11111111-1111-1111-1111-111111111111", "b0000001-0000-0000-0000-000000000001"),
    ("22222222-2222-2222-2222-222222222222", "b0000001-0000-0000-0000-000000000002"),
    ("33333333-3333-3333-3333-333333333333", "b0000001-0000-0000-0000-000000000003"),
    ("44444444-4444-4444-4444-444444444444", "24265265-964e-4b2d-824e-7edee476e621"),
    ("55555555-5555-5555-5555-555555555555", "b0000001-0000-0000-0000-000000000005"),
};

var getStationsSql = "SELECT Id, Name FROM Stations WHERE Id NOT IN ('11111111-1111-1111-1111-111111111111','22222222-2222-2222-2222-222222222222','33333333-3333-3333-3333-333333333333','44444444-4444-4444-4444-444444444444','55555555-5555-5555-5555-555555555555')";
var otherStations = new List<(string id, string name)>();
using (var cmd = new SqlCommand(getStationsSql, conn))
using (var reader = await cmd.ExecuteReaderAsync())
    while (await reader.ReadAsync())
        otherStations.Add((reader["Id"].ToString()!, reader["Name"].ToString()!));

var remainingDealers = new[] {
    "b0000001-0000-0000-0000-000000000006",
    "b0000001-0000-0000-0000-000000000007",
    "b0000001-0000-0000-0000-000000000008",
    "b0000001-0000-0000-0000-000000000009",
    "b0000001-0000-0000-0000-000000000010",
};

var allAssignments = assignments.ToList();
for (int i = 0; i < otherStations.Count && i < remainingDealers.Length; i++)
    allAssignments.Add((otherStations[i].id, remainingDealers[i]));

foreach (var (stationId, dealerId) in allAssignments)
{
    var sql = "INSERT INTO DealerAssignments (Id, StationId, UserId, AssignedAt) VALUES (NEWID(), @s, @d, GETUTCDATE())";
    using var cmd = new SqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@s", stationId);
    cmd.Parameters.AddWithValue("@d", dealerId);
    await cmd.ExecuteNonQueryAsync();
}

var verifySql = "SELECT s.Name, s.City, da.UserId FROM Stations s LEFT JOIN DealerAssignments da ON s.Id = da.StationId ORDER BY s.Name";
using (var cmd = new SqlCommand(verifySql, conn))
using (var reader = await cmd.ExecuteReaderAsync())
{
    Console.WriteLine("\nStation -> Dealer:");
    while (await reader.ReadAsync())
        Console.WriteLine($"  {reader["Name"]} ({reader["City"]}) -> {(reader["UserId"] == DBNull.Value ? "NONE" : reader["UserId"].ToString()!.Substring(0,8))}...");
}
Console.WriteLine($"\nDone: {allAssignments.Count} assignments");
