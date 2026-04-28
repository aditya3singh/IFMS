#!/bin/bash
docker run -d \
  --name ifms-identity-api \
  --network ifms_default \
  -p 5001:8080 \
  -e "ConnectionStrings__DefaultConnection=Server=sqlserver,1433;Database=IFMS_IdentityDB;User Id=sa;Password=Admin@12345;TrustServerCertificate=True" \
  -e "Jwt__Key=IFMS_SuperSecretKey_2024_MustBe32Chars!!" \
  -e "Jwt__Issuer=IFMS.Identity" \
  -e "Jwt__Audience=IFMS.Client" \
  -e "ASPNETCORE_ENVIRONMENT=Development" \
  ifms-identity-api
