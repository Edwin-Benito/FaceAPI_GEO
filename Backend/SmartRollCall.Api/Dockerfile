FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copiar solo el proyecto y restaurar
COPY SmartRollCall.Api.csproj .
RUN dotnet restore SmartRollCall.Api.csproj

# Copiar todo lo demás
COPY . .

# LIMPIEZA PREVIA Y PUBLICACIÓN
# Añadimos --no-restore porque ya lo hicimos arriba y evitamos conflictos
RUN dotnet publish "SmartRollCall.Api.csproj" -c Release -o /app/publish --no-restore /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "SmartRollCall.Api.dll"]