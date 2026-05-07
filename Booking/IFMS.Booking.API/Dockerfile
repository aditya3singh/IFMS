FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IFMS.Booking.API/IFMS.Booking.API.csproj", "IFMS.Booking.API/"]
COPY ["IFMS.Booking.Application/IFMS.Booking.Application.csproj", "IFMS.Booking.Application/"]
COPY ["IFMS.Booking.Domain/IFMS.Booking.Domain.csproj", "IFMS.Booking.Domain/"]
COPY ["IFMS.Booking.Infrastructure/IFMS.Booking.Infrastructure.csproj", "IFMS.Booking.Infrastructure/"]
RUN dotnet restore "IFMS.Booking.API/IFMS.Booking.API.csproj"
COPY . .
WORKDIR "/src/IFMS.Booking.API"
RUN dotnet build "IFMS.Booking.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IFMS.Booking.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IFMS.Booking.API.dll"]
