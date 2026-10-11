# Corporate Automation Lab

[![.NET Tests](https://github.com/rafalitri/Corporate-Automation-Lab/actions/workflows/dotnet-tests.yml/badge.svg)](https://github.com/rafalitri/Corporate-Automation-Lab/actions/workflows/dotnet-tests.yml)

A C# and .NET 10 project focused on task management, REST API development, SQL Server persistence, and automated testing.

The project includes a console application and an ASP.NET Core Web API, both using Entity Framework Core for data access.

## Tech Stack

- C# / .NET 10
- ASP.NET Core Web API
- Entity Framework Core
- Microsoft SQL Server
- SQLite (in-memory integration tests)
- xUnit
- Git & GitHub
- GitHub Actions (CI)
- .NET User Secrets (local development configuration)

## Features

- Create, retrieve, update, and delete tasks through a REST API
- Manage tasks using a console application
- Persistent storage with SQL Server
- Task status and priority management
- Request validation and HTTP error handling
- Dependency injection for database access
- Automated unit and integration tests
- Automated builds and tests with GitHub Actions
- Local database configuration outside the repository using User Secrets

## Project Structure

```text
Corporate-Automation-Lab/
├── .github/
│   └── workflows/
│       └── dotnet-tests.yml
├── src/
│   ├── CorporateAutomation.App/
│   └── CorporateAutomation.Api/
├── tests/
│   └── CorporateAutomation.Tests/
├── CorporateAutomation.slnx
└── README.md
```

**CorporateAutomation.App** — Console application, task models, business logic, and Entity Framework Core.

**CorporateAutomation.Api** — ASP.NET Core REST API with CRUD endpoints and request validation.

**CorporateAutomation.Tests** — xUnit unit tests and HTTP integration tests using SQLite in memory.

## Getting Started

### Requirements

- .NET 10 SDK
- SQL Server Express with a `SQLEXPRESS` instance
- Git

The examples below use Windows authentication with a local `.\SQLEXPRESS` SQL Server instance.

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

Replace `.\SQLEXPRESS` if your SQL Server instance has a different name. These commands are intended for local development with Windows authentication.

`TrustServerCertificate=True` is intended for local development only. User Secrets are stored outside the repository, but they are **not encrypted** and are **not a production secrets manager**.

### Database Migrations

Restore the local Entity Framework Core CLI tool:

```powershell
dotnet tool restore
```

Create or update the SQL Server database using the existing EF Core migration:

```powershell
dotnet ef database update --project src/CorporateAutomation.App --startup-project src/CorporateAutomation.App
```

### Run the Console Application

```powershell
dotnet run --project src/CorporateAutomation.App
```

### Run the Web API

Trust the local HTTPS development certificate:

```powershell
dotnet dev-certs https --trust
```

Start the API using its HTTPS launch profile:

```powershell
dotnet run --project src/CorporateAutomation.Api --launch-profile https
```

API base URL: `https://localhost:7217/api/tareas`

## API Endpoints

| Method | Endpoint | Description |
|--------|----------|-------------|
| GET | `/api/tareas` | Retrieve all tasks |
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

Supported priorities: `Baja`, `Media`, `Alta`.

Supported task statuses: `Pendiente`, `Completada`.

Invalid requests return HTTP 400. Requests for nonexistent tasks return HTTP 404.

## Testing

Run all automated tests:

```powershell
dotnet test
```

The test suite contains 16 unit and integration tests covering task-management logic, API endpoints, and request validation.

Integration tests use isolated SQLite in-memory databases instead of the SQL Server development database, so they do not need SQL Server or local User Secrets.

GitHub Actions automatically restores dependencies, builds the solution, and runs the tests on pull requests targeting `main` and pushes to `main`.

## Purpose

This repository is a practical development project for applying C# and .NET concepts to business applications, database integration, REST API development, and automated testing.
