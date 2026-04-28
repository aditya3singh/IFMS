FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IFMS.GraphQL.API/IFMS.GraphQL.API.csproj", "IFMS.GraphQL.API/"]
COPY ["IFMS.Booking.Infrastructure/IFMS.Booking.Infrastructure.csproj", "IFMS.Booking.Infrastructure/"]
COPY ["IFMS.Booking.Domain/IFMS.Booking.Domain.csproj", "IFMS.Booking.Domain/"]
COPY ["IFMS.Booking.Application/IFMS.Booking.Application.csproj", "IFMS.Booking.Application/"]
COPY ["IFMS.Sales.Infrastructure/IFMS.Sales.Infrastructure.csproj", "IFMS.Sales.Infrastructure/"]
COPY ["IFMS.Sales.Domain/IFMS.Sales.Domain.csproj", "IFMS.Sales.Domain/"]
COPY ["IFMS.Sales.Application/IFMS.Sales.Application.csproj", "IFMS.Sales.Application/"]
COPY ["IFMS.Inventory.Infrastructure/IFMS.Inventory.Infrastructure.csproj", "IFMS.Inventory.Infrastructure/"]
COPY ["IFMS.Inventory.Domain/IFMS.Inventory.Domain.csproj", "IFMS.Inventory.Domain/"]
COPY ["IFMS.Inventory.Application/IFMS.Inventory.Application.csproj", "IFMS.Inventory.Application/"]
COPY ["IFMS.Station.Infrastructure/IFMS.Station.Infrastructure.csproj", "IFMS.Station.Infrastructure/"]
COPY ["IFMS.Station.Domain/IFMS.Station.Domain.csproj", "IFMS.Station.Domain/"]
COPY ["IFMS.Station.Application/IFMS.Station.Application.csproj", "IFMS.Station.Application/"]
COPY ["IFMS.Admin.Infrastructure/IFMS.Admin.Infrastructure.csproj", "IFMS.Admin.Infrastructure/"]
COPY ["IFMS.Admin.Application/IFMS.Admin.Application.csproj", "IFMS.Admin.Application/"]
COPY ["IFMS.Messaging/IFMS.Messaging.csproj", "IFMS.Messaging/"]
RUN dotnet restore "IFMS.GraphQL.API/IFMS.GraphQL.API.csproj"
COPY . .
WORKDIR "/src/IFMS.GraphQL.API"
RUN dotnet build "IFMS.GraphQL.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IFMS.GraphQL.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IFMS.GraphQL.API.dll"]
