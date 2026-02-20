# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore Api/ServiceStationApi.csproj
RUN dotnet publish Api/ServiceStationApi.csproj -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app

RUN apt-get update && \
    apt-get install -y curl && \
    rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .
COPY start.sh .
RUN chmod +x start.sh

EXPOSE 8080

ENTRYPOINT ["./start.sh"]