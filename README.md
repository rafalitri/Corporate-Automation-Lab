# Corporate Automation Lab

[![.NET Tests](https://github.com/rafalitri/Corporate-Automation-Lab/actions/workflows/dotnet-tests.yml/badge.svg)](https://github.com/rafalitri/Corporate-Automation-Lab/actions/workflows/dotnet-tests.yml)

A C# and .NET 10 project focused on task management, REST API development, SQL Server persistence, clean architecture principles, and automated testing.

The solution includes a console application and an ASP.NET Core Web API, both sharing reusable domain models and database infrastructure.

## Tech Stack

- C# / .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- Microsoft SQL Server
- SQLite (in-memory integration tests)
- xUnit
- Scalar / OpenAPI
- Git & GitHub
- GitHub Actions (CI)
- .NET User Secrets
- Docker & Docker Compose
- GitHub Container Registry (GHCR)

## Features

- Create, retrieve, update, and delete tasks through a REST API
- Manage tasks using a console application
- Persistent storage with SQL Server
- Task status and priority management
- Request validation and HTTP status codes
- Filtering and pagination
- Asynchronous API operations
- Structured application logging
- Dependency injection for database access
- Interactive API documentation with Scalar
- Automated unit and integration tests
- Automated builds and tests with GitHub Actions
- Local database configuration using User Secrets
- Separation of domain logic, infrastructure, and application layers
- Centralized exception handling with ProblemDetails
- JWT Bearer authentication for protected API endpoints
- JWT signature and expiration validation
- API and database health checks with Docker monitoring

## Project Structure

```text
Corporate-Automation-Lab/
├── .github/
│   └── workflows/
│       └── dotnet-tests.yml
├── src/
│   ├── CorporateAutomation.Core/
│   ├── CorporateAutomation.Infrastructure/
│   ├── CorporateAutomation.App/
│   └── CorporateAutomation.Api/
├── tests/
│   └── CorporateAutomation.Tests/
├── CorporateAutomation.slnx
└── README.md
```

### Architecture

**CorporateAutomation.Core** — Shared domain models, enumerations, and business logic.

**CorporateAutomation.Infrastructure** — Entity Framework Core, SQL Server persistence, database context, and migrations.

**CorporateAutomation.App** — Console application for managing tasks.

**CorporateAutomation.Api** — ASP.NET Core REST API with CRUD endpoints, request validation, logging, and Scalar documentation.

**CorporateAutomation.Tests** — xUnit unit tests and HTTP integration tests using SQLite in memory.

The console application and REST API share Core and Infrastructure without depending on each other.

## Getting Started

### Requirements

- .NET 10 SDK
- SQL Server Express with a `SQLEXPRESS` instance
- Git

The following examples use Windows authentication with a local `.\SQLEXPRESS` SQL Server instance.

### Installation

Clone the repository and restore its dependencies:

```powershell
git clone https://github.com/rafalitri/Corporate-Automation-Lab.git
cd Corporate-Automation-Lab
dotnet restore
```

### Configure Database Connection

Both the console application and the Web API use .NET User Secrets to keep local database connection strings outside the Git repository.

Configure the console application:

```powershell
dotnet user-secrets set "ConnectionStrings:CorporateAutomation" "Server=.\SQLEXPRESS;Database=CorporateAutomation;Trusted_Connection=True;TrustServerCertificate=True;" --project src/CorporateAutomation.App
```

Configure the Web API:

```powershell
dotnet user-secrets set "ConnectionStrings:CorporateAutomation" "Server=.\SQLEXPRESS;Database=CorporateAutomation;Trusted_Connection=True;TrustServerCertificate=True;" --project src/CorporateAutomation.Api
```

Replace `.\SQLEXPRESS` if your SQL Server instance has a different name.

These commands are intended for local development with Windows authentication.

`TrustServerCertificate=True` is intended for local development only. User Secrets are stored outside the repository, but they are not encrypted and are not suitable as a production secrets manager.

### Database Migrations

Restore the local Entity Framework Core CLI tool:

```powershell
dotnet tool restore
```

Create or update the SQL Server database using the existing EF Core migration:

```powershell
dotnet ef database update --project src/CorporateAutomation.Infrastructure --startup-project src/CorporateAutomation.App
```

The database context and migrations are maintained in `CorporateAutomation.Infrastructure`.

### Run the Console Application

```powershell
dotnet run --project src/CorporateAutomation.App
```

The console application supports creating, listing, searching, and completing tasks.

### Run the Web API

Trust the local HTTPS development certificate:

```powershell
dotnet dev-certs https --trust
```

Start the API using its HTTPS launch profile:

```powershell
dotnet run --project src/CorporateAutomation.Api --launch-profile https
```

API base URL:

`https://localhost:7217/api/tareas`


## Docker

The project supports containerized local development using
Docker and Docker Compose.

Docker Compose runs two separate services:

- **API:** ASP.NET Core 10 application running on Linux.
- **Database:** SQL Server 2022 with persistent storage.

The API uses a multi-stage Dockerfile and runs as a non-root user.

### Requirements

- Docker Desktop with Linux containers
- Docker Compose

### Configuration

Create a `.env` file in the repository root with:

- `MSSQL_SA_PASSWORD`: Strong SQL Server password.
- `JWT_KEY`: Base64-encoded random 256-bit key.
- `DEV_TOKEN_PASSWORD`: Random development credential.

The `.env` file is ignored by Git and must never be committed.

### Start the Database

```powershell
docker compose up -d sqlserver
docker compose ps
```

Wait until SQL Server reports a healthy status.


### Database Initialization

On first use, apply the existing Entity Framework Core migration
to the SQL Server container.

Start SQL Server:

```powershell
docker compose up -d sqlserver
```

In PowerShell, load the SQL Server password from the local
`.env` file and temporarily configure the database connection:

```powershell
$linea = Get-Content .env |
    Where-Object { $_ -like "MSSQL_SA_PASSWORD=*" } |
    Select-Object -First 1

if (-not $linea) {
    throw "MSSQL_SA_PASSWORD not found in .env"
}

$password = $linea.Substring("MSSQL_SA_PASSWORD=".Length)

$env:ConnectionStrings__CorporateAutomation = "Server=127.0.0.1,14330;Database=CorporateAutomation;User Id=sa;Password=$password;Encrypt=True;TrustServerCertificate=True"

try {
    dotnet ef database update --project src/CorporateAutomation.Infrastructure --startup-project src/CorporateAutomation.Api

    if ($LASTEXITCODE -ne 0) {
        throw "Database migration failed"
    }
}
finally {
    Remove-Item Env:\ConnectionStrings__CorporateAutomation -ErrorAction SilentlyContinue
    Remove-Variable linea, password -ErrorAction SilentlyContinue
}
```

This requires the .NET SDK and the EF Core CLI tool.
Restore the local tool with `dotnet tool restore` if necessary.

The migration creates the database and its tables in Docker,
without modifying the local SQL Server Express instance.

The Docker SQL Server instance is accessible at
`127.0.0.1,14330`.


### Start the Application

```powershell
docker compose up -d --build
```

Once the containers are running, access:

- API: http://localhost:8080/api/tareas
- Scalar: http://localhost:8080/scalar

The services are bound to localhost for local development.


### Published Docker Image

The API Docker image is automatically published to
GitHub Container Registry (GHCR) after successful
CI validation on the `main` branch.

**Container image:**

`ghcr.io/rafalitri/corporate-automation-lab`

Download the latest published version:

```powershell
docker pull ghcr.io/rafalitri/corporate-automation-lab:latest
```

Images are tagged with both `latest` and the Git commit SHA,
allowing specific builds to be identified and retrieved.

The published image contains the API only. It requires
a configured database connection and is not a complete
standalone deployment. For local development, use the
Docker Compose instructions above.


### Health Checks

The API provides a health check endpoint:

`GET /health`

This endpoint checks database connectivity using Entity Framework Core.

Expected responses:

- `200 OK` — `Healthy`: the database is accessible.
- `503 Service Unavailable` — `Unhealthy`: the database is unavailable.

Docker Compose monitors both the API and SQL Server.

The API health check runs every 15 seconds and marks the
container as unhealthy after repeated failures.

Check the container status with:

`docker compose ps`

Docker health checks detect problems but do not
automatically restart unhealthy containers.


### Persistent Storage

SQL Server uses a named Docker volume to preserve database data.

```powershell
docker compose down
docker compose up -d
```

The database remains available after recreating the containers.

**Warning:** Running `docker compose down -v` removes
the database volume and its stored data.

### Security

Docker Compose uses the Development environment for local testing.

The development-only JWT token endpoint is not intended for
public deployment or production authentication.
```


### Interactive API Documentation

The project includes Scalar, an interactive OpenAPI interface for exploring and testing REST endpoints.

Start the Web API and open:

**https://localhost:7217/scalar**

Scalar is available only in the Development environment.

The API documentation includes endpoint descriptions, request schemas, and expected HTTP response codes.

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tareas` | Retrieve tasks with optional filters and pagination |
| GET | `/api/tareas/{id}` | Retrieve a task by ID |
| POST | `/api/tareas` | Create a task |
| PUT | `/api/tareas/{id}` | Update a task |
| DELETE | `/api/tareas/{id}` | Delete a task |


### JWT Authentication

The API supports JWT Bearer authentication using HS256.

Currently, the DELETE endpoint requires a valid JWT.
Other task endpoints remain publicly accessible.

JWT validation includes:
- Digital signature verification
- Issuer and audience validation
- Token expiration validation

Signing keys and development credentials are stored
locally using .NET User Secrets.

### Development Token Generation

For local testing, the API provides:

POST /api/dev/token

This endpoint is only registered in the Development
environment and requires a separate development credential
configured through User Secrets.

Generated tokens expire after 15 minutes.

The endpoint is not available in Production.

This is a development-only testing mechanism, not a
production user login system.

### Authentication Tests

Automated integration tests verify:
- Requests without authentication
- Requests with valid JWTs
- Expired tokens
- Invalid token signatures
- Incorrect development credentials
- Successful development token generation
- Endpoint unavailability in Production


### Example: Create a Task

**POST** `/api/tareas`

```json
{
  "nombre": "Review system backups",
  "prioridad": "Alta"
}
```

A successful request returns HTTP `201 Created` with a `Location` header pointing to the new resource.

Example response:

```json
{
  "id": 1,
  "nombre": "Review system backups",
  "estado": "Pendiente",
  "prioridad": "Alta"
}
```

Supported priorities: `Baja`, `Media`, `Alta`.

Supported task statuses: `Pendiente`, `Completada`.

### Filtering and Pagination

The GET endpoint supports optional query parameters:

- `estado`: `Pendiente` or `Completada`
- `prioridad`: `Baja`, `Media`, or `Alta`
- `pagina`: Page number (default: 1)
- `tamanoPagina`: Results per page (default: 10, maximum: 100)

Example:

```http
GET /api/tareas?estado=Pendiente&prioridad=Alta&pagina=1&tamanoPagina=10
```

The response includes an `X-Total-Count` header containing the total number of matching tasks before pagination.

### HTTP Status Codes

| Status | Description |
|--------|-------------|
| 200 OK | Successful retrieval or update |
| 201 Created | Task created successfully |
| 204 No Content | Task deleted successfully |
| 400 Bad Request | Invalid request or parameters |
| 404 Not Found | Requested task does not exist |

## Testing

Run all automated tests:

```powershell
dotnet test
```

The test suite contains **38 automated tests** covering task-management logic, API endpoints, request validation, filtering, and pagination.

Integration tests use isolated SQLite in-memory databases instead of the SQL Server development database.

This allows tests to run without requiring SQL Server or local User Secrets.


## CI/CD Pipeline

GitHub Actions automatically validates the project
on pull requests and pushes to `main`.

The pipeline includes four jobs:

1. **build-and-test:** Restore dependencies, compile the
   .NET solution, and run 38 automated tests.
2. **docker-build:** Build the API Docker image.
3. **docker-integration:** Start SQL Server, apply EF Core
   migrations, check API health, and verify task creation
   and retrieval against a real database.
4. **publish-ghcr:** Publish the validated Docker image
   to GitHub Container Registry.

The publishing job runs only on successful pushes to
`main`, after the three validation jobs have passed.

Temporary credentials are generated for integration
tests, and test containers and volumes are removed
after execution.

The published images are publicly available through
GitHub Container Registry.

[View GitHub Actions](https://github.com/rafalitri/Corporate-Automation-Lab/actions)


## Purpose

Corporate Automation Lab is a practical software development project designed to demonstrate C# and .NET skills through a business-oriented application.

It focuses on REST API development, relational databases, separation of responsibilities, automated testing, and maintainable code.

The project also serves as a technical portfolio demonstrating modern .NET development practices.