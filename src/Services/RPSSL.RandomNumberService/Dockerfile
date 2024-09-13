FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["src/Services/RPSSL.RandomNumberService/RPSSL.RandomNumberService.csproj", "Services/RPSSL.RandomNumberService/"]
COPY ["src/Shared/RPSSL.Shared/RPSSL.Shared.csproj", "Shared/RPSSL.Shared/"]
RUN dotnet restore "Services/RPSSL.RandomNumberService/RPSSL.RandomNumberService.csproj"

COPY src/ .
WORKDIR "/src/Services/RPSSL.RandomNumberService"
RUN dotnet build "RPSSL.RandomNumberService.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "RPSSL.RandomNumberService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .
EXPOSE 443
ENTRYPOINT ["dotnet", "RPSSL.RandomNumberService.dll"]