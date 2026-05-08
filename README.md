# ShiftPlanner API

A backend-focused workforce planning and load analysis system built with C# and .NET.

The purpose of the project is to simulate employee shift assignments, calculate workload scores, and identify potentially overloaded employees based on configurable thresholds.

The project is built using Clean Architecture principles and focuses on maintainability, separation of concerns, and testable business logic.

---

# Features

- Employee and shift domain models
- Shift assignment validation
- Workload score calculation
- High workload detection
- REST API endpoints
- Dependency Injection
- OpenAPI support
- Unit tested business logic

---

# Technologies

- C#
- .NET 9
- ASP.NET Core Web API
- xUnit
- Dependency Injection
- OpenAPI
- Clean Architecture

---

# Architecture

```text
API
↓
Application
↓
Domain
↓
Infrastructure
```

The Domain layer contains core business rules and entities.

The Application layer contains services and orchestration logic.

The API layer exposes REST endpoints.

Infrastructure will later contain database access and external integrations.

---

# API Endpoints

## Health Check

```http
GET /health
```

Returns API health status.

---

## Load Analysis

```http
POST /api/load-analysis
```

Example request:

```json
{
  "employeeName": "Kris",
  "skills": ["CT"],
  "threshold": 4,
  "shifts": [
    {
      "date": "2026-05-11",
      "shiftType": 0,
      "requiredSkill": "CT",
      "requiredStaff": 1,
      "assignEmployee": true
    }
  ]
}
```

Example response:

```json
{
  "employeeId": "guid",
  "employeeName": "Kris",
  "totalLoad": 5,
  "threshold": 4,
  "hasHighLoad": true
}
```

---

# Running the Project

```bash
dotnet build
dotnet test
dotnet run --project src/ShiftPlanner.Api
```

---

# Test Status

The solution includes unit tests covering:

- Shift staffing rules
- Skill validation
- Load score calculations
- Employee workload aggregation
- High load warning logic
- Analysis services

---

# Future Improvements

- Database integration with Entity Framework Core
- Authentication and authorization
- CSV shift import
- Real scheduling rules based on healthcare agreements
- Employee preference profiles
- Advanced workload algorithms
- Frontend dashboard