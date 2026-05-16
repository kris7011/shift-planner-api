# ShiftPlanner API

A backend-focused workforce planning and load analysis system built with C# and .NET.

The project simulates healthcare-oriented workforce scheduling by combining:

- employee skills
- shift requirements
- workload scoring
- overload prevention
- automatic schedule generation

The solution is built using Clean Architecture principles with strong focus on:

- maintainability
- separation of concerns
- testable business logic
- scalable backend architecture

---

# Features

- Employee persistence with EF Core and SQLite
- Shift persistence with employee assignment
- Employee and shift domain models
- Shift assignment validation
- Workload score calculation
- Employee overload detection
- Employee load analysis endpoint
- Schedule generation endpoint
- Skill-based automatic assignment
- Lowest-load employee selection
- High-load prevention during schedule generation
- Persisted generated assignments
- REST API endpoints
- Dependency Injection
- OpenAPI support
- Unit tested business logic

---

# Technologies

- C#
- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
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

## Layer Responsibilities

### Domain

Contains:

- entities
- enums
- business rules
- core scheduling logic

The Domain layer has no external dependencies.

---

### Application

Contains:

- orchestration logic
- scheduling services
- workload calculation services
- overload detection services

---

### Infrastructure

Contains:

- Entity Framework Core persistence
- SQLite database access
- repository implementations

---

### API

Contains:

- REST endpoints
- request/response contracts
- dependency injection setup
- middleware configuration

---

# API Endpoints

## Health Check

```http
GET /health
```

Returns API health status.

---

## Employees

```http
POST /api/employees
GET /api/employees
GET /api/employees/{id}/load
GET /api/employees/{id}/overload-status
```

### Example Employee Request

```json
{
  "name": "Kris",
  "skills": ["CT", "MRI"]
}
```

---

## Shifts

```http
POST /api/shifts
GET /api/shifts
```

### Example Shift Request

```json
{
  "date": "2026-05-12",
  "shiftType": 2,
  "requiredSkill": "CT",
  "requiredStaff": 1,
  "employeeId": "guid"
}
```

---

## Schedule Generation

```http
POST /api/schedule/generate
```

Generates schedule assignments based on:

- required shift skills
- employee skills
- current employee workload
- overload prevention

### Example Response

```json
{
  "message": "Schedule generation completed.",
  "employeeCount": 1,
  "shiftCount": 2,
  "assignments": [
    {
      "shiftId": "guid",
      "employeeId": "guid",
      "employeeName": "Kris",
      "requiredSkill": "CT",
      "wasAssigned": true
    }
  ]
}
```

---

# Running the Project

## Build

```bash
dotnet build
```

## Run Tests

```bash
dotnet test
```

## Start API

```bash
dotnet run --project src/ShiftPlanner.Api
```

---

# Test Status

The solution currently includes 27 unit tests covering:

- Shift staffing rules
- Skill validation
- Load score calculations
- Employee workload aggregation
- High load warning logic
- Load status calculation
- Schedule generation
- Skill-based assignment
- Lowest-load employee selection
- High-load assignment prevention

---

# Example Scheduling Flow

```text
Open shifts
↓
Schedule generator
↓
Skill matching
↓
Load balancing
↓
Overload prevention
↓
Persist assignments
↓
Updated database state
```

---

# Future Improvements

- Weekly scheduling windows
- Fairness balancing across departments
- Rest-time validation rules
- Maximum shifts per week
- Employee preference profiles
- CSV shift import/export
- Authentication and authorization
- Advanced healthcare scheduling rules
- Frontend dashboard
- Docker support
- CI/CD pipeline with GitHub Actions