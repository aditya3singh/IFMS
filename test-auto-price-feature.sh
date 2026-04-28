#!/bin/bash

echo "=========================================="
echo "Testing Auto-Fill Fuel Price Feature"
echo "=========================================="
echo ""

# Login as dealer
echo "1. Logging in as dealer..."
LOGIN_RESPONSE=$(curl -s -X POST http://localhost:5010/gateway/identity/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "dealer-test@ifms.com",
    "password": "Dealer@12345"
  }')

TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"token":"[^"]*' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
  echo "❌ Login failed"
  echo "Response: $LOGIN_RESPONSE"
  exit 1
fi

echo "✅ Login successful"
echo ""

# Get dealer's station
echo "2. Getting dealer's station..."
STATION_RESPONSE=$(curl -s -X GET http://localhost:5010/gateway/stations/mine \
  -H "Authorization: Bearer $TOKEN")

STATION_ID=$(echo $STATION_RESPONSE | grep -o '"id":"[^"]*' | head -1 | cut -d'"' -f4)
STATION_NAME=$(echo $STATION_RESPONSE | grep -o '"name":"[^"]*' | head -1 | cut -d'"' -f4)
STATE=$(echo $STATION_RESPONSE | grep -o '"state":"[^"]*' | head -1 | cut -d'"' -f4)
CITY=$(echo $STATION_RESPONSE | grep -o '"city":"[^"]*' | head -1 | cut -d'"' -f4)

echo "Station ID: $STATION_ID"
echo "Station Name: $STATION_NAME"
echo "Location: $CITY, $STATE"
echo ""

# Get real-time fuel prices for the station
echo "3. Fetching real-time fuel prices for station..."
PRICE_RESPONSE=$(curl -s -X GET "http://localhost:5010/gateway/stations/$STATION_ID/realtime-price" \
  -H "Authorization: Bearer $TOKEN")

echo "Price Response:"
echo $PRICE_RESPONSE | jq '.' 2>/dev/null || echo $PRICE_RESPONSE
echo ""

# Extract prices
PETROL_PRICE=$(echo $PRICE_RESPONSE | grep -o '"petrol":[0-9.]*' | cut -d':' -f2)
DIESEL_PRICE=$(echo $PRICE_RESPONSE | grep -o '"diesel":[0-9.]*' | cut -d':' -f2)
CNG_PRICE=$(echo $PRICE_RESPONSE | grep -o '"cng":[0-9.]*' | cut -d':' -f2)

echo "📊 Current Fuel Prices:"
echo "   Petrol: ₹$PETROL_PRICE/L"
echo "   Diesel: ₹$DIESEL_PRICE/L"
echo "   CNG: ₹$CNG_PRICE/L"
echo ""

# Test creating a sale with auto-fetched price
echo "4. Creating a test sale with auto-fetched Petrol price..."
SALE_DATA='{
  "stationId": "'$STATION_ID'",
  "fuelType": "Petrol",
  "quantity": 10,
  "pricePerLitre": '$PETROL_PRICE',
  "paymentMethod": "UPI",
  "customerName": "Auto-Price Test Customer"
}'

SALE_RESPONSE=$(curl -s -X POST http://localhost:5010/gateway/sales \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer $TOKEN" \
  -d "$SALE_DATA")

TRANSACTION_ID=$(echo $SALE_RESPONSE | grep -o '"id":"[^"]*' | head -1 | cut -d'"' -f4)
TOTAL_AMOUNT=$(echo $SALE_RESPONSE | grep -o '"totalAmount":[0-9.]*' | cut -d':' -f2)

if [ -n "$TRANSACTION_ID" ]; then
  echo "✅ Sale created successfully!"
  echo "   Transaction ID: $TRANSACTION_ID"
  echo "   Total Amount: ₹$TOTAL_AMOUNT"
  echo "   (10L × ₹$PETROL_PRICE = ₹$TOTAL_AMOUNT)"
else
  echo "❌ Sale creation failed"
  echo "Response: $SALE_RESPONSE"
fi

echo ""
echo "=========================================="
echo "✅ Auto-Fill Price Feature Test Complete"
echo "=========================================="
echo ""
echo "📝 What was tested:"
echo "   1. Dealer login"
echo "   2. Fetch station details"
echo "   3. Get real-time fuel prices from API"
echo "   4. Create sale with auto-fetched price"
echo ""
echo "🌐 Frontend Testing:"
echo "   1. Open http://localhost:4200"
echo "   2. Login as dealer (dealer-test@ifms.com / Dealer@12345)"
echo "   3. Go to Sales page"
echo "   4. Notice the Price/Litre field is:"
echo "      - Auto-filled with live price"
echo "      - Read-only (gray background)"
echo "      - Shows loading spinner while fetching"
echo "      - Shows price source below the form"
echo "   5. Change fuel type - price updates automatically"
echo ""
