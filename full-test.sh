#!/bin/bash
echo "========== FULL SYSTEM TEST =========="

echo ""
echo "1. ADMIN LOGIN"
ADMIN_TOKEN=$(curl -s -X POST http://localhost:5010/gateway/auth/login -H "Content-Type: application/json" -d '{"email":"admin-test@ifms.com","password":"Admin@12345"}' | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('token',''))" 2>/dev/null)
[ -n "$ADMIN_TOKEN" ] && echo "OK Admin login" || echo "FAIL Admin login"

echo ""
echo "2. DEALER LOGIN"
DEALER_TOKEN=$(curl -s -X POST http://localhost:5010/gateway/auth/login -H "Content-Type: application/json" -d '{"email":"dealer-test@ifms.com","password":"Dealer@12345"}' | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('token',''))" 2>/dev/null)
[ -n "$DEALER_TOKEN" ] && echo "OK Dealer login" || echo "FAIL Dealer login"

echo ""
echo "3. CUSTOMER LOGIN"
CUST_TOKEN=$(curl -s -X POST http://localhost:5010/gateway/auth/login -H "Content-Type: application/json" -d '{"email":"john@test.com","password":"Test@12345"}' | python3 -c "import sys,json; d=json.load(sys.stdin); print(d.get('token',''))" 2>/dev/null)
[ -n "$CUST_TOKEN" ] && echo "OK Customer login" || echo "FAIL Customer login"

echo ""
echo "4. STATIONS"
curl -s http://localhost:5010/gateway/stations | python3 -c "import sys,json; d=json.load(sys.stdin); print(f'OK {len(d)} stations')" 2>/dev/null || echo "FAIL stations"

echo ""
echo "5. INVENTORY (public)"
curl -s "http://localhost:5002/api/inventory/station/44444444-4444-4444-4444-444444444444" | python3 -c "import sys,json; d=json.load(sys.stdin); print(f'OK {len(d)} inventory records')" 2>/dev/null || echo "FAIL inventory"

echo ""
echo "6. ADMIN INVENTORY"
curl -s "http://localhost:5002/api/inventory" -H "Authorization: Bearer $ADMIN_TOKEN" | python3 -c "import sys,json; d=json.load(sys.stdin); print(f'OK {len(d)} total records')" 2>/dev/null || echo "FAIL admin inventory"

echo ""
echo "7. DEALER STATIONS"
curl -s "http://localhost:5010/gateway/stations/mine" -H "Authorization: Bearer $DEALER_TOKEN" | python3 -c "import sys,json; d=json.load(sys.stdin); print(f'OK {len(d)} assigned stations')" 2>/dev/null || echo "FAIL dealer stations"

echo ""
echo "8. SALES"
curl -s "http://localhost:5010/gateway/sales" -H "Authorization: Bearer $DEALER_TOKEN" | python3 -c "import sys,json; d=json.load(sys.stdin); data=d.get('data',d) if isinstance(d,dict) else d; print(f'OK {len(data)} transactions')" 2>/dev/null || echo "FAIL sales"

echo ""
echo "9. ADMIN DASHBOARD"
DASH=$(curl -s "http://localhost:5010/gateway/admin/reports/summary" -H "Authorization: Bearer $ADMIN_TOKEN")
[ -n "$DASH" ] && echo "OK Admin dashboard" || echo "FAIL Admin dashboard"

echo ""
echo "10. BOOKING (customer)"
BOOK=$(curl -s "http://localhost:5010/gateway/booking" -H "Authorization: Bearer $CUST_TOKEN")
[ -n "$BOOK" ] && echo "OK Booking endpoint" || echo "FAIL Booking endpoint"

echo ""
echo "=========================================="
echo "FRONTEND: http://localhost:4200"
echo "=========================================="
