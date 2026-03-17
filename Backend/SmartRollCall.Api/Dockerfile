# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SmartRollCall.Api.csproj", "."]
RUN dotnet restore "SmartRollCall.Api.csproj"
COPY . .
RUN dotnet publish "SmartRollCall.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
COPY --from=build /app/publish .

# Exponer el puerto por el que escuchará Render
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "SmartRollCall.Api.dll"]
