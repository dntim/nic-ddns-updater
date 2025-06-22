<!-- Use this file to provide workspace-specific custom instructions to Copilot. For more details, visit https://code.visualstudio.com/docs/copilot/copilot-customization#_use-a-githubcopilotinstructionsmd-file -->

This project is a Docker-compatible .NET console application for updating DDNS records at NIC.RU. 

**Security Requirements:**
- Credentials (username/password) MUST ONLY be provided via Docker secrets or environment variables
- NEVER include credentials in configuration files or source code
- The project is published as `dntim/nic-ddns-updater` on Docker Hub

**Configuration:**
- Hostnames and update intervals (in seconds) are configurable via appsettings.json and environment variables
- Docker secrets are the preferred method for credentials in production
- Environment variables can be used for development/testing

**Docker Deployment:**
- Multi-architecture support: linux/amd64, linux/arm64, linux/arm/v7
- Optimized for Raspberry Pi, Intel/AMD x64, and ARM devices
- Published via GitHub Actions to Docker Hub as `dntim/nic-ddns-updater:latest`
