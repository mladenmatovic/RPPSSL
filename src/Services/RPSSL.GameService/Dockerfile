FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services/RPSSL.GameService/RPSSL.GameService.csproj", "Services/RPSSL.GameService/"]
COPY ["src/Shared/RPSSL.Shared/RPSSL.Shared.csproj", "Shared/RPSSL.Shared/"]
RUN dotnet restore "Services/RPSSL.GameService/RPSSL.GameService.csproj"

COPY src/ .
WORKDIR "/src/Services/RPSSL.GameService"
RUN dotnet build "RPSSL.GameService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RPSSL.GameService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create a data directory and copy the existing SQLite database
RUN mkdir -p /app/data
COPY ["src/Services/RPSSL.GameService/Games.db", "/app/data/Games.db"]
# Ensure the SQLite database file is readable and writable
RUN chmod 644 /app/data/Games.db

EXPOSE 443

ENTRYPOINT ["dotnet", "RPSSL.GameService.dll"]