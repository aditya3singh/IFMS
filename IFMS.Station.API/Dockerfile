FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IFMS.Station.API/IFMS.Station.API.csproj", "IFMS.Station.API/"]
COPY ["IFMS.Station.Application/IFMS.Station.Application.csproj", "IFMS.Station.Application/"]
COPY ["IFMS.Station.Domain/IFMS.Station.Domain.csproj", "IFMS.Station.Domain/"]
COPY ["IFMS.Station.Infrastructure/IFMS.Station.Infrastructure.csproj", "IFMS.Station.Infrastructure/"]
RUN dotnet restore "IFMS.Station.API/IFMS.Station.API.csproj"
COPY . .
WORKDIR "/src/IFMS.Station.API"
RUN dotnet build "IFMS.Station.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IFMS.Station.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IFMS.Station.API.dll"]
