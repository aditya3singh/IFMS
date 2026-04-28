#!/bin/bash
docker stop ifms-gateway
docker rm ifms-gateway
docker run -d \
  --name ifms-gateway \
  --network ifms_default \
  -p 5010:8080 \
  -e "Jwt__Key=IFMS_SuperSecretKey_2024_MustBe32Chars!!" \
  -e "Jwt__Issuer=IFMS.Identity" \
  -e "Jwt__Audience=IFMS.Client" \
  -e "ASPNETCORE_ENVIRONMENT=Development" \
  ifms-gateway
