#!/bin/bash

echo "=========================================="
echo "Testing Real-Time Fuel Price API"
echo "=========================================="
echo ""

# Test 1: Get available states
echo "Test 1: Get Available States"
echo "----------------------------"
curl -s http://localhost:5010/gateway/stations/available-states | jq '.' || echo "Failed"
echo ""
echo ""

# Test 2: Get districts in Punjab
echo "Test 2: Get Districts in Punjab"
echo "--------------------------------"
curl -s http://localhost:5010/gateway/stations/districts/Punjab | jq '.' || echo "Failed"
echo ""
echo ""

# Test 3: Get real-time fuel price for Delhi
echo "Test 3: Real-Time Fuel Price - Delhi"
echo "-------------------------------------"
curl -s "http://localhost:5010/gateway/stations/realtime-price?state=Delhi&district=New%20Delhi" | jq '.' || echo "Failed"
echo ""
echo ""

# Test 4: Get real-time fuel price for Mumbai
echo "Test 4: Real-Time Fuel Price - Mumbai"
echo "--------------------------------------"
curl -s "http://localhost:5010/gateway/stations/realtime-price?state=Maharashtra&district=Mumbai" | jq '.' || echo "Failed"
echo ""
echo ""

# Test 5: Get all prices for Punjab
echo "Test 5: All Fuel Prices in Punjab"
echo "----------------------------------"
curl -s http://localhost:5010/gateway/stations/state-prices/Punjab | jq '.districts[:3]' || echo "Failed"
echo ""
echo ""

# Test 6: Get real-time price for a specific station
echo "Test 6: Real-Time Price for Station"
echo "------------------------------------"
STATION_ID="44444444-4444-4444-4444-444444444444"
curl -s "http://localhost:5010/gateway/stations/$STATION_ID/realtime-price" | jq '.' || echo "Failed"
echo ""
echo ""

echo "=========================================="
echo "Testing Complete!"
echo "=========================================="
echo ""
echo "Note: Prices are fetched from Indian Fuel Price API"
echo "Prices update daily at 6:00 AM IST"
echo "Data is cached for 6 hours for performance"
