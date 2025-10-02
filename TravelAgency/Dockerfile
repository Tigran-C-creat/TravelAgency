# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443


# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["TravelAgency/TravelAgency.csproj", "TravelAgency/"]
COPY ["TravelAgency.Infrastructure/TravelAgency.Infrastructure.csproj", "TravelAgency.Infrastructure/"]
COPY ["TravelAgency.Application/TravelAgency.Application.csproj", "TravelAgency.Application/"]
COPY ["TravelAgency.Domain/TravelAgency.Domain.csproj", "TravelAgency.Domain/"]
COPY ["TravelAgency.Shared/TravelAgency.Shared.csproj", "TravelAgency.Shared/"]
COPY ["TravelAgency.Persistence/TravelAgency.Persistence.csproj", "TravelAgency.Persistence/"]

# Восстанавливаем зависимости
RUN dotnet restore "./TravelAgency/TravelAgency.csproj"

# Копируем весь исходный код
COPY . .

# Сборка проекта
WORKDIR "/src/TravelAgency"
RUN dotnet build "./TravelAgency.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Публикация проекта
FROM build AS publish
RUN dotnet publish "./TravelAgency.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Сборка проекта
FROM base AS final
WORKDIR /app

# Создаём безопасного пользователя
RUN useradd --home-dir /home/appuser --create-home --uid 1000 --user-group appuser \
    && chown -R appuser:appuser /app

# Копируем опубликованные файлы
COPY --from=publish /app/publish .

# Переключаемся на безопасного пользователя
USER appuser

# Точка входа
ENTRYPOINT ["dotnet", "TravelAgency.dll"]