FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IFMS.Admin.API/IFMS.Admin.API.csproj", "IFMS.Admin.API/"]
COPY ["IFMS.Admin.Application/IFMS.Admin.Application.csproj", "IFMS.Admin.Application/"]
COPY ["IFMS.Admin.Infrastructure/IFMS.Admin.Infrastructure.csproj", "IFMS.Admin.Infrastructure/"]
RUN dotnet restore "IFMS.Admin.API/IFMS.Admin.API.csproj"
COPY . .
WORKDIR "/src/IFMS.Admin.API"
RUN dotnet build "IFMS.Admin.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IFMS.Admin.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IFMS.Admin.API.dll"]