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
IFMS/
├── IFMS.Identity.*          # Auth, users, OTP
├── IFMS.Booking.*           # Fuel bookings, tokens, KYC
├── IFMS.Sales.*             # Transactions, complaints
├── IFMS.Inventory.*         # Fuel stock management
├── IFMS.Station.*           # Station & dealer management
├── IFMS.Admin.*             # Reports, fraud monitoring
├── IFMS.Notification.*      # SMS, Email, in-app alerts
├── IFMS.GraphQL.API/        # GraphQL query layer
├── IFMS.Messaging/          # Shared RabbitMQ event contracts
├── IFMS.Gateway/            # Ocelot API gateway
├── ifms-frontend/           # Angular 17 frontend
├── database/                # SQL schema diagrams
└── docker-compose.yml       # Full stack orchestration
```
