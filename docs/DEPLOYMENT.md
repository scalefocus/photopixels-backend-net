# Deployment Guide

This guide covers building, deploying, and running the PhotoPixels backend in production. Learn about Docker containerization, CI/CD pipeline, environment configuration, and production best practices.

---

## Table of Contents

1. [Overview](#overview)
2. [Docker Build](#docker-build)
3. [CI/CD Pipeline](#cicd-pipeline)
4. [Environment Configuration](#environment-configuration)
5. [Production Deployment](#production-deployment)
6. [Monitoring & Logging](#monitoring--logging)
7. [Troubleshooting](#troubleshooting)
8. [Production Checklist](#production-checklist)

---

## Overview

### Deployment Architecture

PhotoPixels backend is containerized with Docker and deployed via GitHub Actions CI/CD pipeline:

```
┌─────────────────────────────────────────────────────────────┐
│                     GitHub Repository                        │
│  ┌────────────┐  ┌──────────────┐  ┌─────────────────┐     │
│  │   Source   │  │  Dockerfile  │  │  GitHub Actions │     │
│  │    Code    │  │              │  │     Workflow    │     │
│  └────────────┘  └──────────────┘  └─────────────────┘     │
└────────────────────────────┬────────────────────────────────┘
                             │
                             ▼
                ┌────────────────────────┐
                │   GitHub Actions CI    │
                │  1. Build Docker Image │
                │  2. Run Tests          │
                │  3. Publish to GHCR    │
                └────────┬───────────────┘
                         │
                         ▼
          ┌──────────────────────────────┐
          │ GitHub Container Registry    │
          │  ghcr.io/owner/photopixels  │
          └──────────┬───────────────────┘
                     │
                     ▼
        ┌────────────────────────────┐
        │   Production Server        │
        │  1. Pull Image from GHCR   │
        │  2. Start with Docker      │
        │  3. Configure via Env Vars │
        └────────────────────────────┘
```

### Deployment Methods

1. **GitHub Actions** (Recommended): Automated CI/CD on push to `main`
2. **Manual Docker Build**: Build and run locally
3. **Docker Compose**: Local development with PostgreSQL

---

## Docker Build

### Dockerfile Overview

Located at: `photopixels-backend-net/Dockerfile`

**Multi-stage build**:
1. **Base**: Runtime dependencies
2. **Build**: Compile .NET application
3. **Publish**: Create optimized release build
4. **Final**: Production-ready image

```dockerfile
# Stage 1: Base runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Stage 2: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["src/SF.PhotoPixels.API/SF.PhotoPixels.API.csproj", "SF.PhotoPixels.API/"]
COPY ["src/SF.PhotoPixels.Application/SF.PhotoPixels.Application.csproj", "SF.PhotoPixels.Application/"]
COPY ["src/SF.PhotoPixels.Domain/SF.PhotoPixels.Domain.csproj", "SF.PhotoPixels.Domain/"]
COPY ["src/SF.PhotoPixels.Infrastructure/SF.PhotoPixels.Infrastructure.csproj", "SF.PhotoPixels.Infrastructure/"]

# Restore dependencies
RUN dotnet restore "SF.PhotoPixels.API/SF.PhotoPixels.API.csproj"

# Copy source code
COPY src/ .

# Build
WORKDIR "/src/SF.PhotoPixels.API"
RUN dotnet build "SF.PhotoPixels.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Stage 3: Publish
FROM build AS publish
RUN dotnet publish "SF.PhotoPixels.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Stage 4: Final production image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SF.PhotoPixels.API.dll"]
```

### Local Docker Build

```powershell
# Navigate to repository root
cd C:\Work\PhotoPixels\photopixels-backend-net

# Build image
docker build -t photopixels-api:latest .

# Run container
docker run -d `
  -p 8080:8080 `
  -e ConnectionStrings__PhotoPixels="Host=localhost;Database=photopixels;Username=postgres;Password=postgres" `
  -e Jwt__Secret="your-secret-key-here" `
  -e Jwt__Issuer="PhotoPixels" `
  -e Jwt__Audience="PhotoPixels" `
  --name photopixels-api `
  photopixels-api:latest

# View logs
docker logs -f photopixels-api

# Stop container
docker stop photopixels-api
docker rm photopixels-api
```

### Docker Compose (Development)

Located at: `docker-compose.yml`

```yaml
version: '3.8'

services:
  postgres:
    image: postgres:14
    container_name: photopixels-postgres
    environment:
      POSTGRES_DB: photopixels
      POSTGRES_USER: postgres
      POSTGRES_PASSWORD: postgres
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  api:
    build:
      context: .
      dockerfile: Dockerfile
    container_name: photopixels-api
    ports:
      - "8080:8080"
    environment:
      ConnectionStrings__PhotoPixels: "Host=postgres;Database=photopixels;Username=postgres;Password=postgres"
      Jwt__Secret: "dev-secret-key-change-in-production"
      Jwt__Issuer: "PhotoPixels"
      Jwt__Audience: "PhotoPixels"
    depends_on:
      - postgres

volumes:
  postgres_data:
```

**Run with Docker Compose**:

```powershell
# Start all services
docker-compose up -d

# View logs
docker-compose logs -f

# Stop all services
docker-compose down

# Stop and remove volumes (reset database)
docker-compose down -v
```

---

## CI/CD Pipeline

### GitHub Actions Workflow

Located at: `.github/workflows/build_to_publish.yml`

**Workflow Triggers**:
- Push to `main` branch
- Pull request to `main` branch
- Manual dispatch

**Workflow Steps**:

```yaml
name: Build and Publish

on:
  push:
    branches: [ main ]
  pull_request:
    branches: [ main ]
  workflow_dispatch:

jobs:
  build-and-publish:
    runs-on: ubuntu-latest
    
    permissions:
      contents: read
      packages: write

    steps:
      # 1. Checkout code
      - name: Checkout repository
        uses: actions/checkout@v4
        with:
          fetch-depth: 0  # Full history for GitVersion

      # 2. Setup .NET
      - name: Setup .NET
        uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '8.0.x'

      # 3. Install GitVersion
      - name: Install GitVersion
        uses: gittools/actions/gitversion/setup@v0
        with:
          versionSpec: '5.x'

      # 4. Determine version
      - name: Determine Version
        uses: gittools/actions/gitversion/execute@v0
        with:
          useConfigFile: true
          configFilePath: GitVersion.yml

      # 5. Restore dependencies
      - name: Restore dependencies
        run: dotnet restore src/SF.PhotoPixels.API/SF.PhotoPixels.API.csproj

      # 6. Build
      - name: Build
        run: dotnet build src/SF.PhotoPixels.API/SF.PhotoPixels.API.csproj --configuration Release --no-restore

      # 7. Run tests
      - name: Run tests
        run: dotnet test --configuration Release --no-build --verbosity normal

      # 8. Login to GitHub Container Registry
      - name: Login to GHCR
        if: github.event_name != 'pull_request'
        uses: docker/login-action@v3
        with:
          registry: ghcr.io
          username: ${{ github.actor }}
          password: ${{ secrets.GITHUB_TOKEN }}

      # 9. Build and push Docker image
      - name: Build and push Docker image
        if: github.event_name != 'pull_request'
        uses: docker/build-push-action@v5
        with:
          context: .
          file: ./Dockerfile
          push: true
          tags: |
            ghcr.io/${{ github.repository_owner }}/photopixels-api:latest
            ghcr.io/${{ github.repository_owner }}/photopixels-api:${{ env.GitVersion_SemVer }}
          labels: |
            org.opencontainers.image.source=${{ github.server_url }}/${{ github.repository }}
            org.opencontainers.image.version=${{ env.GitVersion_SemVer }}
```

### GitVersion Configuration

Located at: `GitVersion.yml`

```yaml
mode: ContinuousDeployment
branches:
  main:
    tag: ''
  develop:
    tag: 'beta'
  feature:
    tag: 'alpha'
  pull-request:
    tag: 'pr'
ignore:
  sha: []
```

**Versioning Strategy**:
- **main**: `1.0.0`, `1.0.1`, etc.
- **develop**: `1.1.0-beta.1`, `1.1.0-beta.2`, etc.
- **feature branches**: `1.2.0-alpha.1`, etc.
- **pull requests**: `1.2.0-pr.123`, etc.

### Viewing Build Status

1. Go to repository on GitHub
2. Click **Actions** tab
3. View workflow runs
4. Click on a run to see details and logs

### Manual Deployment Trigger

```powershell
# Trigger workflow manually from GitHub UI:
# 1. Go to Actions tab
# 2. Select "Build and Publish" workflow
# 3. Click "Run workflow"
# 4. Select branch
# 5. Click "Run workflow" button

# Or via GitHub CLI:
gh workflow run build_to_publish.yml --ref main
```

---

## Environment Configuration

### Required Environment Variables

| Variable | Description | Example |
|----------|-------------|---------|
| `ConnectionStrings__PhotoPixels` | PostgreSQL connection string | `Host=localhost;Database=photopixels;Username=postgres;Password=postgres` |
| `Jwt__Secret` | JWT signing secret (64+ chars) | `your-super-secret-key-that-is-at-least-64-characters-long` |
| `Jwt__Issuer` | JWT issuer claim | `PhotoPixels` |
| `Jwt__Audience` | JWT audience claim | `PhotoPixels` |
| `Jwt__ExpiryInMinutes` | Token expiration time | `60` (1 hour) |

### Optional Environment Variables

| Variable | Description | Default |
|----------|-------------|---------|
| `ASPNETCORE_ENVIRONMENT` | Environment name | `Production` |
| `ASPNETCORE_URLS` | URLs to listen on | `http://+:8080` |
| `Logging__LogLevel__Default` | Default log level | `Information` |
| `AllowedHosts` | Allowed host headers | `*` |

### appsettings.json

Located at: `src/SF.PhotoPixels.API/appsettings.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*",
  "ConnectionStrings": {
    "PhotoPixels": "Host=localhost;Database=photopixels;Username=postgres;Password=postgres"
  },
  "Jwt": {
    "Secret": "dev-secret-key-change-in-production",
    "Issuer": "PhotoPixels",
    "Audience": "PhotoPixels",
    "ExpiryInMinutes": 60
  }
}
```

### appsettings.Production.json

Located at: `src/SF.PhotoPixels.API/appsettings.Production.json`

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning",
      "SF.PhotoPixels": "Information"
    }
  }
}
```

### Environment Variable Override

Environment variables override `appsettings.json`:

```powershell
# Windows (PowerShell)
$env:ConnectionStrings__PhotoPixels = "Host=prod-db;Database=photopixels;..."
$env:Jwt__Secret = "production-secret-key-here"

# Linux/macOS
export ConnectionStrings__PhotoPixels="Host=prod-db;Database=photopixels;..."
export Jwt__Secret="production-secret-key-here"

# Docker
docker run -e ConnectionStrings__PhotoPixels="..." -e Jwt__Secret="..." ...
```

---

## Production Deployment

### Deployment Steps

#### 1. Pull Latest Image

```bash
# Login to GitHub Container Registry
echo $GITHUB_TOKEN | docker login ghcr.io -u USERNAME --password-stdin

# Pull latest image
docker pull ghcr.io/OWNER/photopixels-api:latest
```

#### 2. Create Environment File

Create `.env` file with production configuration:

```env
ConnectionStrings__PhotoPixels=Host=prod-db.example.com;Port=5432;Database=photopixels;Username=photopixels_user;Password=strong-password-here
Jwt__Secret=your-production-secret-key-at-least-64-characters-long-random-string
Jwt__Issuer=PhotoPixels
Jwt__Audience=PhotoPixels
Jwt__ExpiryInMinutes=60
ASPNETCORE_ENVIRONMENT=Production
ASPNETCORE_URLS=http://+:8080
```

#### 3. Run Container

```bash
# Run with environment file
docker run -d \
  --name photopixels-api \
  --env-file .env \
  -p 8080:8080 \
  --restart unless-stopped \
  ghcr.io/OWNER/photopixels-api:latest

# Verify it's running
docker ps | grep photopixels-api

# Check logs
docker logs photopixels-api
```

#### 4. Setup Reverse Proxy (Nginx)

**Nginx configuration** (`/etc/nginx/sites-available/photopixels-api`):

```nginx
server {
    listen 80;
    server_name api.photopixels.example.com;

    # Redirect HTTP to HTTPS
    return 301 https://$host$request_uri;
}

server {
    listen 443 ssl http2;
    server_name api.photopixels.example.com;

    # SSL certificates (use Let's Encrypt)
    ssl_certificate /etc/letsencrypt/live/api.photopixels.example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/api.photopixels.example.com/privkey.pem;

    # Security headers
    add_header X-Frame-Options "SAMEORIGIN" always;
    add_header X-Content-Type-Options "nosniff" always;
    add_header X-XSS-Protection "1; mode=block" always;

    # Client max body size (for photo uploads)
    client_max_body_size 100M;

    location / {
        proxy_pass http://localhost:8080;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_cache_bypass $http_upgrade;
    }
}
```

Enable site:
```bash
sudo ln -s /etc/nginx/sites-available/photopixels-api /etc/nginx/sites-enabled/
sudo nginx -t
sudo systemctl reload nginx
```

#### 5. SSL Certificate (Let's Encrypt)

```bash
# Install certbot
sudo apt install certbot python3-certbot-nginx

# Obtain certificate
sudo certbot --nginx -d api.photopixels.example.com

# Auto-renewal is configured automatically
# Test renewal:
sudo certbot renew --dry-run
```

### Database Migration

Production database setup:

```bash
# Connect to PostgreSQL
psql -h prod-db.example.com -U postgres

# Create database and user
CREATE DATABASE photopixels;
CREATE USER photopixels_user WITH PASSWORD 'strong-password';
GRANT ALL PRIVILEGES ON DATABASE photopixels TO photopixels_user;

# Exit psql
\q
```

**Migrations run automatically** on app startup via `DependencyInjection.cs`.

### Zero-Downtime Deployment

Use **blue-green deployment**:

```bash
# Current container: photopixels-api-blue (running on port 8080)

# Pull new image
docker pull ghcr.io/OWNER/photopixels-api:latest

# Start green container on different port
docker run -d \
  --name photopixels-api-green \
  --env-file .env \
  -e ASPNETCORE_URLS=http://+:8081 \
  -p 8081:8081 \
  --restart unless-stopped \
  ghcr.io/OWNER/photopixels-api:latest

# Test green container
curl http://localhost:8081/health

# Switch Nginx to green container
# Update proxy_pass to http://localhost:8081
sudo nano /etc/nginx/sites-available/photopixels-api
sudo nginx -t
sudo systemctl reload nginx

# Stop blue container
docker stop photopixels-api-blue
docker rm photopixels-api-blue

# Rename green to blue for next deployment
docker rename photopixels-api-green photopixels-api-blue
```

---

## Monitoring & Logging

### Health Check Endpoint

```bash
# Check API health
curl https://api.photopixels.example.com/health

# Expected response:
# HTTP 200 OK
# "Healthy"
```

### Application Logs

```bash
# View live logs
docker logs -f photopixels-api

# Last 100 lines
docker logs --tail 100 photopixels-api

# Since timestamp
docker logs --since 2024-01-01T00:00:00 photopixels-api

# Save logs to file
docker logs photopixels-api > app.log 2>&1
```

### Structured Logging

Application uses **Serilog** for structured logging:

```json
{
  "Timestamp": "2024-01-01T12:00:00.000Z",
  "Level": "Information",
  "MessageTemplate": "User {UserId} uploaded photo {ObjectId}",
  "Properties": {
    "UserId": "550e8400-e29b-41d4-a716-446655440000",
    "ObjectId": "660e8400-e29b-41d4-a716-446655440001",
    "SourceContext": "SF.PhotoPixels.Application.Commands.UploadPhotoHandler"
  }
}
```

### Log Aggregation (Optional)

**Send logs to external service** (e.g., Seq, ELK, Datadog):

```csharp
// Program.cs
builder.Host.UseSerilog((context, config) =>
{
    config
        .ReadFrom.Configuration(context.Configuration)
        .WriteTo.Console()
        .WriteTo.Seq("https://seq.example.com", apiKey: "YOUR_API_KEY");
});
```

### Metrics & Performance Monitoring

Consider integrating:
- **Application Insights** (Azure)
- **Prometheus + Grafana**
- **Datadog APM**

Example: Add Application Insights

```csharp
// Program.cs
builder.Services.AddApplicationInsightsTelemetry(options =>
{
    options.ConnectionString = builder.Configuration["ApplicationInsights:ConnectionString"];
});
```

---

## Troubleshooting

### Container Won't Start

```bash
# Check container status
docker ps -a | grep photopixels

# View container logs
docker logs photopixels-api

# Inspect container
docker inspect photopixels-api

# Common issues:
# 1. Missing environment variables
# 2. Cannot connect to database
# 3. Port already in use
```

### Database Connection Errors

```bash
# Test database connectivity from container
docker exec -it photopixels-api /bin/bash
apt update && apt install -y postgresql-client
psql -h prod-db.example.com -U photopixels_user -d photopixels

# Common issues:
# 1. Incorrect connection string
# 2. Firewall blocking port 5432
# 3. Database not created
# 4. User lacks permissions
```

### Migration Failures

```bash
# View migration logs
docker logs photopixels-api | grep -i migration

# Common issues:
# 1. Duplicate migration file names
# 2. SQL syntax errors
# 3. Missing schema permissions

# Rollback manually if needed
psql -h prod-db -U photopixels_user -d photopixels
# Run rollback script from Migrations/Rollback/
```

### High Memory Usage

```bash
# Check container resource usage
docker stats photopixels-api

# Limit container memory
docker run --memory="512m" --memory-swap="1g" ...
```

### 502 Bad Gateway (Nginx)

```bash
# Check if API is running
docker ps | grep photopixels-api

# Check API port binding
docker port photopixels-api

# Test API directly
curl http://localhost:8080/health

# Check Nginx error logs
sudo tail -f /var/log/nginx/error.log
```

---

## Production Checklist

### Pre-Deployment

- [ ] Update `appsettings.Production.json` with production settings
- [ ] Generate strong JWT secret (64+ characters)
- [ ] Setup production PostgreSQL database
- [ ] Configure database backups
- [ ] Setup SSL certificate (Let's Encrypt)
- [ ] Configure firewall rules
- [ ] Test Docker image locally
- [ ] Review and test migrations
- [ ] Setup monitoring and alerting

### Security

- [ ] Use strong, unique JWT secret
- [ ] Enable HTTPS only
- [ ] Configure CORS properly
- [ ] Use environment variables (never commit secrets)
- [ ] Limit database user permissions
- [ ] Enable request size limits
- [ ] Configure rate limiting (if needed)
- [ ] Review security headers in Nginx

### Performance

- [ ] Configure database connection pooling
- [ ] Enable response compression
- [ ] Setup CDN for static assets (if any)
- [ ] Configure caching headers
- [ ] Optimize database indexes
- [ ] Monitor query performance

### Reliability

- [ ] Setup container auto-restart (`--restart unless-stopped`)
- [ ] Configure health checks
- [ ] Setup log rotation
- [ ] Plan for database backups
- [ ] Document rollback procedure
- [ ] Setup monitoring alerts
- [ ] Test disaster recovery

### Post-Deployment

- [ ] Verify all endpoints are accessible
- [ ] Test authentication flow
- [ ] Upload test photo
- [ ] Create test album
- [ ] Check application logs
- [ ] Monitor resource usage
- [ ] Verify database migrations applied
- [ ] Test health check endpoint
- [ ] Confirm SSL certificate is valid

---

## Additional Resources

- [Docker Documentation](https://docs.docker.com/)
- [GitHub Actions Documentation](https://docs.github.com/en/actions)
- [Nginx Documentation](https://nginx.org/en/docs/)
- [Let's Encrypt](https://letsencrypt.org/)
- [PostgreSQL Production Checklist](https://www.postgresql.org/docs/current/app-initdb.html)

---

**Next Steps**:
- Review [Getting Started](./GETTING_STARTED.md) for local development setup
- See [Database Guide](./DATABASE.md) for migration management
- Check [Troubleshooting Guide](./TROUBLESHOOTING.md) for common production issues

---

**Last Updated**: December 2024  
**Version**: 1.0.0
