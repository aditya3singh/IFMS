# IFMS — Integrated Fuel Management System

A production-grade microservices platform for managing fuel stations, bookings, inventory, and sales.

## Architecture

```
Angular Frontend (4200)
        │
   Ocelot Gateway (5010)
        │
┌───────┼───────────────────────────────────────┐
│       │                                       │
Identity  Booking   Sales   Inventory  Station  │
  API     API       API       API       API     │
 (5001)  (5007)   (5003)    (5002)    (5006)   │
                                                │
         Notification  Admin   GraphQL          │
            API         API      API            │
           (5005)      (5004)   (5011)          │
└───────────────────────────────────────────────┘
        │               │
     RabbitMQ         Redis
     (5672)           (6379)
        │
   SQL Server (1433)
   5 separate databases
```

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Backend | ASP.NET Core 8, C# 12 |
| Frontend | Angular 17, TypeScript, Tailwind CSS |
| Database | SQL Server (Azure SQL Edge) |
| Cache | Redis 7 |
| Message Bus | RabbitMQ 3 + MassTransit 8 |
| API Gateway | Ocelot 23 |
| GraphQL | Hot Chocolate 14 |
| Auth | JWT Bearer + BCrypt |
| SMS | Twilio |
| Email | Gmail SMTP |
| Container | Docker + Docker Compose |

## Quick Start

### Prerequisites
- Docker Desktop
- Node.js 18+ (for frontend)

### 1. Configure credentials

Copy and fill in your credentials:

```bash
# In docker-compose.yml, replace placeholders:
Twilio__AccountSid: YOUR_TWILIO_ACCOUNT_SID
Twilio__AuthToken: YOUR_TWILIO_AUTH_TOKEN
Twilio__FromPhone: YOUR_TWILIO_PHONE_NUMBER
Gmail__User: YOUR_GMAIL_ADDRESS
Gmail__AppPassword: YOUR_GMAIL_APP_PASSWORD
```

### 2. Start all services

```bash
cd IFMS
docker compose up --build
```

Wait ~60 seconds for all services to be healthy.

### 3. Start frontend

```bash
cd ifms-frontend
npm install
npm start
```

Open **http://localhost:4200**

## Service URLs

| Service | URL |
|---------|-----|
| Frontend | http://localhost:4200 |
| Gateway | http://localhost:5010 |
| GraphQL | http://localhost:5011/graphql |
| RabbitMQ UI | http://localhost:15672 (ifms / ifms@12345) |
| Identity Swagger | http://localhost:5001/swagger |
| Booking Swagger | http://localhost:5007/swagger |
| Sales Swagger | http://localhost:5003/swagger |
| Inventory Swagger | http://localhost:5002/swagger |
| Station Swagger | http://localhost:5006/swagger |
| Admin Swagger | http://localhost:5004/swagger |
| Notification Swagger | http://localhost:5005/swagger |

## Test Accounts

| Role | Email | Password |
|------|-------|----------|
| Admin | admin-test@ifms.com | Admin@12345 |
| Dealer | dealer-test@ifms.com | Dealer@12345 |
| Customer | customer-test@ifms.com | Customer@12345 |

## GraphQL Example Queries

```graphql
# Admin overview
{ adminOverview { totalTransactions totalRevenue petrolSold dieselSold } }

# Revenue trend
{ revenueTrend(from: "2026-01-01", to: "2026-12-31", groupBy: "month") {
    period revenue transactions
  }
}

# Customer bookings
{ customerBookings(customerId: "YOUR_GUID") {
    tokenCode fuelType quantityLiters totalPaid tokenStatus
  }
}

# Low stock alerts
{ fuelStocks(lowStockOnly: true) { stationId fuelType quantity } }
```

## Project Structure

```
📂 IFMS Platform Repository
│
├── 🌐 Frontend
│   └── ifms-frontend/                    # Angular 17 SPA — port 4200
│
├── 🚪 Gateways & Aggregators
│   ├── IFMS.Gateway/                     # API Gateway (Ocelot) — port 5010
│   └── IFMS.GraphQL.API/                 # GraphQL Aggregation Layer — port 5011
│
├── ⚙️ Core Microservices
│   │
│   ├── Admin/
│   │   ├── IFMS.Admin.API                # Reports, fraud monitor — port 5004
│   │   ├── IFMS.Admin.Application
│   │   └── IFMS.Admin.Infrastructure
│   │
│   ├── Booking/
│   │   ├── IFMS.Booking.API              # Bookings, KYC, tokens — port 5007
│   │   ├── IFMS.Booking.Application
│   │   ├── IFMS.Booking.Domain
│   │   ├── IFMS.Booking.Infrastructure   # EF Core, Redis, RabbitMQ publisher
│   │   └── IFMS.Booking.Tests
│   │
│   ├── Identity/
│   │   ├── IFMS.Identity.API             # Auth, users, OTP — port 5001
│   │   ├── IFMS.Identity.Application
│   │   ├── IFMS.Identity.Domain
│   │   ├── IFMS.Identity.Infrastructure  # JWT, BCrypt, OTP delivery
│   │   └── IFMS.Identity.Tests
│   │
│   ├── Inventory/
│   │   ├── IFMS.Inventory.API            # Fuel stock management — port 5002
│   │   ├── IFMS.Inventory.Application
│   │   ├── IFMS.Inventory.Domain
│   │   ├── IFMS.Inventory.Infrastructure # EF Core, RabbitMQ publisher
│   │   └── IFMS.Inventory.Tests
│   │
│   ├── Notification/
│   │   ├── IFMS.Notification.API         # Twilio SMS, Gmail, in-app — port 5005
│   │   │   ├── Consumers/                # RabbitMQ MassTransit consumers
│   │   │   ├── Controllers/
│   │   │   ├── DTOs/
│   │   │   └── Services/                 # RealNotificationService, NotificationStore
│   │   └── IFMS.Notification.Tests
│   │
│   ├── Sales/
│   │   ├── IFMS.Sales.API                # Transactions, complaints — port 5003
│   │   ├── IFMS.Sales.Application
│   │   ├── IFMS.Sales.Domain             # Transaction, Complaint entities
│   │   ├── IFMS.Sales.Infrastructure     # EF Core, RabbitMQ publisher
│   │   └── IFMS.Sales.Tests
│   │
│   └── Station/
│       ├── IFMS.Station.API              # Stations, dealers — port 5006
│       ├── IFMS.Station.API.Tests
│       ├── IFMS.Station.Application      # DTOs, pricing
│       ├── IFMS.Station.Domain
│       ├── IFMS.Station.Domain.Tests
│       ├── IFMS.Station.Infrastructure
│       └── IFMS.Station.Infrastructure.Tests
│
├── 📦 Shared Libraries
│   └── IFMS.Messaging/                   # RabbitMQ event contracts (MassTransit)
│       └── Events/
│           ├── BookingEvents.cs          # BookingCreated, BookingConfirmed, BookingCancelled
│           ├── SalesEvents.cs            # SaleRecorded
│           └── InventoryEvents.cs        # LowStockAlert
│
├── 🗄️ Database
│   ├── database/                         # SQL schema scripts & seed data
│   ├── fix-db/                           # One-time DB fix utilities
│   └── seed-stations.sql
│
├── 🐳 DevOps & Configuration
│   ├── docker-compose.yml                # Full stack (12 containers)
│   ├── .gitignore
│   ├── README.md
│   └── TEST-CREDENTIALS.md
│
└── 🛠️ Scripts & Testing
    ├── full-test.sh
    ├── restart-*.sh                      # Per-service restart helpers
    ├── test-*.sh                         # API test scripts
    ├── test-apis.html                    # Browser-based API tester
    └── test-price-api.html
```
