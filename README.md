# ShiftPlanner API

A backend-focused workforce planning and scheduling system built with C# and .NET.

The project simulates healthcare-oriented workforce scheduling by combining:

- employee skills
- shift requirements
- workload scoring
- overload prevention
- rule-based scheduling
- explainable scheduling decisions

The solution is built using Clean Architecture principles with strong focus on:

- maintainability
- separation of concerns
- testable business logic
- scalable backend architecture
- explainable scheduling pipelines

---

# Features

- Employee persistence with EF Core and SQLite
- Shift persistence with employee assignment
- Employee and shift domain models
- Rule-based scheduling engine
- Skill-based automatic assignment
- Lowest-load employee selection
- Maximum assignment protection
- Same-day assignment prevention
- Night-to-day shift protection
- High workload prevention
- Scheduling failure reason tracking
- Explainable scheduling decisions
- Employee workload calculation
- Employee overload detection
- REST API endpoints
- Dependency Injection
- Swagger / OpenAPI support
- GitHub Actions CI pipeline
- Unit tested business logic
- Scheduling rule context for extensible rule evaluation

---

# Technologies

- C#
- .NET 9
- ASP.NET Core Web API
- Entity Framework Core
- SQLite
- xUnit
- Swagger / OpenAPI
- Dependency Injection
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

---

# Layer Responsibilities

## Domain

Contains:

- entities
- enums
- business rules
- core scheduling concepts

The Domain layer has no external dependencies.

---

## Application

Contains:

- scheduling engine
- scheduling rules
- workload calculation services
- overload detection services
- orchestration logic

---

## Infrastructure

Contains:

- Entity Framework Core persistence
- SQLite database access
- repository implementations

---

## API

Contains:

- REST endpoints
- request/response contracts
- middleware
- Swagger configuration
- dependency injection setup

---

# Scheduling Engine

Each rule receives a SchedulingRuleContext that contains the employee, shift, planned shifts, and scheduling limits. This keeps rule signatures stable as the scheduling engine grows.

Current rules include:

- MaxAssignmentsRule
- SameDayShiftRule
- NightToDayRule
- HighLoadRule

Rules are injected through dependency injection and evaluated during schedule generation.

---

# Explainable Scheduling

The scheduler does not only determine whether an employee can be assigned.

It also explains why assignment failed.

Example failure reasons:

- Employee already assigned to a shift on the same day
- Employee projected workload would be too high
- Employee has reached maximum assignments
- Employee cannot work day shift after night shift

This creates a foundation for:

- scheduling analytics
- audit logging
- explainable AI scheduling
- frontend decision visibility
- manager tooling

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

- employee skills
- shift requirements
- workload balancing
- scheduling rules
- overload prevention

### Example Request

```json
{
  "maxAssignmentsPerEmployee": 5
}
```

### Example Successful Response

```json
{
  "message": "Schedule generation completed.",
  "employeeCount": 2,
  "shiftCount": 2,
  "assignments": [
    {
      "shiftId": "guid",
      "employeeId": "guid",
      "employeeName": "Kris",
      "requiredSkill": "CT",
      "wasAssigned": true,
      "failureReasons": []
    }
  ]
}
```

### Example Failed Assignment

```json
{
  "shiftId": "guid",
  "employeeId": null,
  "employeeName": null,
  "requiredSkill": "CT",
  "wasAssigned": false,
  "failureReasons": [
    "Kris: Employee is already assigned to a shift on the same day."
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

# CI Pipeline

The repository includes a GitHub Actions pipeline that automatically:

- restores dependencies
- builds the solution
- runs all unit tests

on every push to GitHub.

---

# Test Status

The solution currently includes 38 unit tests covering:

- Shift staffing rules
- Skill validation
- Workload calculations
- Employee load aggregation
- Overload detection
- Schedule generation
- Lowest-load assignment selection
- High-load prevention
- Same-day assignment prevention
- Night-to-day shift prevention
- Scheduling failure reasons
- Rule evaluation behavior
- Isolated scheduling rule tests

---

# Example Scheduling Flow

```text
Open shifts
↓
Scheduling engine
↓
Rule evaluation
↓
Load balancing
↓
Candidate selection
↓
Assignment persistence
↓
Updated database state
```

---

# Future Improvements

- Weekly scheduling windows
- Fairness balancing across departments
- Employee preference profiles
- Rest-time validation rules
- Configurable scheduling policies
- Weighted rule priorities
- Scheduling analytics
- Audit logging
- CSV import/export
- Authentication and authorization
- Docker support
- Frontend dashboard
- AI-assisted scheduling recommendations