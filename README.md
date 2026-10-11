# Corporate Automation Lab

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

## Features

- Create, retrieve, update, and delete tasks through a REST API
- Manage tasks using a console application
- Persistent storage with SQL Server
- Task status and priority management
- Request validation and HTTP error handling
- Dependency injection for database access
- Automated unit and integration tests

## Project Structure

```text
Corporate-Automation-Lab/
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

The default configuration uses Windows authentication to connect to `.\SQLEXPRESS`.

### Installation

Clone the repository:

```powershell
git clone https://github.com/rafalitri/Corporate-Automation-Lab.git
cd Corporate-Automation-Lab
dotnet restore
```

Restore the local Entity Framework Core tool:

```powershell
dotnet tool restore
```

Create the database using the existing EF Core migration:

```powershell
dotnet ef database update --project src/CorporateAutomation.App --startup-project src/CorporateAutomation.App
```

If needed, adjust the connection string in `src/CorporateAutomation.Api/appsettings.json` and `src/CorporateAutomation.App/CorporateAutomationContext.cs` to match your local SQL Server instance.

### Run the Console Application

```powershell
dotnet run --project src/CorporateAutomation.App
```

### Run the Web API

Trust the local HTTPS development certificate:

```powershell
dotnet dev-certs https --trust
```

Start the API:

```powershell
dotnet run --project src/CorporateAutomation.Api --launch-profile https
```

API base URL:

`https://localhost:7217/api/tareas`

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

The test suite includes unit tests for task-management logic and integration tests for API endpoints and request validation.

Integration tests use isolated SQLite in-memory databases instead of the SQL Server development database.

## Purpose

This repository is a practical development project for applying C# and .NET concepts to business applications, database integration, REST API development, and automated testing.
