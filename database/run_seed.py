#!/usr/bin/env python3
"""
IFMS Full Bulk Seeder — works with the ACTUAL live schema discovered in-container.
Databases that exist: IFMS_IdentityDB, IFMS_StationDB, IFMS_BookingDB
Creates:             IFMS_InventoryDB, IFMS_SalesDB  (with minimal schema + data)
"""

import pymssql, uuid, random, datetime

HOST, PORT, USER, PASSWD = "127.0.0.1", 1433, "sa", "Admin@12345"

def conn(db="master"):
    return pymssql.connect(HOST, USER, PASSWD, db, port=PORT, timeout=120, login_timeout=30)

def run(c, sql, args=None):
    cur = c.cursor()
    try:
        if args:
            cur.executemany(sql, args) if isinstance(args, list) else cur.execute(sql, args)
        else:
            cur.execute(sql)
        c.commit()
        return cur
    except Exception as e:
        c.rollback()
        raise e

def scalar(c, sql):
    cur = c.cursor(); cur.execute(sql); r = cur.fetchone(); return r[0] if r else 0

def header(title):
    print(f"\n{'='*60}\n  {title}\n{'='*60}")

# ─── BCrypt hash for "Pass@1234" (pre-computed, cost-10) ────────────────────
PWD = "$2a$10$N9qo8uLOickgx2ZMRZoMyeIjZAgcfl7p92ldGxad68LJZdL17lkii"

CUSTOMER_NAMES = [
    ("Aditya Sharma","customer001@ifms.in","9800000001"),
    ("Bhavna Patel","customer002@ifms.in","9800000002"),
    ("Chetan Mehta","customer003@ifms.in","9800000003"),
    ("Divya Nair","customer004@ifms.in","9800000004"),
    ("Eshan Rao","customer005@ifms.in","9800000005"),
    ("Falguni Desai","customer006@ifms.in","9800000006"),
    ("Gaurav Singh","customer007@ifms.in","9800000007"),
    ("Harini Krishnan","customer008@ifms.in","9800000008"),
    ("Ishaan Verma","customer009@ifms.in","9800000009"),
    ("Jaya Iyer","customer010@ifms.in","9800000010"),
    ("Kiran Malhotra","customer011@ifms.in","9800000011"),
    ("Lavanya Reddy","customer012@ifms.in","9800000012"),
    ("Manish Gupta","customer013@ifms.in","9800000013"),
    ("Nalini Joshi","customer014@ifms.in","9800000014"),
    ("Om Prakash","customer015@ifms.in","9800000015"),
    ("Priya Saxena","customer016@ifms.in","9800000016"),
    ("Qasim Khan","customer017@ifms.in","9800000017"),
    ("Rashmi Tiwari","customer018@ifms.in","9800000018"),
    ("Siddharth Pillai","customer019@ifms.in","9800000019"),
    ("Tanvi Bhatt","customer020@ifms.in","9800000020"),
    ("Uday Kumar","customer021@ifms.in","9800000021"),
    ("Vandana Mishra","customer022@ifms.in","9800000022"),
    ("Waqar Ahmed","customer023@ifms.in","9800000023"),
    ("Yashraj Thakur","customer024@ifms.in","9800000024"),
    ("Zara Siddiqui","customer025@ifms.in","9800000025"),
    ("Arjun Bose","customer026@ifms.in","9800000026"),
    ("Bhumika Kaur","customer027@ifms.in","9800000027"),
    ("Chirag Menon","customer028@ifms.in","9800000028"),
    ("Deepal Shah","customer029@ifms.in","9800000029"),
    ("Esha Pandey","customer030@ifms.in","9800000030"),
    ("Farhan Mirza","customer031@ifms.in","9800000031"),
    ("Gargi Chatterjee","customer032@ifms.in","9800000032"),
    ("Hemant Shukla","customer033@ifms.in","9800000033"),
    ("Isha Kapoor","customer034@ifms.in","9800000034"),
    ("Jai Prakash","customer035@ifms.in","9800000035"),
    ("Kavya Nambiar","customer036@ifms.in","9800000036"),
    ("Lalit Dubey","customer037@ifms.in","9800000037"),
    ("Meena Pillai","customer038@ifms.in","9800000038"),
    ("Nikhil Agarwal","customer039@ifms.in","9800000039"),
    ("Ojasvi Bajaj","customer040@ifms.in","9800000040"),
    ("Pallavi Ghosh","customer041@ifms.in","9800000041"),
    ("Rahul Anand","customer042@ifms.in","9800000042"),
    ("Shruti Kulkarni","customer043@ifms.in","9800000043"),
    ("Tarun Soni","customer044@ifms.in","9800000044"),
    ("Umesh Patil","customer045@ifms.in","9800000045"),
    ("Veena Rajan","customer046@ifms.in","9800000046"),
    ("Wasim Qureshi","customer047@ifms.in","9800000047"),
    ("Yogesh Tomar","customer048@ifms.in","9800000048"),
    ("Zoya Begum","customer049@ifms.in","9800000049"),
    ("Preeti Singh","customer050@ifms.in","9800000050"),
]

DEALER_NAMES = [
    ("Dealer Mumbai","dealer001@ifms.in","9900000001"),
    ("Dealer Bengaluru","dealer002@ifms.in","9900000002"),
    ("Dealer New Delhi","dealer003@ifms.in","9900000003"),
    ("Dealer Hyderabad","dealer004@ifms.in","9900000004"),
    ("Dealer Ahmedabad","dealer005@ifms.in","9900000005"),
    ("Dealer Chennai","dealer006@ifms.in","9900000006"),
    ("Dealer Pune","dealer007@ifms.in","9900000007"),
    ("Dealer Kolkata","dealer008@ifms.in","9900000008"),
    ("Dealer Jaipur","dealer009@ifms.in","9900000009"),
    ("Dealer Lucknow","dealer010@ifms.in","9900000010"),
]

# ─── SECTION 1: Users ───────────────────────────────────────────────────────
def seed_users():
    header("SECTION 1 · Identity DB — Users")
    c = conn("IFMS_IdentityDB")
    existing_emails = set()
    cur = c.cursor()
    cur.execute("SELECT Email FROM Users")
    for r in cur.fetchall(): existing_emails.add(r[0])

    inserted = 0
    base = datetime.datetime(2026, 1, 2, 8, 0, 0)
    for i, (name, email, phone) in enumerate(CUSTOMER_NAMES):
        if email in existing_emails:
            continue
        uid = str(uuid.uuid4())
        created = base + datetime.timedelta(days=i, hours=i % 8)
        run(c, """INSERT INTO Users (Id,FullName,Email,PhoneNumber,PasswordHash,Role,IsActive,CreatedAt)
                  VALUES (%s,%s,%s,%s,%s,'Customer',1,%s)""",
            (uid, name, email, phone, PWD, created))
        inserted += 1

    for i, (name, email, phone) in enumerate(DEALER_NAMES):
        if email in existing_emails:
            continue
        uid = str(uuid.uuid4())
        created = datetime.datetime(2026, 1, 1, 6, i*10, 0)
        run(c, """INSERT INTO Users (Id,FullName,Email,PhoneNumber,PasswordHash,Role,IsActive,CreatedAt)
                  VALUES (%s,%s,%s,%s,%s,'Dealer',1,%s)""",
            (uid, name, email, phone, PWD, created))
        inserted += 1

    total = scalar(c, "SELECT COUNT(*) FROM Users")
    print(f"  ✓ Inserted {inserted} new users. Total users: {total}")
    c.close()

# ─── SECTION 2: Extra Stations ──────────────────────────────────────────────
EXTRA_STATIONS = [
    ("Marina Fuel Hub","IND-LIC-006","Chennai","Tamil Nadu",13.0827,80.2707),
    ("Deccan Energy Point","IND-LIC-007","Pune","Maharashtra",18.5204,73.8567),
    ("Hooghly River Fuels","IND-LIC-008","Kolkata","West Bengal",22.5726,88.3639),
    ("Pink City Petroleum","IND-LIC-009","Jaipur","Rajasthan",26.9124,75.7873),
    ("Nawabi Fuel Station","IND-LIC-010","Lucknow","Uttar Pradesh",26.8469,80.9462),
]

def seed_stations():
    header("SECTION 2 · Station DB — Extra Stations + Dealer Assignments")
    c = conn("IFMS_StationDB")
    cur = c.cursor()
    cur.execute("SELECT LicenseNumber FROM Stations")
    existing_lic = {r[0] for r in cur.fetchall()}

    inserted = 0
    station_ids = []
    now = datetime.datetime(2026, 1, 1, 0, 0, 0)
    for name, lic, city, state, lat, lng in EXTRA_STATIONS:
        if lic in existing_lic:
            cur2 = c.cursor()
            cur2.execute("SELECT Id FROM Stations WHERE LicenseNumber=%s", (lic,))
            r = cur2.fetchone()
            if r: station_ids.append(str(r[0]))
            continue
        uid = str(uuid.uuid4())
        station_ids.append(uid)
        run(c, """INSERT INTO Stations (Id,Name,LicenseNumber,City,State,Latitude,Longitude,IsActive,CreatedAt,UpdatedAt)
                  VALUES (%s,%s,%s,%s,%s,%s,%s,1,%s,%s)""",
            (uid, name, lic, city, state, lat, lng, now, now))
        inserted += 1

    total = scalar(c, "SELECT COUNT(*) FROM Stations")
    print(f"  ✓ Inserted {inserted} new stations. Total: {total}")

    # Dealer assignments — link dealers to stations
    # DealerAssignments has UNIQUE on StationId: one dealer per station
    ic = conn("IFMS_IdentityDB")
    dcur = ic.cursor()
    dcur.execute("SELECT Id FROM Users WHERE Role='Dealer' ORDER BY CreatedAt")
    dealer_ids = [str(r[0]) for r in dcur.fetchall()]
    ic.close()

    scur = c.cursor()
    scur.execute("SELECT Id FROM Stations ORDER BY CreatedAt")
    all_station_ids = [str(r[0]) for r in scur.fetchall()]

    # Check which stations already have assignments
    ecur = c.cursor()
    ecur.execute("SELECT StationId, UserId FROM DealerAssignments")
    existing_assignments = {str(r[0]): str(r[1]) for r in ecur.fetchall()}
    assigned_dealer_ids = set(existing_assignments.values())

    assigned = 0
    for i, dealer_id in enumerate(dealer_ids):
        if dealer_id in assigned_dealer_ids:
            continue  # dealer already assigned somewhere
        station_id = all_station_ids[i % len(all_station_ids)]
        if station_id in existing_assignments:
            continue  # station already has a dealer
        try:
            run(c, "INSERT INTO DealerAssignments (Id,StationId,UserId,AssignedAt) VALUES (%s,%s,%s,%s)",
                (str(uuid.uuid4()), station_id, dealer_id, datetime.datetime(2026, 1, 1, 8, 0, 0)))
            existing_assignments[station_id] = dealer_id
            assigned_dealer_ids.add(dealer_id)
            assigned += 1
        except Exception:
            pass  # skip any remaining constraint conflicts

    total_assign = scalar(c, "SELECT COUNT(*) FROM DealerAssignments")
    print(f"  ✓ Assigned {assigned} new dealers. Total assignments: {total_assign}")
    c.close()

# ─── SECTION 3: Create IFMS_InventoryDB + seed full stock ──────────────────
FUEL_CONFIGS = [
    # (FuelType, MaxQuantity, PricePerUnit)
    ("Petrol",   10000.00, 102.50),
    ("Diesel",   15000.00,  89.75),
    ("CNG",       5000.00,  75.00),
    ("Electric",  2000.00,   8.50),
]

def seed_inventory():
    header("SECTION 3 · Inventory DB — Create + Full Stock Fill")
    mc = conn("master")

    # Create DB if not exists
    try:
        mc.autocommit(True)
        cur_mc = mc.cursor()
        cur_mc.execute("IF DB_ID('IFMS_InventoryDB') IS NULL CREATE DATABASE IFMS_InventoryDB")
        print("  ✓ IFMS_InventoryDB ensured")
    except Exception as e:
        print(f"  · InventoryDB note: {e}")
    mc.close()

    c = conn("IFMS_InventoryDB")

    # Create FuelStocks table
    run(c, """
        IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='FuelStocks')
        CREATE TABLE FuelStocks (
            Id            UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
            StationId     UNIQUEIDENTIFIER NOT NULL,
            FuelType      NVARCHAR(20)     NOT NULL,
            Quantity      DECIMAL(12,2)    NOT NULL DEFAULT 0,
            PricePerUnit  DECIMAL(10,2)    NOT NULL,
            Status        NVARCHAR(20)     NOT NULL DEFAULT 'Available',
            LastUpdated   DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
            CONSTRAINT UQ_FuelStocks_StationFuel UNIQUE (StationId, FuelType)
        )
    """)

    # Create StockMovements table
    run(c, """
        IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='StockMovements')
        CREATE TABLE StockMovements (
            Id               UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
            StationId        UNIQUEIDENTIFIER NOT NULL,
            FuelType         NVARCHAR(20)     NOT NULL,
            MovementType     NVARCHAR(20)     NOT NULL,
            Quantity         DECIMAL(12,2)    NOT NULL,
            PreviousQuantity DECIMAL(12,2)    NOT NULL,
            NewQuantity      DECIMAL(12,2)    NOT NULL,
            Reason           NVARCHAR(500)    NULL,
            ReferenceId      NVARCHAR(100)    NULL,
            CreatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE()
        )
    """)
    print("  ✓ FuelStocks + StockMovements tables ready")

    # Get all active stations
    sc = conn("IFMS_StationDB")
    scur = sc.cursor()
    scur.execute("SELECT Id FROM Stations WHERE IsActive=1")
    station_ids = [str(r[0]) for r in scur.fetchall()]
    sc.close()

    # UPSERT full stock for every station × fuel type
    upserted = 0
    now = datetime.datetime.utcnow()
    for sid in station_ids:
        for ft, max_qty, price in FUEL_CONFIGS:
            cur = c.cursor()
            cur.execute("SELECT Id, Quantity FROM FuelStocks WHERE StationId=%s AND FuelType=%s", (sid, ft))
            existing = cur.fetchone()
            if existing:
                if existing[1] < max_qty:
                    run(c, """UPDATE FuelStocks SET Quantity=%s, PricePerUnit=%s, Status='Available', LastUpdated=%s
                              WHERE StationId=%s AND FuelType=%s""",
                        (max_qty, price, now, sid, ft))
                    upserted += 1
            else:
                run(c, """INSERT INTO FuelStocks (Id,StationId,FuelType,Quantity,PricePerUnit,Status,LastUpdated)
                          VALUES (%s,%s,%s,%s,%s,'Available',%s)""",
                    (str(uuid.uuid4()), sid, ft, max_qty, price, now))
                upserted += 1

    total_stocks = scalar(c, "SELECT COUNT(*) FROM FuelStocks")
    print(f"  ✓ Upserted {upserted} fuel stock records. Total FuelStocks: {total_stocks}")

    # Stock Movements — one initial refill per stock
    existing_movements = scalar(c, "SELECT COUNT(*) FROM StockMovements")
    if existing_movements == 0:
        mcur = c.cursor()
        mcur.execute("SELECT Id, StationId, FuelType, Quantity FROM FuelStocks")
        stocks = mcur.fetchall()
        mov_count = 0
        for row in stocks:
            stock_id, sid, ft, qty = str(row[0]), str(row[1]), row[2], row[3]
            run(c, """INSERT INTO StockMovements
                        (Id,StationId,FuelType,MovementType,Quantity,PreviousQuantity,NewQuantity,Reason,ReferenceId,CreatedAt)
                      VALUES (%s,%s,%s,'Purchase',%s,0,%s,%s,%s,%s)""",
                (str(uuid.uuid4()), sid, ft, qty, qty,
                 f"Initial full refill – {ft}",
                 f"REFILL-{sid[:8]}-{ft}",
                 datetime.datetime(2026, 1, 1, 0, 0, 0)))
            mov_count += 1

        print(f"  ✓ Inserted {mov_count} initial StockMovement records")
    else:
        print(f"  · StockMovements already seeded ({existing_movements} rows)")

    # Print inventory summary
    cur2 = c.cursor()
    cur2.execute("""
        SELECT FuelType, COUNT(*) AS Stations, SUM(Quantity) AS TotalQty, AVG(Quantity) AS AvgQty
        FROM FuelStocks GROUP BY FuelType ORDER BY FuelType
    """)
    print(f"\n  {'FuelType':<12} {'Stations':>8} {'TotalQty':>12} {'AvgQty':>10}")
    print(f"  {'-'*12} {'-'*8} {'-'*12} {'-'*10}")
    for r in cur2.fetchall():
        print(f"  {r[0]:<12} {r[1]:>8} {r[2]:>12.2f} {r[3]:>10.2f}")

    c.close()

# ─── SECTION 4: Create IFMS_SalesDB + seed transactions ────────────────────
FUEL_PRICES = {"Petrol": 102.50, "Diesel": 89.75, "CNG": 75.00}
PAYMENT_METHODS = ["Cash","Card","UPI","Wallet","Token"]
TXN_STATUSES = ["Completed"] * 16 + ["Failed"] + ["Refunded"]  # ~90% completed

def seed_sales():
    header("SECTION 4 · Sales DB — Create + 130 Transactions")
    mc = conn("master")
    try:
        mc.autocommit(True)
        cur_mc = mc.cursor()
        cur_mc.execute("IF DB_ID('IFMS_SalesDB') IS NULL CREATE DATABASE IFMS_SalesDB")
        print("  ✓ IFMS_SalesDB ensured")
    except Exception as e:
        print(f"  · SalesDB note: {e}")
    mc.close()

    c = conn("IFMS_SalesDB")

    # Create Transactions table
    run(c, """
        IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='Transactions')
        CREATE TABLE Transactions (
            Id              UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
            StationId       UNIQUEIDENTIFIER NOT NULL,
            FuelType        NVARCHAR(20)     NOT NULL,
            Quantity        DECIMAL(10,2)    NOT NULL,
            PricePerLitre   DECIMAL(10,2)    NOT NULL,
            TotalAmount     DECIMAL(12,2)    NOT NULL,
            PaymentMethod   NVARCHAR(50)     NOT NULL,
            Status          NVARCHAR(20)     NOT NULL DEFAULT 'Completed',
            TransactionDate DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
            CustomerName    NVARCHAR(200)    NULL,
            CustomerId      UNIQUEIDENTIFIER NULL,
            BookingId       UNIQUEIDENTIFIER NULL,
            TokenCode       NVARCHAR(20)     NULL,
            ReferenceNumber NVARCHAR(100)    NULL,
            Notes           NVARCHAR(500)    NULL
        )
    """)

    # Create DailySalesSummary table
    run(c, """
        IF NOT EXISTS (SELECT 1 FROM sys.tables WHERE name='DailySalesSummary')
        CREATE TABLE DailySalesSummary (
            Id               UNIQUEIDENTIFIER PRIMARY KEY DEFAULT NEWID(),
            StationId        UNIQUEIDENTIFIER NOT NULL,
            SaleDate         DATE             NOT NULL,
            FuelType         NVARCHAR(20)     NOT NULL,
            TotalQuantity    DECIMAL(12,2)    NOT NULL DEFAULT 0,
            TotalRevenue     DECIMAL(15,2)    NOT NULL DEFAULT 0,
            TransactionCount INT              NOT NULL DEFAULT 0,
            CreatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
            UpdatedAt        DATETIME2        NOT NULL DEFAULT GETUTCDATE(),
            CONSTRAINT UQ_DailySalesSummary UNIQUE (StationId, SaleDate, FuelType)
        )
    """)
    print("  ✓ Transactions + DailySalesSummary tables ready")

    # Fetch stations + customers
    sc = conn("IFMS_StationDB")
    scur = sc.cursor(); scur.execute("SELECT Id FROM Stations WHERE IsActive=1")
    station_ids = [str(r[0]) for r in scur.fetchall()]
    sc.close()

    ic = conn("IFMS_IdentityDB")
    icur = ic.cursor(); icur.execute("SELECT Id, FullName FROM Users WHERE Role='Customer'")
    customers = [(str(r[0]), r[1]) for r in icur.fetchall()]
    ic.close()

    existing_txns = scalar(c, "SELECT COUNT(*) FROM Transactions WHERE Notes='Bulk seeded transaction'")
    if existing_txns >= 130:
        print(f"  · Transactions already seeded ({existing_txns} rows). Skipping.")
    else:
        inserted = 0
        fuel_types = list(FUEL_PRICES.keys())
        base_date = datetime.datetime.utcnow() - datetime.timedelta(days=90)

        for n in range(1, 131):
            sid = station_ids[n % len(station_ids)]
            cust_id, cust_name = customers[n % len(customers)]
            ft = fuel_types[n % len(fuel_types)]
            qty = round(5.0 + (n % 40) + (n % 6) * 0.25, 2)
            price = FUEL_PRICES[ft] + round((n % 10) * 0.05, 2)
            total = round(qty * price, 2)
            pm = PAYMENT_METHODS[n % len(PAYMENT_METHODS)]
            status = TXN_STATUSES[n % len(TXN_STATUSES)]
            txn_date = base_date + datetime.timedelta(days=(n % 90), minutes=(n * 17) % 1440)
            ref = f"TXN-{n:05d}"

            run(c, """INSERT INTO Transactions
                        (Id,StationId,FuelType,Quantity,PricePerLitre,TotalAmount,
                         PaymentMethod,Status,TransactionDate,CustomerName,CustomerId,ReferenceNumber,Notes)
                      VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,'Bulk seeded transaction')""",
                (str(uuid.uuid4()), sid, ft, qty, price, total,
                 pm, status, txn_date, cust_name, cust_id, ref))
            inserted += 1

        print(f"  ✓ Inserted {inserted} transactions")

    # Refresh DailySalesSummary
    cur3 = c.cursor()
    cur3.execute("""
        SELECT StationId, CAST(TransactionDate AS DATE), FuelType,
               SUM(Quantity), SUM(TotalAmount), COUNT(*)
        FROM Transactions
        GROUP BY StationId, CAST(TransactionDate AS DATE), FuelType
    """)
    daily_rows = cur3.fetchall()
    summary_inserted = 0
    for row in daily_rows:
        sid_val, sale_date, ft, tot_qty, tot_rev, cnt = str(row[0]), row[1], row[2], row[3], row[4], row[5]
        try:
            run(c, """
                MERGE INTO DailySalesSummary AS tgt
                USING (SELECT %s AS StationId, %s AS SaleDate, %s AS FuelType) AS src
                   ON tgt.StationId=src.StationId AND tgt.SaleDate=src.SaleDate AND tgt.FuelType=src.FuelType
                WHEN MATCHED THEN
                    UPDATE SET TotalQuantity=%s, TotalRevenue=%s, TransactionCount=%s, UpdatedAt=GETUTCDATE()
                WHEN NOT MATCHED THEN
                    INSERT (Id,StationId,SaleDate,FuelType,TotalQuantity,TotalRevenue,TransactionCount,CreatedAt,UpdatedAt)
                    VALUES (NEWID(),%s,%s,%s,%s,%s,%s,GETUTCDATE(),GETUTCDATE());
            """, (sid_val, sale_date, ft, tot_qty, tot_rev, cnt, sid_val, sale_date, ft, tot_qty, tot_rev, cnt))
            summary_inserted += 1
        except Exception as e:
            pass  # skip duplicate conflicts

    total_summary = scalar(c, "SELECT COUNT(*) FROM DailySalesSummary")
    print(f"  ✓ DailySalesSummary refreshed. Total rows: {total_summary}")
    c.close()

# ─── SECTION 5: Bookings ────────────────────────────────────────────────────
TOKEN_STATUSES = ["PENDING"] * 6 + ["USED"] * 2 + ["EXPIRED"] + ["CANCELLED"]

def seed_bookings():
    header("SECTION 5 · Booking DB — 130 Bookings")
    c = conn("IFMS_BookingDB")

    existing = scalar(c, "SELECT COUNT(*) FROM Bookings WHERE PaymentId LIKE 'PAY-SEED-%'")
    if existing >= 130:
        print(f"  · Bookings already seeded ({existing} rows). Skipping.")
        c.close()
        return

    # Fetch stations + customers
    sc = conn("IFMS_StationDB")
    scur = sc.cursor(); scur.execute("SELECT Id FROM Stations WHERE IsActive=1")
    station_ids = [str(r[0]) for r in scur.fetchall()]
    sc.close()

    ic = conn("IFMS_IdentityDB")
    icur = ic.cursor(); icur.execute("SELECT Id FROM Users WHERE Role='Customer'")
    customer_ids = [str(r[0]) for r in icur.fetchall()]
    ic.close()

    fuel_types = ["Petrol", "Diesel", "CNG"]
    fuel_prices = {"Petrol": 102.50, "Diesel": 89.75, "CNG": 75.00}
    base_date = datetime.datetime.utcnow() - datetime.timedelta(days=60)

    inserted = 0
    for n in range(1, 131):
        sid = station_ids[n % len(station_ids)]
        cid = customer_ids[n % len(customer_ids)]
        ft = fuel_types[n % len(fuel_types)]
        qty = round(5.0 + (n % 35) + (n % 5) * 0.5, 2)
        total = round(qty * fuel_prices[ft], 2)
        # TokenCode max 13 chars: "TK-{n:08d}" = 11 chars ✓
        token = f"TK-{n:08d}"
        status = TOKEN_STATUSES[n % len(TOKEN_STATUSES)]
        pay_id = f"PAY-SEED-{n:05d}"
        booked_at = base_date + datetime.timedelta(days=(n % 60), minutes=(n * 23) % 1440)
        expires_at = booked_at + datetime.timedelta(hours=24)
        used_at = (booked_at + datetime.timedelta(hours=2)) if status == "USED" else None

        try:
            run(c, """INSERT INTO Bookings
                        (BookingId,CustomerId,StationId,FuelType,QuantityLiters,TotalPaid,
                         TokenCode,TokenStatus,PaymentId,BookedAt,ExpiresAt,UsedAt)
                      VALUES (%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s,%s)""",
                (str(uuid.uuid4()), cid, sid, ft, qty, total,
                 token, status, pay_id, booked_at, expires_at, used_at))
            inserted += 1
        except Exception as e:
            pass  # skip duplicates on re-run

    total_bookings = scalar(c, "SELECT COUNT(*) FROM Bookings")
    print(f"  ✓ Inserted {inserted} bookings. Total Bookings: {total_bookings}")
    c.close()

# ─── VERIFICATION ────────────────────────────────────────────────────────────
def verify():
    header("VERIFICATION — Final Row Counts")
    checks = [
        ("IFMS_IdentityDB",  "Users",                     "SELECT COUNT(*) FROM Users"),
        ("IFMS_IdentityDB",  "  → Customers",             "SELECT COUNT(*) FROM Users WHERE Role='Customer'"),
        ("IFMS_IdentityDB",  "  → Dealers",               "SELECT COUNT(*) FROM Users WHERE Role='Dealer'"),
        ("IFMS_IdentityDB",  "  → Admins",                "SELECT COUNT(*) FROM Users WHERE Role='Admin'"),
        ("IFMS_StationDB",   "Stations",                   "SELECT COUNT(*) FROM Stations"),
        ("IFMS_StationDB",   "DealerAssignments",          "SELECT COUNT(*) FROM DealerAssignments"),
        ("IFMS_InventoryDB", "FuelStocks",                 "SELECT COUNT(*) FROM FuelStocks"),
        ("IFMS_InventoryDB", "  → Fully stocked",         "SELECT COUNT(*) FROM FuelStocks WHERE Status='Available'"),
        ("IFMS_InventoryDB", "StockMovements",             "SELECT COUNT(*) FROM StockMovements"),
        ("IFMS_BookingDB",   "Bookings",                   "SELECT COUNT(*) FROM Bookings"),
        ("IFMS_BookingDB",   "  → PENDING",               "SELECT COUNT(*) FROM Bookings WHERE TokenStatus='PENDING'"),
        ("IFMS_BookingDB",   "  → USED",                  "SELECT COUNT(*) FROM Bookings WHERE TokenStatus='USED'"),
        ("IFMS_SalesDB",     "Transactions",               "SELECT COUNT(*) FROM Transactions"),
        ("IFMS_SalesDB",     "  → Completed",             "SELECT COUNT(*) FROM Transactions WHERE Status='Completed'"),
        ("IFMS_SalesDB",     "DailySalesSummary",          "SELECT COUNT(*) FROM DailySalesSummary"),
    ]

    cur_db = None; c = None
    print(f"\n  {'Database':<22} {'Table':<28} {'Rows':>6}")
    print(f"  {'-'*22} {'-'*28} {'-'*6}")
    for db, label, sql in checks:
        if db != cur_db:
            if c: c.close()
            c = conn(db); cur_db = db
        count = scalar(c, sql)
        print(f"  {db:<22} {label:<28} {count:>6}")
    if c: c.close()

    # Inventory detail
    ic = conn("IFMS_InventoryDB")
    cur = ic.cursor()
    cur.execute("""
        SELECT FuelType, COUNT(*) Stations, SUM(Quantity) TotalStock, Status
        FROM FuelStocks GROUP BY FuelType, Status ORDER BY FuelType
    """)
    print(f"\n  {'FuelType':<10} {'Status':<12} {'Stations':>8} {'TotalStock':>12}")
    print(f"  {'-'*10} {'-'*12} {'-'*8} {'-'*12}")
    for r in cur.fetchall():
        print(f"  {r[0]:<10} {r[3]:<12} {r[1]:>8} {r[2]:>12.2f}")
    ic.close()

    print(f"\n{'='*60}")
    print("  ✅  ALL DONE — IFMS database fully seeded!")
    print(f"{'='*60}\n")

# ─── MAIN ────────────────────────────────────────────────────────────────────
if __name__ == "__main__":
    seed_users()
    seed_stations()
    seed_inventory()
    seed_sales()
    seed_bookings()
    verify()
