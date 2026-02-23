FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY . .
RUN dotnet restore Api/ServiceStationApi.csproj
RUN dotnet publish Api/ServiceStationApi.csproj -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
COPY start.sh .
RUN chmod +x start.sh

EXPOSE 8080

ENTRYPOINT ["./start.sh"]