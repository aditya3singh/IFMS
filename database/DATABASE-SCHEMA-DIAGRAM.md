# IFMS Database Schema Diagram

## Database Overview

```
┌─────────────────────────────────────────────────────────────────┐
│                    IFMS Database Architecture                    │
│                     (5 Separate Databases)                       │
└─────────────────────────────────────────────────────────────────┘

┌──────────────────┐  ┌──────────────────┐  ┌──────────────────┐
│ IFMS_IdentityDB  │  │  IFMS_StationDB  │  │ IFMS_InventoryDB │
│                  │  │                  │  │                  │
│ • Users          │  │ • Stations       │  │ • FuelStocks     │
│ • Otps           │  │ • DealerAssign   │  │ • StockMovements │
│ • RefreshTokens  │  │ • StationPricing │  │ • FuelTypes      │
│ • UserSessions   │  │                  │  │                  │
│ • AuditLogs      │  │                  │  │                  │
└──────────────────┘  └──────────────────┘  └──────────────────┘

┌──────────────────┐  ┌──────────────────┐
│  IFMS_SalesDB    │  │  IFMS_BookingDB  │
│                  │  │                  │
│ • Transactions   │  │ • Bookings       │
│ • DailySales     │  │ • KycVerify      │
│ • PaymentMethods │  │ • vw_BookingHist │
└──────────────────┘  └──────────────────┘
```

## IFMS_IdentityDB Schema

```
┌─────────────────────────────────────────────────────────────┐
│                          Users                               │
├─────────────────────────────────────────────────────────────┤
│ PK  Id                    UNIQUEIDENTIFIER                   │
│     Email                 NVARCHAR(256) UNIQUE               │
│     PhoneNumber           NVARCHAR(20)                       │
│     PasswordHash          NVARCHAR(MAX)                      │
│     FirstName             NVARCHAR(100)                      │
│     LastName              NVARCHAR(100)                      │
│     Role                  NVARCHAR(50) [Customer/Dealer/Admin]│
│     IsActive              BIT                                │
│     IsEmailVerified       BIT                                │
│     IsPhoneVerified       BIT                                │
│     CreatedAt             DATETIME2                          │
│     UpdatedAt             DATETIME2                          │
│     LastLoginAt           DATETIME2                          │
│     ProfileImageUrl       NVARCHAR(500)                      │
│     CustomerId            NVARCHAR(50)                       │
│     DealerId              NVARCHAR(50)                       │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ 1:N
                              ▼
        ┌─────────────────────────────────────────┐
        │              Otps                        │
        ├─────────────────────────────────────────┤
        │ PK  Id              UNIQUEIDENTIFIER     │
        │ FK  UserId          UNIQUEIDENTIFIER     │
        │     OtpCode         NVARCHAR(10)         │
        │     Purpose         NVARCHAR(50)         │
        │     ExpiresAt       DATETIME2            │
        │     IsUsed          BIT                  │
        │     CreatedAt       DATETIME2            │
        │     UsedAt          DATETIME2            │
        └─────────────────────────────────────────┘

        ┌─────────────────────────────────────────┐
        │         RefreshTokens                    │
        ├─────────────────────────────────────────┤
        │ PK  Id              UNIQUEIDENTIFIER     │
        │ FK  UserId          UNIQUEIDENTIFIER     │
        │     Token           NVARCHAR(500) UNIQUE │
        │     ExpiresAt       DATETIME2            │
        │     CreatedAt       DATETIME2            │
        │     RevokedAt       DATETIME2            │
        │     IsRevoked       BIT                  │
        │     DeviceInfo      NVARCHAR(500)        │
        │     IpAddress       NVARCHAR(50)         │
        └─────────────────────────────────────────┘

        ┌─────────────────────────────────────────┐
        │         UserSessions                     │
        ├─────────────────────────────────────────┤
        │ PK  Id              UNIQUEIDENTIFIER     │
        │ FK  UserId          UNIQUEIDENTIFIER     │
        │     SessionToken    NVARCHAR(500) UNIQUE │
        │     DeviceType      NVARCHAR(50)         │
        │     DeviceName      NVARCHAR(200)        │
        │     Browser         NVARCHAR(100)        │
        │     IpAddress       NVARCHAR(50)         │
        │     Location        NVARCHAR(200)        │
        │     CreatedAt       DATETIME2            │
        │     LastActivityAt  DATETIME2            │
        │     ExpiresAt       DATETIME2            │
        │     IsActive        BIT                  │
        └─────────────────────────────────────────┘

        ┌─────────────────────────────────────────┐
        │          AuditLogs                       │
        ├─────────────────────────────────────────┤
        │ PK  Id              UNIQUEIDENTIFIER     │
        │ FK  UserId          UNIQUEIDENTIFIER     │
        │     Action          NVARCHAR(100)        │
        │     EntityType      NVARCHAR(100)        │
        │     EntityId        NVARCHAR(100)        │
        │     OldValues       NVARCHAR(MAX)        │
        │     NewValues       NVARCHAR(MAX)        │
        │     IpAddress       NVARCHAR(50)         │
        │     UserAgent       NVARCHAR(500)        │
        │     CreatedAt       DATETIME2            │
        └─────────────────────────────────────────┘
```

## IFMS_StationDB Schema

```
┌─────────────────────────────────────────────────────────────┐
│                        Stations                              │
├─────────────────────────────────────────────────────────────┤
│ PK  Id                    UNIQUEIDENTIFIER                   │
│     Name                  NVARCHAR(200)                      │
│     LicenseNumber         NVARCHAR(50) UNIQUE               │
│     City                  NVARCHAR(100)                      │
│     State                 NVARCHAR(100)                      │
│     Latitude              DECIMAL(10,7)                      │
│     Longitude             DECIMAL(10,7)                      │
│     IsActive              BIT                                │
│     CreatedAt             DATETIME2                          │
│     UpdatedAt             DATETIME2                          │
└─────────────────────────────────────────────────────────────┘
                              │
                              │ 1:1
                              ▼
        ┌─────────────────────────────────────────┐
        │       DealerAssignments                  │
        ├─────────────────────────────────────────┤
        │ PK  Id              UNIQUEIDENTIFIER     │
        │ FK  StationId       UNIQUEIDENTIFIER     │
        │     UserId          UNIQUEIDENTIFIER     │
        │     AssignedAt      DATETIME2            │
        └─────────────────────────────────────────┘

                              │ 1:N
                              ▼
        ┌─────────────────────────────────────────┐
        │        StationPricing                    │
        ├─────────────────────────────────────────┤
        │ PK  Id              UNIQUEIDENTIFIER     │
        │ FK  StationId       UNIQUEIDENTIFIER     │
        │     FuelType        NVARCHAR(20)         │
        │     PricePerLitre   DECIMAL(10,2)        │
        │     EffectiveFrom   DATETIME2            │
        │     EffectiveTo     DATETIME2            │
        │     IsActive        BIT                  │
        │     UpdatedAt       DATETIME2            │
        └─────────────────────────────────────────┘
```

## IFMS_InventoryDB Schema

```
┌─────────────────────────────────────────────────────────────┐
│                       FuelStocks                             │
├─────────────────────────────────────────────────────────────┤
│ PK  Id                    UNIQUEIDENTIFIER                   │
│     StationId             UNIQUEIDENTIFIER                   │
│     FuelType              NVARCHAR(20)                       │
│     Quantity              DECIMAL(12,2)                      │
│     PricePerLitre         DECIMAL(10,2)                      │
│     Status                NVARCHAR(20)                       │
│     LastUpdated           DATETIME2                          │
│ UQ  (StationId, FuelType)                                    │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    StockMovements                            │
├─────────────────────────────────────────────────────────────┤
│ PK  Id                    UNIQUEIDENTIFIER                   │
│     StationId             UNIQUEIDENTIFIER                   │
│     FuelType              NVARCHAR(20)                       │
│     MovementType          NVARCHAR(20)                       │
│     Quantity              DECIMAL(12,2)                      │
│     PreviousQuantity      DECIMAL(12,2)                      │
│     NewQuantity           DECIMAL(12,2)                      │
│     Reason                NVARCHAR(500)                      │
│     ReferenceId           NVARCHAR(100)                      │
│     CreatedBy             UNIQUEIDENTIFIER                   │
│     CreatedAt             DATETIME2                          │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                       FuelTypes                              │
├─────────────────────────────────────────────────────────────┤
│ PK  Id                    UNIQUEIDENTIFIER                   │
│     Name                  NVARCHAR(20) UNIQUE               │
│     DisplayName           NVARCHAR(50)                       │
│     Unit                  NVARCHAR(10)                       │
│     IsActive              BIT                                │
│     CreatedAt             DATETIME2                          │
└─────────────────────────────────────────────────────────────┘
```

## IFMS_SalesDB Schema

```
┌─────────────────────────────────────────────────────────────┐
│                      Transactions                            │
├─────────────────────────────────────────────────────────────┤
│ PK  Id                    UNIQUEIDENTIFIER                   │
│     StationId             UNIQUEIDENTIFIER                   │
│     FuelType              NVARCHAR(20)                       │
│     Quantity              DECIMAL(10,2)                      │
│     PricePerLitre         DECIMAL(10,2)                      │
│     TotalAmount           DECIMAL(12,2)                      │
│     PaymentMethod         NVARCHAR(50)                       │
│     Status                NVARCHAR(20)                       │
│     TransactionDate       DATETIME2                          │
│     CustomerName          NVARCHAR(200)                      │
│     CustomerId            UNIQUEIDENTIFIER                   │
│     BookingId             UNIQUEIDENTIFIER                   │
│     TokenCode             NVARCHAR(20)                       │
│     ReferenceNumber       NVARCHAR(100)                      │
│     Notes                 NVARCHAR(500)                      │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                   DailySalesSummary                          │
├─────────────────────────────────────────────────────────────┤
│ PK  Id                    UNIQUEIDENTIFIER                   │
│     StationId             UNIQUEIDENTIFIER                   │
│     SaleDate              DATE                               │
│     FuelType              NVARCHAR(20)                       │
│     TotalQuantity         DECIMAL(12,2)                      │
│     TotalRevenue          DECIMAL(15,2)                      │
│     TransactionCount      INT                                │
│     CreatedAt             DATETIME2                          │
│     UpdatedAt             DATETIME2                          │
│ UQ  (StationId, SaleDate, FuelType)                          │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                    PaymentMethods                            │
├─────────────────────────────────────────────────────────────┤
│ PK  Id                    UNIQUEIDENTIFIER                   │
│     Name                  NVARCHAR(50) UNIQUE               │
│     DisplayName           NVARCHAR(100)                      │
│     IsActive              BIT                                │
│     ProcessingFeePercent  DECIMAL(5,2)                       │
│     CreatedAt             DATETIME2                          │
└─────────────────────────────────────────────────────────────┘
```

## IFMS_BookingDB Schema

```
┌─────────────────────────────────────────────────────────────┐
│                        Bookings                              │
├─────────────────────────────────────────────────────────────┤
│ PK  BookingId             UNIQUEIDENTIFIER                   │
│     CustomerId            UNIQUEIDENTIFIER                   │
│     StationId             UNIQUEIDENTIFIER                   │
│     FuelType              NVARCHAR(20)                       │
│     QuantityLiters        DECIMAL(10,2)                      │
│     TotalPaid             DECIMAL(12,2)                      │
│     TokenCode             NVARCHAR(13) UNIQUE               │
│     TokenStatus           NVARCHAR(20)                       │
│     PaymentId             NVARCHAR(100)                      │
│     BookedAt              DATETIME2                          │
│     ExpiresAt             DATETIME2                          │
│     UsedAt                DATETIME2                          │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                   KycVerifications                           │
├─────────────────────────────────────────────────────────────┤
│ PK  Id                    UNIQUEIDENTIFIER                   │
│     CustomerId            UNIQUEIDENTIFIER                   │
│     DocumentType          NVARCHAR(20)                       │
│     DocumentNumber        NVARCHAR(50)                       │
│     VerificationStatus    NVARCHAR(20)                       │
│     VerifiedAt            DATETIME2                          │
│     ExpiresAt             DATETIME2                          │
│     RejectionReason       NVARCHAR(500)                      │
│     ProviderResponse      NVARCHAR(MAX)                      │
│     CreatedAt             DATETIME2                          │
│     UpdatedAt             DATETIME2                          │
│ UQ  (CustomerId, DocumentType)                               │
└─────────────────────────────────────────────────────────────┘

┌─────────────────────────────────────────────────────────────┐
│                  vw_BookingHistory (VIEW)                    │
├─────────────────────────────────────────────────────────────┤
│     BookingId             UNIQUEIDENTIFIER                   │
│     CustomerId            UNIQUEIDENTIFIER                   │
│     StationId             UNIQUEIDENTIFIER                   │
│     FuelType              NVARCHAR(20)                       │
│     QuantityLiters        DECIMAL(10,2)                      │
│     TotalPaid             DECIMAL(12,2)                      │
│     TokenCode             NVARCHAR(13)                       │
│     TokenStatus           NVARCHAR(20)                       │
│     PaymentId             NVARCHAR(100)                      │
│     BookedAt              DATETIME2                          │
│     ExpiresAt             DATETIME2                          │
│     UsedAt                DATETIME2                          │
│     DisplayStatus         COMPUTED                           │
│     HoursToUse            COMPUTED                           │
└─────────────────────────────────────────────────────────────┘
```

## Cross-Database Relationships

```
┌──────────────────────────────────────────────────────────────┐
│              Logical Relationships (No FK)                    │
└──────────────────────────────────────────────────────────────┘

Users (IdentityDB)
    │
    ├─→ CustomerId ──→ Bookings.CustomerId (BookingDB)
    │                  Transactions.CustomerId (SalesDB)
    │
    └─→ DealerId ───→ DealerAssignments.UserId (StationDB)

Stations (StationDB)
    │
    ├─→ StationId ──→ FuelStocks.StationId (InventoryDB)
    │                 Transactions.StationId (SalesDB)
    │                 Bookings.StationId (BookingDB)
    │
    └─→ Pricing ────→ StationPricing (StationDB)

Bookings (BookingDB)
    │
    └─→ BookingId ──→ Transactions.BookingId (SalesDB)
```

## Indexes Summary

### Performance Indexes
- All primary keys (clustered)
- Foreign keys (non-clustered)
- Frequently queried columns (Email, TokenCode, etc.)
- Date columns for time-based queries
- Composite indexes for location searches

### Unique Indexes
- Email (Users)
- LicenseNumber (Stations)
- TokenCode (Bookings)
- Token (RefreshTokens)
- SessionToken (UserSessions)

## Data Flow Example: Fuel Booking

```
1. Customer Login
   IdentityDB.Users → Generate JWT → Store in RefreshTokens

2. Browse Stations
   StationDB.Stations → Join StationPricing → Return available stations

3. Check Inventory
   InventoryDB.FuelStocks → Verify availability

4. Create Booking
   BookingDB.Bookings → Generate TokenCode → Store booking

5. Process Payment
   SalesDB.Transactions → Record payment → Link to BookingId

6. Update Inventory
   InventoryDB.FuelStocks → Reduce quantity
   InventoryDB.StockMovements → Log movement

7. Use Token at Station
   BookingDB.Bookings → Validate TokenCode → Mark as USED
   SalesDB.Transactions → Record fuel dispensed
```

## Constraints Summary

### Check Constraints
- Role IN ('Customer', 'Dealer', 'Admin')
- FuelType IN ('Petrol', 'Diesel', 'CNG', 'Electric')
- TokenStatus IN ('PENDING', 'USED', 'EXPIRED', 'CANCELLED')
- Latitude BETWEEN -90 AND 90
- Longitude BETWEEN -180 AND 180
- Quantity > 0
- PricePerLitre > 0

### Foreign Key Constraints
- Otps.UserId → Users.Id (CASCADE DELETE)
- RefreshTokens.UserId → Users.Id (CASCADE DELETE)
- UserSessions.UserId → Users.Id (CASCADE DELETE)
- AuditLogs.UserId → Users.Id (SET NULL)
- DealerAssignments.StationId → Stations.Id (CASCADE DELETE)
- StationPricing.StationId → Stations.Id (CASCADE DELETE)

---

**Legend:**
- PK = Primary Key
- FK = Foreign Key
- UQ = Unique Constraint
- → = Relationship (logical, no FK across databases)
