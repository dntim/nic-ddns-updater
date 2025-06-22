# NIC.RU DDNS Updater
## RU-Center DDNS Updater

A Docker-compatible .NET 9 console application for updating DDNS records at NIC.RU with multi-architecture support for various platforms.

## Features
- Periodically updates DDNS records for multiple hostnames at NIC.RU
- Configurable hostnames and update intervals
- **Secure credential management via Docker secrets only**
- **Multi-architecture support**: Intel/AMD x64, ARM64 (Raspberry Pi 4+), ARM32v7 (older Raspberry Pi)
- Optimized for containerized deployment across different platforms
- Includes GitHub Actions workflow for CI/CD and Docker image publishing
- Available as `dntim/nic-ddns-updater` on Docker Hub

## Security Model

**⚠️ Important Security Notice:**
- Credentials (username/password) are **ONLY** provided via Docker secrets or environment variables
- **NEVER** include credentials in configuration files or source code
- Configuration files are for hostnames and settings only

## Configuration

The application can be configured in multiple ways:

### Credentials (Required)
1. **Docker Secrets** (recommended for production)
2. **Environment Variables** (for development/testing only)

### Settings
- **Configuration File** (`appsettings.json`) - for hostnames and intervals
- **Environment Variables** - can override configuration file settings

| Setting | Description | Default | Environment Variable |
|---------|-------------|---------|---------------------|
| `DDNS:Hostnames` | Array of hostnames to update | `[]` | `DDNS__Hostnames__0`, `DDNS__Hostnames__1`, etc. |
| `DDNS:UpdateIntervalSeconds` | Update interval in seconds | `300` | `DDNS__UpdateIntervalSeconds` |

**Credentials:**
- Username: Docker secret `ddns_username` or env var `DDNS__USERNAME`
- Password: Docker secret `ddns_password` or env var `DDNS__PASSWORD`

## Usage

### 1. Using Pre-built Docker Image from Docker Hub

#### With Docker Secrets (Recommended for Production)

1. Create secrets directory and files:
   ```bash
   mkdir secrets
   echo "your_nic_ru_username" > secrets/ddns_username.txt
   echo "your_nic_ru_password" > secrets/ddns_password.txt
   ```

2. Create `docker-compose.yml`:
   ```yaml
   version: '3.8'
   
   services:
     ddns-updater:
       image: dntim/nic-ddns-updater:latest
       container_name: nic-ddns-updater
       restart: unless-stopped
       secrets:
         - ddns_username
         - ddns_password
       environment:
         - TZ=UTC
         - DDNS__Hostnames__0=example.com
         - DDNS__Hostnames__1=www.example.com
         - DDNS__Hostnames__2=api.example.com
         - DDNS__UpdateIntervalSeconds=300
   
   secrets:
     ddns_username:
       file: ./secrets/ddns_username.txt
     ddns_password:
       file: ./secrets/ddns_password.txt
   ```

3. Run the container:
   ```bash
   docker-compose up -d
   ```

#### With Environment Variables (Development/Testing Only)

```bash
docker run -d \
  --name nic-ddns-updater \
  --restart unless-stopped \
  -e DDNS__USERNAME=your_username \
  -e DDNS__PASSWORD=your_password \
  -e DDNS__Hostnames__0=example.com \
  -e DDNS__Hostnames__1=www.example.com \
  -e DDNS__Hostnames__2=api.example.com \
  -e DDNS__UpdateIntervalSeconds=300 \
  dntim/nic-ddns-updater:latest
```

#### With Custom Configuration File + Docker Secrets

1. Create your custom `appsettings.json`:
   ```json
   {
     "DDNS": {       "Hostnames": [
         "example.com",
         "www.example.com",
         "api.example.com"
       ],
       "UpdateIntervalSeconds": 600
     }
   }
   ```

2. Create secrets and run:
   ```bash
   mkdir secrets
   echo "your_username" > secrets/ddns_username.txt
   echo "your_password" > secrets/ddns_password.txt
   
   docker run -d \
     --name nic-ddns-updater \
     --restart unless-stopped \
     -v ./appsettings.json:/app/appsettings.json:ro \
     -v ./secrets/ddns_username.txt:/run/secrets/ddns_username:ro \
     -v ./secrets/ddns_password.txt:/run/secrets/ddns_password:ro \
     dntim/nic-ddns-updater:latest
   ```

### 2. Building from Source

1. Clone the repository:
   ```bash
   git clone https://github.com/dntim/nic-ddns-updater.git
   cd nic-ddns-updater
   ```

2. Build and run with Docker Compose:
   ```bash
   docker-compose up --build -d
   ```

### 3. Development Setup

1. Copy the example configuration:
   ```bash
   cp appsettings.example.json appsettings.json
   ```

2. Edit `appsettings.json` with your hostnames and settings

3. Provide credentials via environment variables:
   ```bash
   export DDNS__USERNAME=your_username
   export DDNS__PASSWORD=your_password
   ```

4. Run the application:
   ```bash
   dotnet run
   ```

## GitHub Actions

The included workflow (`.github/workflows/docker-image.yml`) builds and publishes multi-architecture Docker images to Docker Hub as `dntim/nic-ddns-updater:latest` on every push to `main`.

**Supported Architectures:**
- `linux/amd64` - Intel/AMD x64 systems
- `linux/arm64` - ARM64/AArch64 (Raspberry Pi 4+, Apple Silicon, AWS Graviton, etc.)

## Configuration Examples

### Multiple Hostnames via Environment Variables
```bash
DDNS__Hostnames__0=example.com
DDNS__Hostnames__1=www.example.com
DDNS__Hostnames__2=api.example.com
DDNS__Hostnames__3=mail.example.com
```

### Custom Update Interval
```bash
DDNS__UpdateIntervalSeconds=600  # Update every 10 minutes (600 seconds)
```

### Docker Compose with Environment Variables (Development Only)
```yaml
version: '3.8'

services:
  ddns-updater:
    image: dntim/nic-ddns-updater:latest
    environment:
      - DDNS__USERNAME=your_username
      - DDNS__PASSWORD=your_password      - DDNS__Hostnames__0=example.com
      - DDNS__Hostnames__1=www.example.com
      - DDNS__UpdateIntervalSeconds=300
```

## Platform Compatibility

The Docker image supports multiple architectures and can run on:

### Hardware Platforms
- **Intel/AMD x64**: Desktop computers, servers, cloud instances
- **ARM64/AArch64**: Raspberry Pi 4+, Apple Silicon Macs, AWS Graviton, Oracle Ampere

### Host Operating Systems
- **Linux**: Native Docker support
- **Windows**: Docker Desktop with WSL2 or Hyper-V
- **macOS**: Docker Desktop with virtualization
- **Cloud Platforms**: AWS, Azure, Google Cloud, DigitalOcean, etc.

### Container Runtimes
- Docker Engine
- Podman
- containerd
- Kubernetes (any distribution)

## Security Best Practices

- **ALWAYS** use Docker secrets for production deployments
- **NEVER** include credentials in configuration files or commit them to version control
- Use environment variables for development/testing only
- Regularly rotate your NIC.RU credentials
- The `appsettings.json` file is excluded from git to prevent accidental credential commits

## Monitoring

The application logs all DDNS update attempts with their results. Monitor the container logs:

```bash
docker logs -f nic-ddns-updater
```

## Note about GenAI

The whole solution has been developed using GitHub Copilot and Claude Sonnet 4 (Preview) model. Few manual edits have been made, mostly in the Readme file.

## License

MIT
