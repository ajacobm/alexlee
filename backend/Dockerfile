# Multi-stage build for .NET 8 API
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 5000
ENV ASPNETCORE_URLS=http://+:5000

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files for restore
COPY ["src/AlexLee.Api/AlexLee.Api.csproj", "src/AlexLee.Api/"]
COPY ["src/AlexLee.Application/AlexLee.Application.csproj", "src/AlexLee.Application/"]
COPY ["src/AlexLee.Domain/AlexLee.Domain.csproj", "src/AlexLee.Domain/"]
COPY ["src/AlexLee.Infrastructure/AlexLee.Infrastructure.csproj", "src/AlexLee.Infrastructure/"]
COPY ["src/AlexLee.Algorithms/AlexLee.Algorithms.csproj", "src/AlexLee.Algorithms/"]

# Restore dependencies
RUN dotnet restore "src/AlexLee.Api/AlexLee.Api.csproj"

# Copy source code
COPY . .
WORKDIR "/src/src/AlexLee.Api"

# Build the application
RUN dotnet build "AlexLee.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "AlexLee.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Create directory for SQLite database
RUN mkdir -p /app/data

# Set environment variables
ENV ConnectionStrings__DefaultConnection="Data Source=/app/data/alexlee.db"
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "AlexLee.Api.dll"]
