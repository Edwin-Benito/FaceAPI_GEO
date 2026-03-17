# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar proyecto y restaurar
COPY ["SmartRollCall.Api.csproj", "./"]
RUN dotnet restore "SmartRollCall.Api.csproj"

# Copiar TODO el código
COPY . .

# Publicar (Aquí es donde fallaba antes)
RUN dotnet publish "SmartRollCall.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render usa el puerto 8080 por defecto para Docker
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmartRollCall.Api.dll"]