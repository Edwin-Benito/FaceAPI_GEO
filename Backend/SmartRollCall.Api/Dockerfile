# Etapa 1: Construcción
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Copiamos el proyecto y limpiamos cualquier rastro previo
COPY ["SmartRollCall.Api.csproj", "./"]
RUN dotnet restore "SmartRollCall.Api.csproj"

# 2. Copiamos el resto del código
COPY . .

# 3. Borramos carpetas temporales locales que se hayan colado antes de publicar
RUN rm -rf obj bin

# 4. Publicamos (Quitamos el --no-restore para que él mismo asegure sus archivos)
RUN dotnet publish "SmartRollCall.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Render usa el 8080
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmartRollCall.Api.dll"]