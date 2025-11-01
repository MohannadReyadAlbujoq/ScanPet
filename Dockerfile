# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ["MobileBackend.sln", "./"]
COPY ["src/API/MobileBackend.API/MobileBackend.API.csproj", "src/API/MobileBackend.API/"]
COPY ["src/Application/MobileBackend.Application/MobileBackend.Application.csproj", "src/Application/MobileBackend.Application/"]
COPY ["src/Domain/MobileBackend.Domain/MobileBackend.Domain.csproj", "src/Domain/MobileBackend.Domain/"]
COPY ["src/Infrastructure/MobileBackend.Infrastructure/MobileBackend.Infrastructure.csproj", "src/Infrastructure/MobileBackend.Infrastructure/"]
COPY ["src/Framework/MobileBackend.Framework/MobileBackend.Framework.csproj", "src/Framework/MobileBackend.Framework/"]

# Restore dependencies
RUN dotnet restore "src/API/MobileBackend.API/MobileBackend.API.csproj"

# Copy everything else
COPY . .

# Build the application
WORKDIR "/src/src/API/MobileBackend.API"
RUN dotnet build "MobileBackend.API.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "MobileBackend.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Install curl for healthcheck
RUN apt-get update && apt-get install -y curl && rm -rf /var/lib/apt/lists/*

# Copy published application
COPY --from=publish /app/publish .

# Expose ports
EXPOSE 8080
EXPOSE 8081

# Environment variables
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl --fail http://localhost:8080/health || exit 1

# Run the application
ENTRYPOINT ["dotnet", "MobileBackend.API.dll"]
