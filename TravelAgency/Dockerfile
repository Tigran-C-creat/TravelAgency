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
RUN dotnet restore "./TravelAgency/TravelAgency.csproj"
COPY . .
WORKDIR "/src/TravelAgency"
RUN dotnet build "./TravelAgency.csproj" -c $BUILD_CONFIGURATION -o /app/build

# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish
RUN dotnet publish "./TravelAgency.csproj" -c Release -o /app/publish /p:UseAppHost=false

# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
RUN apt-get update && apt-get install -y curl
ENV ASPNETCORE_ENVIRONMENT=Development
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TravelAgency.dll"]