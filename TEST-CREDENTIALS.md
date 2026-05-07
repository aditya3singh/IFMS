# Test Credentials - IFMS

All test accounts are now active and ready to use.

## Available Test Accounts

### 👨‍💼 Admin Account
```
Email: admin-test@ifms.com
Password: Admin@12345
Role: Admin
```
**Access:**
- Full system monitoring
- View all transactions across all stations
- Generate reports
- Station management
- Dealer assignments

**Login URL:** http://localhost:4200/login

---

### 🏪 Dealer Account
```
Email: dealer-test@ifms.com
Password: Dealer@12345
Role: Dealer
```
**Access:**
- Record offline sales
- Manage inventory for assigned station
- View station transactions
- Generate receipts

**Assigned Station:** Main Test Station (ID: 44444444-4444-4444-4444-444444444444)

**Login URL:** http://localhost:4200/login

---

### 🏪 Seeded Dealer Accounts (city-matched)

Each dealer is assigned to the station in their city. Password for all: `Pass@1234`

| Email | Dealer Name | Assigned Station | City |
|---|---|---|---|
| dealer001@ifms.in | Dealer Mumbai | Western Express Fuel Point | Mumbai |
| dealer002@ifms.in | Dealer Bengaluru | Silicon Corridor Pump | Bengaluru |
| dealer003@ifms.in | Dealer New Delhi | NCR Central Energy | New Delhi |
| dealer004@ifms.in | Dealer Hyderabad | HITEC City Fuels | Hyderabad |
| dealer005@ifms.in | Dealer Ahmedabad | Sabarmati Retail Outlet | Ahmedabad |
| dealer006@ifms.in | Dealer Chennai | Marina Fuel Hub | Chennai |
| dealer007@ifms.in | Dealer Pune | Deccan Energy Point | Pune |
| dealer008@ifms.in | Dealer Kolkata | Hooghly River Fuels | Kolkata |
| dealer009@ifms.in | Dealer Jaipur | Pink City Petroleum | Jaipur |
| dealer010@ifms.in | Dealer Lucknow | Nawabi Fuel Station | Lucknow |

> **Note:** If assignments are wrong, run `database/12-fix-dealer-assignments.sql` to reset them correctly by city match.

---

### 👤 Customer Accounts

#### Customer 1 (Primary)
```
Email: john@test.com
Password: Test@12345
Role: Customer
```
**Access:**
- Book fuel online
- View booking history
- View fuel prices
- Track orders

**Login URL:** http://localhost:4200/login

#### Customer 2 (Alternative)
```
Email: customer-test@ifms.com
Password: Customer@12345
Role: Customer
```
**Access:** Same as Customer 1

---

## Quick Login Test

### Test Admin Login
```bash
curl -X POST http://localhost:5010/gateway/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin-test@ifms.com","password":"Admin@12345"}'
```

### Test Dealer Login
```bash
curl -X POST http://localhost:5010/gateway/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"dealer-test@ifms.com","password":"Dealer@12345"}'
```

### Test Customer Login
```bash
curl -X POST http://localhost:5010/gateway/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"john@test.com","password":"Test@12345"}'
```

---

## Creating New Test Accounts

If you need to create additional test accounts:

```bash
curl -X POST http://localhost:5010/gateway/auth/register \
  -H "Content-Type: application/json" \
  -d '{
    "fullName": "Your Name",
    "email": "your-email@test.com",
    "password": "YourPassword@123",
    "role": "Customer"
  }'
```

**Valid Roles:** Customer, Dealer, Admin

---

## Notes

- All passwords follow the pattern: `[Role]@12345` or `Test@12345`
- Dealer account is pre-assigned to the main test station
- Customer accounts can book fuel from any active station
- Admin can see all data across the system
