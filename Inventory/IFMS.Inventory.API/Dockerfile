FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Inventory/IFMS.Inventory.API/IFMS.Inventory.API.csproj", "Inventory/IFMS.Inventory.API/"]
COPY ["Inventory/IFMS.Inventory.Application/IFMS.Inventory.Application.csproj", "Inventory/IFMS.Inventory.Application/"]
COPY ["Inventory/IFMS.Inventory.Domain/IFMS.Inventory.Domain.csproj", "Inventory/IFMS.Inventory.Domain/"]
COPY ["Inventory/IFMS.Inventory.Infrastructure/IFMS.Inventory.Infrastructure.csproj", "Inventory/IFMS.Inventory.Infrastructure/"]
COPY ["Station/IFMS.Station.Application/IFMS.Station.Application.csproj", "Station/IFMS.Station.Application/"]
COPY ["Station/IFMS.Station.Infrastructure/IFMS.Station.Infrastructure.csproj", "Station/IFMS.Station.Infrastructure/"]
COPY ["Station/IFMS.Station.Domain/IFMS.Station.Domain.csproj", "Station/IFMS.Station.Domain/"]
COPY ["Shared/IFMS.Messaging/IFMS.Messaging.csproj", "Shared/IFMS.Messaging/"]
RUN dotnet restore "Inventory/IFMS.Inventory.API/IFMS.Inventory.API.csproj"
COPY Inventory/ Inventory/
COPY Station/ Station/
COPY Shared/ Shared/
WORKDIR "/src/Inventory/IFMS.Inventory.API"
RUN dotnet build "IFMS.Inventory.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IFMS.Inventory.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IFMS.Inventory.API.dll"]