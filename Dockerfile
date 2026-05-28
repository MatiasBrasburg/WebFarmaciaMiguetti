FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /app

# Restaurar dependencias
COPY *.csproj ./
RUN dotnet restore

# Copiar todo y compilar
COPY . ./
RUN dotnet publish -c Release -o out

# Imagen de corrida
FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/out .

EXPOSE 80
ENTRYPOINT ["dotnet", "WebFarmaciaMiguetti.dll"]