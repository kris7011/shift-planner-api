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
- Maximum assignments per employee
- Prevention of multiple shifts on the same day
- Prevention of day shifts after night shifts
- Persisted generated assignments
- REST API endpoints
- Swagger / OpenAPI documentation
- Dependency Injection
- Clean Architecture separation
- Unit tested business logic
- GitHub Actions CI pipeline

---

# Technologies

- C#
- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- xUnit
- Dependency Injection
- Swagger / OpenAPI
- GitHub Actions
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
- scheduling rules
- request/response models

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
- Swagger configuration

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
- maximum assignments
- same-day assignment prevention
- night-to-day scheduling prevention

### Example Request

```json
{
  "maxAssignmentsPerEmployee": 5
}
```

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

# Scheduling Rules

The scheduling engine currently supports several automatic scheduling constraints:

- Employees must have the required shift skill
- Employees with the lowest current workload are prioritized
- Employees cannot exceed configured assignment limits
- Employees cannot be assigned multiple shifts on the same day
- Employees cannot be assigned a day shift directly after a night shift
- High projected workload assignments are automatically prevented

The scheduling engine is designed for future extension through isolated scheduling rules and services.

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

# Swagger UI

After starting the API, Swagger UI is available at:

```text
http://localhost:5026/swagger
```

Swagger provides interactive API documentation and endpoint testing.

---

# CI Pipeline

The project includes a GitHub Actions workflow that automatically:

- restores dependencies
- builds the solution
- runs unit tests

on every push and pull request.

Workflow file:

```text
.github/workflows/dotnet.yml
```

---

# Test Status

The solution currently includes unit tests covering:

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
- Maximum assignment rules
- Same-day assignment prevention
- Night-to-day scheduling prevention

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
Scheduling rules validation
↓
Overload prevention
↓
Persist assignments
↓
Updated database state
```

---

# Future Improvements

- Rule-based scheduling engine
- Dedicated scheduling rule classes
- Weekly scheduling windows
- Fairness balancing across departments
- EU rest-time compliance validation
- Maximum shifts per week
- Weekend distribution balancing
- Employee preference profiles
- CSV/Excel shift import/export
- Authentication and authorization
- Advanced healthcare scheduling rules
- React or Blazor frontend dashboard
- Docker container support
- CI/CD deployment pipeline
- AI-assisted scheduling recommendations
- Scheduling analytics and reporting

---

# Project Goals

This project is designed as a backend architecture and scheduling engine portfolio project focused on:

- scalable API design
- healthcare-oriented scheduling logic
- clean separation of responsibilities
- maintainable business rules
- extensible scheduling architecture
- automated testing and validation