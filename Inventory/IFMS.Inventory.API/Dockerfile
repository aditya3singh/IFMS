FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IFMS.Inventory.API/IFMS.Inventory.API.csproj", "IFMS.Inventory.API/"]
COPY ["IFMS.Inventory.Application/IFMS.Inventory.Application.csproj", "IFMS.Inventory.Application/"]
COPY ["IFMS.Inventory.Domain/IFMS.Inventory.Domain.csproj", "IFMS.Inventory.Domain/"]
COPY ["IFMS.Inventory.Infrastructure/IFMS.Inventory.Infrastructure.csproj", "IFMS.Inventory.Infrastructure/"]
RUN dotnet restore "IFMS.Inventory.API/IFMS.Inventory.API.csproj"
COPY . .
WORKDIR "/src/IFMS.Inventory.API"
RUN dotnet build "IFMS.Inventory.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IFMS.Inventory.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IFMS.Inventory.API.dll"]