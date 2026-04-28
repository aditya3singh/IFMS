FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["IFMS.Identity.API/IFMS.Identity.API.csproj", "IFMS.Identity.API/"]
COPY ["IFMS.Identity.Application/IFMS.Identity.Application.csproj", "IFMS.Identity.Application/"]
COPY ["IFMS.Identity.Domain/IFMS.Identity.Domain.csproj", "IFMS.Identity.Domain/"]
COPY ["IFMS.Identity.Infrastructure/IFMS.Identity.Infrastructure.csproj", "IFMS.Identity.Infrastructure/"]
RUN dotnet restore "IFMS.Identity.API/IFMS.Identity.API.csproj"
COPY . .
WORKDIR "/src/IFMS.Identity.API"
RUN dotnet build "IFMS.Identity.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "IFMS.Identity.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "IFMS.Identity.API.dll"]