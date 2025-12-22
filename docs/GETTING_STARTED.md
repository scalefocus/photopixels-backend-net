# Getting Started with PhotoPixels Backend

## Table of Contents
1. [Overview](#overview)
2. [Prerequisites](#prerequisites)
3. [Local Development Setup](#local-development-setup)
4. [First Steps](#first-steps)
5. [Troubleshooting](#troubleshooting)

---

## Overview

### What is PhotoPixels?

PhotoPixels is an open-source, self-hosted photo management API built with .NET 8. It provides users with the ability to:
- Sync photos and videos to a server
- Access, download, delete, and modify media files
- Organize media into albums
- Manage user accounts and authentication
- Upload large files using resumable uploads (TUS protocol)

### Technology Stack

- **Framework**: .NET 8 / ASP.NET Core
- **Database**: PostgreSQL with Marten (Document DB & Event Store)
- **Authentication**: JWT with Microsoft.Identity
- **Image Processing**: ImageSharp
- **Video Processing**: FFmpeg
- **Resumable Uploads**: SolidTUS (TUS Protocol)
- **Email**: MailKit
- **API Documentation**: Swagger/OpenAPI
- **Telemetry**: OpenTelemetry, Prometheus, Grafana, Tempo
- **Containerization**: Docker

---

## Prerequisites

### Required Software

- **.NET 8 SDK** or later - [Download](https://dotnet.microsoft.com/download/dotnet/8.0)
- **Docker Desktop** - [Download](https://www.docker.com/products/docker-desktop)
- **Git** - [Download](https://git-scm.com/downloads)
- **Visual Studio Code** or **Visual Studio 2022**

### Recommended Extensions (VS Code)

- **C# Dev Kit** - Microsoft
- **Docker** - Microsoft
- **REST Client** or **Thunder Client** - For API testing
- **GitLens** - For Git integration

### System Requirements

- **OS**: Windows 10/11, macOS, or Linux
- **RAM**: 8 GB minimum, 16 GB recommended
- **Storage**: 10 GB free space minimum

---

## Local Development Setup

### Method 1: Docker Compose (Recommended)

This is the quickest way to get started, as it sets up everything automatically.

#### 1. Clone the Repository

```powershell
git clone https://github.com/your-org/photopixels-backend-net.git
cd photopixels-backend-net
```

#### 2. Configure Environment Variables

Create a `.env` file in the root directory:

```powershell
Copy-Item .env.dev .env
```

Edit `.env` with your settings:

```env
# Docker image version
IMAGE_VERSION=latest

# Exposed ports
APP_PUBLIC_PORT=5000
DB_PUBLIC_PORT=5432

# Database credentials
DB_PASSWORD=YourSecurePassword123!

# Default admin account
ADMIN_EMAIL=admin@example.com
ADMIN_PASSWORD=Admin123!

# Photo storage location
PHOTOS_LOCATION=./photos
```

#### 3. Start the Application

```powershell
docker-compose up
```

Wait for all services to start. You should see:
```
web_1    | Now listening on: http://[::]:80
pgsql_1  | database system is ready to accept connections
```

#### 4. Access the Application

- **API**: http://localhost:5000
- **Swagger Documentation**: http://localhost:5000/swagger
- **PostgreSQL**: localhost:5432 (use credentials from `.env`)

#### 5. Stop the Application

```powershell
# Stop containers
docker-compose down

# Stop and remove all data (reset database)
docker-compose down -v
```

---

### Method 2: Local Development (Without Docker)

Use this method if you want to debug the application or make frequent code changes.

#### 1. Clone the Repository

```powershell
git clone https://github.com/your-org/photopixels-backend-net.git
cd photopixels-backend-net
```

#### 2. Install PostgreSQL

**Option A: Docker Container**
```powershell
docker run --name photopixels-postgres `
    -e POSTGRES_PASSWORD=postgres `
    -e POSTGRES_DB=photosdb `
    -p 5432:5432 `
    -d postgres:14.3
```

**Option B: Local Installation**
- Download and install [PostgreSQL 14+](https://www.postgresql.org/download/)
- Create a database named `photosdb`

#### 3. Configure Application Settings

Create or update `src/SF.PhotoPixels.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "PhotosMetadata": "Host=localhost;Port=5432;Database=photosdb;Username=postgres;Password=postgres"
  },
  "Admin": {
    "Email": "admin@example.com",
    "Password": "Admin123!"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "Telemetry": {
    "Enabled": false
  }
}
```

#### 4. Restore Dependencies

```powershell
dotnet restore
```

#### 5. Run Database Migrations

Migrations run automatically on application startup, but you can verify:

```powershell
cd src\SF.PhotoPixels.API
dotnet run
```

Watch for migration messages in the console output.

#### 6. Run the Application

```powershell
cd src\SF.PhotoPixels.API
dotnet run
```

Or use the VS Code debugger (F5).

#### 7. Access the Application

- **API**: https://localhost:7000 or http://localhost:5000
- **Swagger**: https://localhost:7000/swagger

---

## First Steps

### 1. Explore Swagger Documentation

Open http://localhost:5000/swagger in your browser.

You'll see all available API endpoints organized by category:
- **Authentication**: User login, registration, token management
- **User**: User management, quota adjustment
- **Photos**: Upload, download, delete photos
- **Album operations**: Create and manage albums
- **Tus**: Resumable upload operations

### 2. Create an Admin Account

The default admin account is created automatically on first startup using credentials from your configuration:
- Email: `admin@example.com`
- Password: `Admin123!`

### 3. Test Authentication

**Login Request**:
```http
POST http://localhost:5000/auth/login
Content-Type: application/json

{
  "email": "admin@example.com",
  "password": "Admin123!"
}
```

**Expected Response**:
```json
{
  "accessToken": "eyJhbGciOiJIUzI1NiIs...",
  "expiresIn": 600,
  "tokenType": "Bearer"
}
```

Copy the `accessToken` for subsequent requests.

### 4. Make an Authenticated Request

Add the token to the `Authorization` header:

```http
GET http://localhost:5000/user/profile
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
```

### 5. Upload Your First Photo

Use the Swagger UI or a REST client:

```http
POST http://localhost:5000/upload
Authorization: Bearer eyJhbGciOiJIUzI1NiIs...
Content-Type: multipart/form-data

file: (select your photo file)
```

---

## Development Workflow

### Using Visual Studio Code

1. **Open the workspace**: `File > Open Folder` → Select `photopixels-backend-net`
2. **Install recommended extensions** when prompted
3. **Press F5** to start debugging
4. **Set breakpoints** in your code
5. **Use the Debug Console** to inspect variables

### Using Visual Studio 2022

1. **Open solution**: `SF.PhotoPixels.sln`
2. **Set startup project**: Right-click `SF.PhotoPixels.API` → Set as Startup Project
3. **Press F5** to start debugging
4. **Use breakpoints and watch windows** for debugging

### Hot Reload

The application supports hot reload for quick iterations:

```powershell
cd src\SF.PhotoPixels.API
dotnet watch run
```

Changes to `.cs` files will automatically reload the application.

---

## Troubleshooting

### Docker Issues

**Problem**: "Port already in use"

**Solution**:
```powershell
# Check what's using the port
netstat -ano | findstr :5000

# Stop the process or change APP_PUBLIC_PORT in .env
```

**Problem**: "Cannot connect to Docker daemon"

**Solution**:
- Ensure Docker Desktop is running
- Restart Docker Desktop
- Check Docker settings → Resources

### Database Connection Issues

**Problem**: "Could not connect to PostgreSQL"

**Solution**:
```powershell
# Verify PostgreSQL is running
docker ps | findstr postgres

# Or check local PostgreSQL service
Get-Service postgresql*

# Test connection
psql -h localhost -U postgres -d photosdb
```

### Build Errors

**Problem**: "Project file does not exist"

**Solution**:
```powershell
# Restore NuGet packages
dotnet restore

# Clean and rebuild
dotnet clean
dotnet build
```

**Problem**: "Cannot find .NET SDK"

**Solution**:
```powershell
# Check installed SDKs
dotnet --list-sdks

# Install .NET 8 SDK if missing
# Download from https://dotnet.microsoft.com/download/dotnet/8.0
```

### Migration Errors

**Problem**: "Migration script failed"

**Solution**:
- Check PostgreSQL logs: `docker logs photopixels-postgres`
- Verify database exists: `psql -l`
- Reset database: `docker-compose down -v` then `docker-compose up`

### VS Code Issues

**Problem**: "C# IntelliSense not working"

**Solution**:
1. Install **C# Dev Kit** extension
2. Run `dotnet restore`
3. Reload window: `Ctrl+Shift+P` → "Reload Window"
4. Check OmniSharp logs: View → Output → OmniSharp Log

---

## Next Steps

Now that you have PhotoPixels running:

1. **Read the Architecture Guide** → [ARCHITECTURE.md](./ARCHITECTURE.md)
2. **Learn API Development** → [API_DEVELOPMENT.md](./API_DEVELOPMENT.md)
3. **Understand the Database** → [DATABASE.md](./DATABASE.md)
4. **Write Tests** → [TESTING.md](./TESTING.md)
5. **Deploy to Production** → [DEPLOYMENT.md](./DEPLOYMENT.md)

---

## Getting Help

- **Documentation**: Browse the `docs/` folder
- **GitHub Issues**: [Report bugs](https://github.com/your-org/photopixels-backend-net/issues)
- **Discussions**: [Ask questions](https://github.com/your-org/photopixels-backend-net/discussions)

---

**Last Updated**: December 2025  
**Version**: 1.0.0
