# Etapa 1: Construcción
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar solo el proyecto y restaurar
COPY ["SmartRollCall.Api.csproj", "./"]
RUN dotnet restore "SmartRollCall.Api.csproj"

# Copiar todo el código
COPY . .

# Publicar (Ya no debería fallar por los .resx)
RUN dotnet publish "SmartRollCall.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render usa el puerto 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmartRollCall.Api.dll"]