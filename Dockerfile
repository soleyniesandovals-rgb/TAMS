# Etapa 1: restaurar y publicar la API.
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar primero solo los proyectos para aprovechar la caché de capas de NuGet.
COPY src/Tams.Api/Tams.Api.csproj src/Tams.Api/
COPY src/Core/Core.csproj src/Core/
COPY src/Tams.Negocio/Tams.Negocio.csproj src/Tams.Negocio/
RUN dotnet restore src/Tams.Api/Tams.Api.csproj

COPY . .
RUN dotnet publish src/Tams.Api/Tams.Api.csproj -c Release -o /app/publish --no-restore

# Etapa 2: imagen de ejecución, sin SDK y con usuario no root.
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_HTTP_PORTS=8080
ENV TAMS_DOCUMENTOS_RUTA=/app/almacen

# La imagen base trae el usuario "app" (uid/gid 1654). Se crea la carpeta de
# almacenamiento y se entrega a ese usuario antes de cambiar de usuario.
RUN mkdir -p /app/almacen && chown -R app:app /app

COPY --from=build --chown=app:app /app/publish .

USER app

EXPOSE 8080

CMD ["dotnet", "Tams.Api.dll"]
