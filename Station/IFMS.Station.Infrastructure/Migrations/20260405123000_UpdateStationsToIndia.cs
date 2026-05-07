using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace IFMS.Station.Infrastructure.Migrations;

/// <summary>Indian outlets; state names match RegionalFuelPriceQuote lookup.</summary>
public partial class UpdateStationsToIndia : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE Stations SET Name=N'Western Express Fuel Point', LicenseNumber=N'IND-LIC-001', City=N'Mumbai', State=N'Maharashtra', Latitude=19.076000, Longitude=72.877700, UpdatedAt='2026-04-05T00:00:00' WHERE Id='11111111-1111-1111-1111-111111111111';
            UPDATE Stations SET Name=N'Silicon Corridor Pump', LicenseNumber=N'IND-LIC-002', City=N'Bengaluru', State=N'Karnataka', Latitude=12.971600, Longitude=77.594600, UpdatedAt='2026-04-05T00:00:00' WHERE Id='22222222-2222-2222-2222-222222222222';
            UPDATE Stations SET Name=N'NCR Central Energy', LicenseNumber=N'IND-LIC-003', City=N'New Delhi', State=N'Delhi', Latitude=28.613900, Longitude=77.209000, UpdatedAt='2026-04-05T00:00:00' WHERE Id='33333333-3333-3333-3333-333333333333';
            UPDATE Stations SET Name=N'HITEC City Fuels', LicenseNumber=N'IND-LIC-004', City=N'Hyderabad', State=N'Telangana', Latitude=17.385000, Longitude=78.486700, UpdatedAt='2026-04-05T00:00:00' WHERE Id='44444444-4444-4444-4444-444444444444';
            UPDATE Stations SET Name=N'Sabarmati Retail Outlet', LicenseNumber=N'IND-LIC-005', City=N'Ahmedabad', State=N'Gujarat', Latitude=23.022500, Longitude=72.571400, UpdatedAt='2026-04-05T00:00:00' WHERE Id='55555555-5555-5555-5555-555555555555';
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.Sql("""
            UPDATE Stations SET Name=N'Downtown Fuel Station', LicenseNumber=N'LIC-001', City=N'New York', State=N'NY', Latitude=40.712776, Longitude=-74.005974, UpdatedAt='2026-01-01T00:00:00' WHERE Id='11111111-1111-1111-1111-111111111111';
            UPDATE Stations SET Name=N'West Coast Energy Hub', LicenseNumber=N'LIC-002', City=N'Los Angeles', State=N'CA', Latitude=34.052235, Longitude=-118.243683, UpdatedAt='2026-01-01T00:00:00' WHERE Id='22222222-2222-2222-2222-222222222222';
            UPDATE Stations SET Name=N'Midwest Fuel Center', LicenseNumber=N'LIC-003', City=N'Chicago', State=N'IL', Latitude=41.878113, Longitude=-87.629799, UpdatedAt='2026-01-01T00:00:00' WHERE Id='33333333-3333-3333-3333-333333333333';
            UPDATE Stations SET Name=N'Southern Fuel Depot', LicenseNumber=N'LIC-004', City=N'Houston', State=N'TX', Latitude=29.760427, Longitude=-95.369804, UpdatedAt='2026-01-01T00:00:00' WHERE Id='44444444-4444-4444-4444-444444444444';
            UPDATE Stations SET Name=N'Pacific Northwest Station', LicenseNumber=N'LIC-005', City=N'Seattle', State=N'WA', Latitude=47.606209, Longitude=-122.332069, UpdatedAt='2026-01-01T00:00:00' WHERE Id='55555555-5555-5555-5555-555555555555';
            """);
    }
}
