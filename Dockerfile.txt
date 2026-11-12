# Etapa 1: compilar
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish APIRest_min/APIRest_min.csproj -c Release -o /app/publish
 
# Etapa 2: ejecutar (imagen mas ligera, sin el SDK)
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app/publish .
 
# Render espera que el servicio escuche en el puerto 10000, en 0.0.0.0
ENV ASPNETCORE_URLS=http://+:10000
EXPOSE 10000
 
ENTRYPOINT ["dotnet", "APIRest_min.dll"] 