#!/bin/bash

echo "🔧 Testing Admin Dashboard Fix"
echo "================================"

# Get dealer token
echo "1. Logging in as dealer..."
DEALER_TOKEN=$(curl -s -X POST http://localhost:5010/gateway/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"dealer-test@ifms.com","password":"Dealer@12345"}' \
  | grep -o '"token":"[^"]*' | cut -d'"' -f4)

if [ -z "$DEALER_TOKEN" ]; then
    echo "❌ Failed to login as dealer"
    exit 1
fi
echo "✅ Dealer logged in"

# Get station ID
echo ""
echo "2. Getting station ID..."
STATION_ID=$(curl -s http://localhost:5010/gateway/stations | grep -o '"id":"[^"]*' | head -1 | cut -d'"' -f4)
if [ -z "$STATION_ID" ]; then
    STATION_ID=$(curl -s http://localhost:5010/gateway/stations | grep -o '"Id":"[^"]*' | head -1 | cut -d'"' -f4)
fi

if [ -z "$STATION_ID" ]; then
    echo "❌ No stations found"
    exit 1
fi
echo "✅ Station ID: $STATION_ID"

# Create 10 test transactions
echo ""
echo "3. Creating 10 test transactions..."
for i in {1..10}; do
    QUANTITY=$((20 + i * 5))
    RESULT=$(curl -s -X POST http://localhost:5010/gateway/sales \
      -H "Authorization: Bearer $DEALER_TOKEN" \
      -H "Content-Type: application/json" \
      -d "{
        \"stationId\": \"$STATION_ID\",
        \"fuelType\": \"Petrol\",
        \"quantity\": $QUANTITY,
        \"pricePerLitre\": 95.50,
        \"paymentMethod\": \"Cash\",
        \"customerName\": \"Test Customer $i\"
      }")
    
    if echo "$RESULT" | grep -q "id\|Id"; then
        echo "  ✅ Transaction $i created: ${QUANTITY}L"
    else
        echo "  ⚠️  Transaction $i failed"
    fi
    sleep 0.5
done

# Get admin token
echo ""
echo "4. Logging in as admin..."
ADMIN_TOKEN=$(curl -s -X POST http://localhost:5010/gateway/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"admin-test@ifms.com","password":"Admin@12345"}' \
  | grep -o '"token":"[^"]*' | cut -d'"' -f4)

if [ -z "$ADMIN_TOKEN" ]; then
    echo "❌ Failed to login as admin"
    exit 1
fi
echo "✅ Admin logged in"

# Test admin overview
echo ""
echo "5. Testing Admin Overview..."
OVERVIEW=$(curl -s http://localhost:5010/gateway/admin/overview \
  -H "Authorization: Bearer $ADMIN_TOKEN")

echo "$OVERVIEW" | python3 -m json.tool 2>/dev/null || echo "$OVERVIEW"

TOTAL_TX=$(echo "$OVERVIEW" | grep -o '"totalTransactions":[0-9]*' | grep -o '[0-9]*')
TOTAL_REV=$(echo "$OVERVIEW" | grep -o '"totalRevenue":[0-9.]*' | grep -o '[0-9.]*')
PETROL=$(echo "$OVERVIEW" | grep -o '"petrolSold":[0-9.]*' | grep -o '[0-9.]*')

echo ""
echo "📊 Results:"
echo "  Total Transactions: $TOTAL_TX"
echo "  Total Revenue: ₹$TOTAL_REV"
echo "  Petrol Sold: ${PETROL}L"

# Test daily report
echo ""
echo "6. Testing Daily Report..."
TODAY=$(date +%Y-%m-%d)
DAILY=$(curl -s "http://localhost:5010/gateway/admin/daily-report?date=$TODAY" \
  -H "Authorization: Bearer $ADMIN_TOKEN")

echo "$DAILY" | python3 -m json.tool 2>/dev/null || echo "$DAILY"

# Test fraud monitor
echo ""
echo "7. Testing Fraud Monitor..."
FRAUD=$(curl -s http://localhost:5010/gateway/admin/fraud-monitor \
  -H "Authorization: Bearer $ADMIN_TOKEN")

FRAUD_COUNT=$(echo "$FRAUD" | grep -o '"totalFlagged":[0-9]*' | grep -o '[0-9]*')
echo "  Fraud Alerts: $FRAUD_COUNT"

echo ""
echo "================================"
echo "✅ Admin Dashboard Test Complete!"
echo ""
echo "Now refresh your admin dashboard at:"
echo "http://localhost:4200/admin"
echo ""
