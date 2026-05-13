FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copiar archivos de proyecto y restaurar
COPY ["Presentation/API/API.csproj", "Presentation/API/"]
COPY ["Presentation/Contracts/Contracts.csproj", "Presentation/Contracts/"]
COPY ["Presentation/src/RentGuard.Core.Business/RentGuard.Core.Business.csproj", "Presentation/src/RentGuard.Core.Business/"]
RUN dotnet restore "Presentation/API/API.csproj"

# Copiar todo y compilar
COPY . .
WORKDIR "/app/Presentation/API"
RUN dotnet publish "API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Imagen de ejecucin
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "API.dll"]
