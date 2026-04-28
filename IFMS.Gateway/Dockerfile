FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IFMS.Gateway/IFMS.Gateway.csproj", "IFMS.Gateway/"]
RUN dotnet restore "IFMS.Gateway/IFMS.Gateway.csproj"
COPY . .
WORKDIR "/src/IFMS.Gateway"
RUN dotnet build "IFMS.Gateway.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IFMS.Gateway.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IFMS.Gateway.dll"]
