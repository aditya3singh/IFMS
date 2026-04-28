#!/bin/bash
docker stop ifms-notification-api
docker rm ifms-notification-api
docker run -d \
  --name ifms-notification-api \
  --network ifms_default \
  -p 5005:8080 \
  -e "Jwt__Key=IFMS_SuperSecretKey_2024_MustBe32Chars!!" \
  -e "Jwt__Issuer=IFMS.Identity" \
  -e "Jwt__Audience=IFMS.Client" \
  -e "InternalApiKey=ifms-internal-2024" \
  -e "ASPNETCORE_ENVIRONMENT=Development" \
  ifms-notification-api
