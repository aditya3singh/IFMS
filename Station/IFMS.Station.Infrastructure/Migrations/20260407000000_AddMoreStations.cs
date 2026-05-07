using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IFMS.Station.Infrastructure.Migrations;

/// <summary>
/// Add 10 more Indian fuel stations across major cities
/// Total stations: 15 (5 existing + 10 new)
/// </summary>
public partial class AddMoreStations : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            -- Station 6: Pune, Maharashtra
            INSERT INTO Stations (Id, Name, LicenseNumber, City, State, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt)
            VALUES ('66666666-6666-6666-6666-666666666666', N'Pune Highway Fuel Station', N'IND-LIC-006', N'Pune', N'Maharashtra', 18.520430, 73.856743, 1, '2026-04-07T00:00:00', '2026-04-07T00:00:00');

            -- Station 7: Chennai, Tamil Nadu
            INSERT INTO Stations (Id, Name, LicenseNumber, City, State, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt)
            VALUES ('77777777-7777-7777-7777-777777777777', N'Marina Beach Fuel Point', N'IND-LIC-007', N'Chennai', N'Tamil Nadu', 13.082680, 80.270721, 1, '2026-04-07T00:00:00', '2026-04-07T00:00:00');

            -- Station 8: Kolkata, West Bengal
            INSERT INTO Stations (Id, Name, LicenseNumber, City, State, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt)
            VALUES ('88888888-8888-8888-8888-888888888888', N'Howrah Bridge Energy Hub', N'IND-LIC-008', N'Kolkata', N'West Bengal', 22.572645, 88.363892, 1, '2026-04-07T00:00:00', '2026-04-07T00:00:00');

            -- Station 9: Jaipur, Rajasthan
            INSERT INTO Stations (Id, Name, LicenseNumber, City, State, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt)
            VALUES ('99999999-9999-9999-9999-999999999999', N'Pink City Fuel Station', N'IND-LIC-009', N'Jaipur', N'Rajasthan', 26.912434, 75.787270, 1, '2026-04-07T00:00:00', '2026-04-07T00:00:00');

            -- Station 10: Lucknow, Uttar Pradesh
            INSERT INTO Stations (Id, Name, LicenseNumber, City, State, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt)
            VALUES ('aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa', N'Gomti Nagar Fuel Point', N'IND-LIC-010', N'Lucknow', N'Uttar Pradesh', 26.846694, 80.946166, 1, '2026-04-07T00:00:00', '2026-04-07T00:00:00');

            -- Station 11: Chandigarh, Punjab
            INSERT INTO Stations (Id, Name, LicenseNumber, City, State, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt)
            VALUES ('bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb', N'Sector 17 Energy Station', N'IND-LIC-011', N'Chandigarh', N'Punjab', 30.733315, 76.779419, 1, '2026-04-07T00:00:00', '2026-04-07T00:00:00');

            -- Station 12: Kochi, Kerala
            INSERT INTO Stations (Id, Name, LicenseNumber, City, State, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt)
            VALUES ('cccccccc-cccc-cccc-cccc-cccccccccccc', N'Backwater Fuel Hub', N'IND-LIC-012', N'Kochi', N'Kerala', 9.931233, 76.267303, 1, '2026-04-07T00:00:00', '2026-04-07T00:00:00');

            -- Station 13: Indore, Madhya Pradesh
            INSERT INTO Stations (Id, Name, LicenseNumber, City, State, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt)
            VALUES ('dddddddd-dddd-dddd-dddd-dddddddddddd', N'Holkar Stadium Fuel Point', N'IND-LIC-013', N'Indore', N'Madhya Pradesh', 22.719568, 75.857727, 1, '2026-04-07T00:00:00', '2026-04-07T00:00:00');

            -- Station 14: Bhubaneswar, Odisha
            INSERT INTO Stations (Id, Name, LicenseNumber, City, State, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt)
            VALUES ('eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee', N'Temple City Energy Hub', N'IND-LIC-014', N'Bhubaneswar', N'Odisha', 20.296059, 85.824539, 1, '2026-04-07T00:00:00', '2026-04-07T00:00:00');

            -- Station 15: Patna, Bihar
            INSERT INTO Stations (Id, Name, LicenseNumber, City, State, Latitude, Longitude, IsActive, CreatedAt, UpdatedAt)
            VALUES ('ffffffff-ffff-ffff-ffff-ffffffffffff', N'Gandhi Maidan Fuel Station', N'IND-LIC-015', N'Patna', N'Bihar', 25.594095, 85.137566, 1, '2026-04-07T00:00:00', '2026-04-07T00:00:00');
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            DELETE FROM Stations WHERE Id IN (
                '66666666-6666-6666-6666-666666666666',
                '77777777-7777-7777-7777-777777777777',
                '88888888-8888-8888-8888-888888888888',
                '99999999-9999-9999-9999-999999999999',
                'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa',
                'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb',
                'cccccccc-cccc-cccc-cccc-cccccccccccc',
                'dddddddd-dddd-dddd-dddd-dddddddddddd',
                'eeeeeeee-eeee-eeee-eeee-eeeeeeeeeeee',
                'ffffffff-ffff-ffff-ffff-ffffffffffff'
            );
            """);
    }
}
