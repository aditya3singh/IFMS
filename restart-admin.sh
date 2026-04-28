#!/bin/bash
docker stop ifms-admin-api
docker rm ifms-admin-api
docker run -d \
  --name ifms-admin-api \
  --network ifms_default \
  -p 5004:8080 \
  -e "Jwt__Key=IFMS_SuperSecretKey_2024_MustBe32Chars!!" \
  -e "Jwt__Issuer=IFMS.Identity" \
  -e "Jwt__Audience=IFMS.Client" \
  -e "ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=IFMS_SalesDB;User Id=sa;Password=Admin@12345;TrustServerCertificate=True" \
  -e "ConnectionStrings__StationConnection=Server=sqlserver,1433;Database=IFMS_StationDB;User Id=sa;Password=Admin@12345;TrustServerCertificate=True" \
  -e "ConnectionStrings__BookingConnection=Server=sqlserver,1433;Database=IFMS_BookingDB;User Id=sa;Password=Admin@12345;TrustServerCertificate=True" \
  -e "ConnectionStrings__InventoryConnection=Server=sqlserver,1433;Database=IFMS_InventoryDB;User Id=sa;Password=Admin@12345;TrustServerCertificate=True" \
  ifms-admin-api
