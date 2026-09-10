# Imagen de la API. Se restauran primero solo los .csproj para aprovechar la cache de
# capas: cambiar codigo no vuelve a descargar los paquetes.
# Se compila el proyecto de la API y no el .slnx, porque el formato de solucion .slnx
# necesita un SDK mas nuevo que el de esta imagen.
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY SB.PortalSolicitudes.Domain/SB.PortalSolicitudes.Domain.csproj SB.PortalSolicitudes.Domain/
COPY SB.PortalSolicitudes.Application/SB.PortalSolicitudes.Application.csproj SB.PortalSolicitudes.Application/
COPY SB.PortalSolicitudes.Infraestructure/SB.PortalSolicitudes.Infraestructure.csproj SB.PortalSolicitudes.Infraestructure/
COPY SB.PortalSolicitudes.API/SB.PortalSolicitudes.API.csproj SB.PortalSolicitudes.API/
RUN dotnet restore SB.PortalSolicitudes.API/SB.PortalSolicitudes.API.csproj

COPY . .
RUN dotnet publish SB.PortalSolicitudes.API/SB.PortalSolicitudes.API.csproj \
    --configuration Release --no-restore --output /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SB.PortalSolicitudes.API.dll"]
