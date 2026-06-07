FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["Station/IFMS.Station.API/IFMS.Station.API.csproj", "Station/IFMS.Station.API/"]
COPY ["Station/IFMS.Station.Application/IFMS.Station.Application.csproj", "Station/IFMS.Station.Application/"]
COPY ["Station/IFMS.Station.Domain/IFMS.Station.Domain.csproj", "Station/IFMS.Station.Domain/"]
COPY ["Station/IFMS.Station.Infrastructure/IFMS.Station.Infrastructure.csproj", "Station/IFMS.Station.Infrastructure/"]
RUN dotnet restore "Station/IFMS.Station.API/IFMS.Station.API.csproj"
COPY Station/ Station/
WORKDIR "/src/Station/IFMS.Station.API"
RUN dotnet build "IFMS.Station.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IFMS.Station.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IFMS.Station.API.dll"]
