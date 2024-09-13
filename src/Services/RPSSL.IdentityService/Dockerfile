FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services/RPSSL.IdentityService/RPSSL.IdentityService.csproj", "Services/RPSSL.IdentityService/"]
RUN dotnet restore "Services/RPSSL.IdentityService/RPSSL.IdentityService.csproj"

COPY src/ .
WORKDIR "/src/Services/RPSSL.IdentityService"
RUN dotnet build "RPSSL.IdentityService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RPSSL.IdentityService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a data directory and copy the existing SQLite database
RUN mkdir -p /app/data
COPY ["src/Services/RPSSL.IdentityService/User.db", "/app/data/User.db"]
# Ensure the SQLite database file is readable and writable
RUN chmod 644 /app/data/User.db

EXPOSE 443

ENTRYPOINT ["dotnet", "RPSSL.IdentityService.dll"]