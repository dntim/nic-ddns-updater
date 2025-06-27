# Multi-stage build for multiple architectures
FROM mcr.microsoft.com/dotnet/sdk:9.0-bookworm-slim AS build

# Set working directory
WORKDIR /src

# Copy project files
COPY *.csproj ./
RUN dotnet restore

# Copy source code
COPY . .

# Build and publish the application (framework-dependent, portable across architectures)
RUN dotnet publish NicDDNSUpdater.csproj -c Release -o /app/publish --no-restore --no-self-contained

# Final stage - runtime image (automatically pulls correct architecture)
FROM mcr.microsoft.com/dotnet/runtime:9.0-bookworm-slim AS runtime

# Set working directory
WORKDIR /app

# Copy the published application
COPY --from=build /app/publish .

# Create a non-root user for security
RUN adduser --disabled-password --gecos '' appuser && chown -R appuser /app
USER appuser

# Set the entry point
ENTRYPOINT ["dotnet", "NicDDNSUpdater.dll"]
