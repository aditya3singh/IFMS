FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IFMS.Sales.API/IFMS.Sales.API.csproj", "IFMS.Sales.API/"]
COPY ["IFMS.Sales.Application/IFMS.Sales.Application.csproj", "IFMS.Sales.Application/"]
COPY ["IFMS.Sales.Domain/IFMS.Sales.Domain.csproj", "IFMS.Sales.Domain/"]
COPY ["IFMS.Sales.Infrastructure/IFMS.Sales.Infrastructure.csproj", "IFMS.Sales.Infrastructure/"]
RUN dotnet restore "IFMS.Sales.API/IFMS.Sales.API.csproj"
COPY . .
WORKDIR "/src/IFMS.Sales.API"
RUN dotnet build "IFMS.Sales.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IFMS.Sales.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IFMS.Sales.API.dll"]