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

The test suite contains **27 automated tests** covering task-management logic, API endpoints, request validation, filtering, and pagination.

Integration tests use isolated SQLite in-memory databases instead of the SQL Server development database.

This allows tests to run without requiring SQL Server or local User Secrets.

## Continuous Integration

GitHub Actions automatically performs the following checks:

1. Restore .NET dependencies.
2. Build the solution.
3. Execute the automated tests.

The workflow runs on pull requests targeting `main` and pushes to `main`.

Build and test status is displayed in the badge at the top of this README.

## Purpose

Corporate Automation Lab is a practical software development project designed to demonstrate C# and .NET skills through a business-oriented application.

It focuses on REST API development, relational databases, separation of responsibilities, automated testing, and maintainable code.

The project also serves as a technical portfolio demonstrating modern .NET development practices.