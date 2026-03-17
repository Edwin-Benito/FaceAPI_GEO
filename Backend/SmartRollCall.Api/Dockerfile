# Etapa 1: Construcción
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Copiar solo el proyecto y restaurar (muy limpio)
COPY SmartRollCall.Api.csproj ./
RUN dotnet restore

# 2. Copiar todo lo demás
COPY . .

# 3. ELIMINAR BASURA LOCAL QUE SE HAYA COPIADO
RUN rm -rf obj bin

# 4. Publicar
RUN dotnet publish "SmartRollCall.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

# Etapa 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmartRollCall.Api.dll"]