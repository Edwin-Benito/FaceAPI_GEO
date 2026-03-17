FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# 1. Restaurar usando solo el csproj
COPY ["SmartRollCall.Api.csproj", "./"]
RUN dotnet restore "SmartRollCall.Api.csproj"

# 2. Copiar el código (el .dockerignore evitará que entre basura)
COPY . .

# 3. Publicar directamente
RUN dotnet publish "SmartRollCall.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# 4. Etapa Final
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmartRollCall.Api.dll"]