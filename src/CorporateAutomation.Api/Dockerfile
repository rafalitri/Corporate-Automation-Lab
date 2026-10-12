
# Etapa 1: compilar y publicar la API.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Copiar los archivos de proyecto para restaurar dependencias.
COPY ["src/CorporateAutomation.Api/CorporateAutomation.Api.csproj", "src/CorporateAutomation.Api/"]
COPY ["src/CorporateAutomation.Core/CorporateAutomation.Core.csproj", "src/CorporateAutomation.Core/"]
COPY ["src/CorporateAutomation.Infrastructure/CorporateAutomation.Infrastructure.csproj", "src/CorporateAutomation.Infrastructure/"]

RUN dotnet restore "src/CorporateAutomation.Api/CorporateAutomation.Api.csproj"

# Copiar el código fuente.
COPY . .

# Publicar la aplicación.
RUN dotnet publish "src/CorporateAutomation.Api/CorporateAutomation.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore \
    /p:UseAppHost=false

# Etapa 2: imagen final para ejecutar la API.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish .

# Ejecutar con un usuario sin privilegios de administrador.
USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "CorporateAutomation.Api.dll"]
