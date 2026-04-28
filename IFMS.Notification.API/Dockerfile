FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IFMS.Notification.API/IFMS.Notification.API.csproj", "IFMS.Notification.API/"]
RUN dotnet restore "IFMS.Notification.API/IFMS.Notification.API.csproj"
COPY . .
WORKDIR "/src/IFMS.Notification.API"
RUN dotnet build "IFMS.Notification.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IFMS.Notification.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IFMS.Notification.API.dll"]
