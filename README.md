# NIC.RU DDNS Updater
## RU-Center DDNS Updater

A Docker-compatible .NET 9 console application for updating DDNS records at NIC.RU with multi-architecture support.

## Features
- Periodically updates DDNS records for multiple hostnames at NIC.RU
- **Secure credential management via Docker secrets or environment variables**
- **Multi-architecture support**: linux/amd64, linux/arm64, linux/arm/v7
- Available as `dntim/nic-ddns-updater` on Docker Hub

## Security Model

**⚠️ Important:** Credentials are provided via Docker secrets (typically recommended for production) or environment variables. Never include credentials in configuration files.

## Configuration

| Setting | Description | Default | Environment Variable |
|---------|-------------|---------|---------------------|
| Username | NIC.RU DDNS username | - | `DDNS__USERNAME` or Docker secret `ddns_username` |
| Password | NIC.RU DDNS password | - | `DDNS__PASSWORD` or Docker secret `ddns_password` |
| Hostnames | Array of hostnames to update | `[]` | `DDNS__Hostnames__0`, `DDNS__Hostnames__1`, etc. |
| Update Interval | Update interval in seconds | `300` | `DDNS__UpdateIntervalSeconds` |

## Quick Start

### Option 1: Docker Swarm Stack with Secrets

1. Initialize Docker Swarm and create secrets:
   ```bash
   docker swarm init
   echo "your_username" | docker secret create ddns_username -
   echo "your_password" | docker secret create ddns_password -
   ```

2. Deploy with Docker Compose stack:
   ```bash
   # Edit docker-compose.yml to configure your hostnames
   docker stack deploy -c docker-compose.yml ddns
   ```

   Or deploy with Docker service directly:
   ```bash
   docker service create \
     --name nic-ddns-updater \
     --secret ddns_username \
     --secret ddns_password \
     --env DDNS__Hostnames__0=example.com \
     --env DDNS__Hostnames__1=www.example.com \
     --env DDNS__UpdateIntervalSeconds=300 \
     --restart-condition on-failure \
     dntim/nic-ddns-updater:latest
   ```

### Option 2: Docker Compose with Environment Variables

1. Edit `docker-compose.yml`:
   - Comment out the `secrets` section
   - Uncomment the username/password environment variables
   - Optionally uncomment the Watchtower service for automatic updates

2. Deploy with regular Docker Compose:
   ```bash
   docker-compose up -d
   ```

   Or deploy with simple Docker run:
   ```bash
   docker run -d \
     --name nic-ddns-updater \
     --restart unless-stopped \
     -e DDNS__USERNAME=your_username \
     -e DDNS__PASSWORD=your_password \
     -e DDNS__Hostnames__0=example.com \
     -e DDNS__Hostnames__1=www.example.com \
     -e DDNS__UpdateIntervalSeconds=300 \
     dntim/nic-ddns-updater:latest
   ```

## Monitoring

View logs:
```bash
# For Docker Swarm stack
docker service logs -f ddns_ddns-updater

# For Docker Compose
docker-compose logs -f ddns-updater

# For regular container
docker logs -f nic-ddns-updater
```

## Managing Secrets

```bash
# List secrets
docker secret ls

# Rotate credentials
docker secret rm ddns_username ddns_password
echo "new_username" | docker secret create ddns_username -
echo "new_password" | docker secret create ddns_password -
docker service update --force ddns_ddns-updater
```

## Platform Support

**Architectures:** linux/amd64, linux/arm64, linux/arm/v7
**Platforms:** Intel/AMD x64, ARM64 (Raspberry Pi 4+), ARMv7
**Runtimes:** Docker, Podman, containerd, Kubernetes

## CI/CD

GitHub Actions automatically builds and publishes multi-architecture images to Docker Hub as `dntim/nic-ddns-updater:latest` on every push to `main`.

## License

MIT

---
*Developed using GitHub Copilot and Claude Sonnet 4 (Preview)*