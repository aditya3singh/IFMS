#!/bin/bash

echo "=========================================="
echo "Testing Admin Inventory Access"
echo "=========================================="
echo ""

# Login as admin
echo "1. Logging in as admin..."
LOGIN_RESPONSE=$(curl -s -X POST http://localhost:5001/api/identity/login \
  -H "Content-Type: application/json" \
  -d '{
    "email": "admin-test@ifms.com",
    "password": "Admin@12345"
  }')

TOKEN=$(echo $LOGIN_RESPONSE | grep -o '"token":"[^"]*' | cut -d'"' -f4)

if [ -z "$TOKEN" ]; then
  echo "❌ Admin login failed"
  echo "Response: $LOGIN_RESPONSE"
  exit 1
fi

echo "✅ Admin logged in successfully"
echo "Token: ${TOKEN:0:50}..."
echo ""

# Test inventory access
echo "2. Accessing inventory with admin token..."
INVENTORY_RESPONSE=$(curl -s -w "\nHTTP_CODE:%{http_code}" \
  -H "Authorization: Bearer $TOKEN" \
  http://localhost:5002/api/inventory)

HTTP_CODE=$(echo "$INVENTORY_RESPONSE" | grep "HTTP_CODE:" | cut -d: -f2)
BODY=$(echo "$INVENTORY_RESPONSE" | sed '/HTTP_CODE:/d')

echo "HTTP Status: $HTTP_CODE"

if [ "$HTTP_CODE" = "200" ]; then
  echo "✅ SUCCESS! Admin can access inventory"
  echo "Found $(echo "$BODY" | grep -o '"id"' | wc -l) inventory records"
elif [ "$HTTP_CODE" = "401" ]; then
  echo "❌ FAILED! Got 401 Unauthorized"
  echo "This means JWT configuration mismatch"
  echo "Response: $BODY"
elif [ "$HTTP_CODE" = "403" ]; then
  echo "❌ FAILED! Got 403 Forbidden"
  echo "This means admin role not recognized"
  echo "Response: $BODY"
else
  echo "❌ FAILED! Got HTTP $HTTP_CODE"
  echo "Response: $BODY"
fi

echo ""
echo "=========================================="
echo "Diagnosis"
echo "=========================================="
echo ""

if [ "$HTTP_CODE" = "401" ]; then
  echo "Problem: JWT token not accepted by Inventory API"
  echo ""
  echo "Cause: Inventory API JWT settings don't match Identity API"
  echo ""
  echo "Current Inventory API JWT settings:"
  docker inspect ifms-inventory-api | grep "Jwt__" | head -3
  echo ""
  echo "Should match Identity API settings:"
  docker inspect ifms-identity-api | grep "Jwt__" | head -3
  echo ""
  echo "Fix: Restart inventory-api with matching JWT settings"
elif [ "$HTTP_CODE" = "200" ]; then
  echo "✅ Everything working!"
  echo ""
  echo "If admin portal still redirects to login:"
  echo "  1. Clear browser cache (Ctrl+Shift+Delete)"
  echo "  2. Logout and login again as admin"
  echo "  3. Check browser console (F12) for errors"
  echo "  4. Verify token in localStorage (F12 > Application > Local Storage)"
fi

echo ""
