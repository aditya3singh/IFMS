# =============================================
# Bharat Kinetic IFMS - Database Migration Runner (PowerShell)
# Version: 1.0.0
# Description: Executes all database migration scripts in order
# =============================================

param(
    [string]$Server = "localhost,1433",
    [string]$Username = "sa",
    [string]$Password = "Admin@12345",
    [switch]$UpdateOnly,
    [switch]$SeedOnly
)

$ErrorActionPreference = "Stop"

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Bharat Kinetic IFMS - Database Migration" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# Check if sqlcmd is available
try {
    $null = Get-Command sqlcmd -ErrorAction Stop
} catch {
    Write-Host "ERROR: sqlcmd is not installed or not in PATH" -ForegroundColor Red
    Write-Host "Please install SQL Server Command Line Tools" -ForegroundColor Yellow
    exit 1
}

# Function to execute SQL script
function Execute-SqlScript {
    param(
        [string]$ScriptPath,
        [string]$Description
    )
    
    Write-Host "Executing: $Description" -ForegroundColor Yellow
    Write-Host "Script: $ScriptPath" -ForegroundColor Gray
    
    try {
        sqlcmd -S $Server -U $Username -P $Password -i $ScriptPath -b
        if ($LASTEXITCODE -eq 0) {
            Write-Host "✓ Success: $Description" -ForegroundColor Green
            Write-Host ""
        } else {
            throw "Script execution failed with exit code $LASTEXITCODE"
        }
    } catch {
        Write-Host "✗ Failed: $Description" -ForegroundColor Red
        Write-Host "Error: $_" -ForegroundColor Red
        exit 1
    }
}

# Get script directory
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path

# Test connection
Write-Host "Testing database connection..." -ForegroundColor Yellow
try {
    sqlcmd -S $Server -U $Username -P $Password -Q "SELECT @@VERSION" -b | Out-Null
    Write-Host "✓ Database connection successful" -ForegroundColor Green
    Write-Host ""
} catch {
    Write-Host "✗ Failed to connect to database" -ForegroundColor Red
    Write-Host "Server: $Server" -ForegroundColor Gray
    Write-Host "Username: $Username" -ForegroundColor Gray
    exit 1
}

# Execute migration scripts
if (-not $UpdateOnly -and -not $SeedOnly) {
    Write-Host "Running FULL MIGRATION (Create + Seed)..." -ForegroundColor Cyan
    Write-Host ""
    
    Execute-SqlScript "$ScriptDir/01-create-databases.sql" "Create Databases"
    Execute-SqlScript "$ScriptDir/02-identity-schema.sql" "Identity Schema"
    Execute-SqlScript "$ScriptDir/03-station-schema.sql" "Station Schema"
    Execute-SqlScript "$ScriptDir/04-inventory-schema.sql" "Inventory Schema"
    Execute-SqlScript "$ScriptDir/05-sales-schema.sql" "Sales Schema"
    Execute-SqlScript "$ScriptDir/06-booking-schema.sql" "Booking Schema"
    Execute-SqlScript "$ScriptDir/07-seed-data.sql" "Seed Data"
}
elseif ($UpdateOnly) {
    Write-Host "Running UPDATE ONLY..." -ForegroundColor Cyan
    Write-Host ""
    
    Execute-SqlScript "$ScriptDir/08-update-existing.sql" "Update Existing Databases"
}
elseif ($SeedOnly) {
    Write-Host "Running SEED DATA ONLY..." -ForegroundColor Cyan
    Write-Host ""
    
    Execute-SqlScript "$ScriptDir/07-seed-data.sql" "Seed Data"
}

Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Migration completed successfully!" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""
Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "1. Verify databases in SQL Server Management Studio" -ForegroundColor Gray
Write-Host "2. Update connection strings in appsettings.json" -ForegroundColor Gray
Write-Host "3. Run Entity Framework migrations if needed" -ForegroundColor Gray
Write-Host ""
