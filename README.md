# ShiftPlanner API

A backend-focused workforce planning and scheduling system built with C# and .NET.

The project simulates healthcare-oriented workforce scheduling by combining:

- employee skills
- shift requirements
- workload scoring
- overload prevention
- rule-based scheduling
- explainable scheduling decisions
- schedule overview insights
- skill gap identification
- what-if schedule simulation

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
- Schedule overview endpoint
- Skill gap overview for unassigned shifts
- Skill capacity overview for department competencies
- Uncovered required skills overview
- Capacity summary for department skill coverage
- Schedule risk summary with Low, Medium, and High risk levels
- Schedule risk indicators for dashboard-ready warnings
- Schedule simulation endpoint for what-if planning
- Simulation impact summary for leadership decision support
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
- scheduling overview services
- schedule simulation services
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

The scheduling engine uses a rule-based architecture where each scheduling rule is isolated into its own class.

Each rule receives a `SchedulingRuleContext` that contains:

- employee
- shift
- planned shifts
- scheduling limits

This keeps rule signatures stable as the scheduling engine grows.

Current rules include:

- `MaxAssignmentsRule`
- `SameDayShiftRule`
- `NightToDayRule`
- `HighLoadRule`

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
- Employee is missing the required skill

This creates a foundation for:

- scheduling analytics
- audit logging
- explainable AI scheduling
- frontend decision visibility
- manager tooling

---

# Skill Gap Overview

The schedule overview identifies which required skills are missing from unassigned shifts.

Example:

```json
{
  "skillGaps": [
    {
      "requiredSkill": "UL",
      "unassignedShiftCount": 1
    }
  ]
}
```

This helps managers identify staffing vulnerabilities and missing competencies in the current schedule.

---

# Capacity Overview

The schedule overview also compares required skills with the department's available employee competencies.

Example:

```json
{
  "skillCapacity": [
    {
      "skill": "CT",
      "employeeCount": 1
    },
    {
      "skill": "MRI",
      "employeeCount": 1
    }
  ],
  "uncoveredRequiredSkills": [
    {
      "skill": "UL",
      "requiredByUnassignedShifts": 1,
      "availableEmployees": 0
    }
  ],
  "capacitySummary": {
    "totalSkills": 2,
    "missingRequiredSkills": 1,
    "criticalSkillGaps": 1
  }
}
```

This helps managers distinguish between a planning issue and an actual competency capacity issue.

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
  "requiredSkill": "UL",
  "wasAssigned": false,
  "failureReasons": [
    "Kris: Missing required skill 'UL'."
  ]
}
```

---

## Schedule Overview

```http
GET /api/schedule/overview
```

Returns a leadership-oriented overview of the current schedule.

The overview includes:

- total shifts
- assigned shifts
- unassigned shifts
- coverage rate
- employee count
- high-risk employee count
- unassigned shift details
- skill gaps
- risk summary
- risk indicators
- skill capacity
- uncovered required skills
- capacity summary

### Example Response

```json
{
  "totalShifts": 3,
  "assignedShifts": 2,
  "unassignedShifts": 1,
  "coverageRate": 66.67,
  "employeeCount": 1,
  "highRiskEmployeeCount": 0,
  "unassignedShiftDetails": [
    {
      "shiftId": "guid",
      "date": "2026-05-13",
      "shiftType": 2,
      "requiredSkill": "UL",
      "failureReasons": [
        "Kris: Missing required skill 'UL'."
      ]
    }
  ],
  "skillGaps": [
    {
      "requiredSkill": "UL",
      "unassignedShiftCount": 1
    }
  ],
  "riskSummary": {
    "coverageRisk": "Medium",
    "unassignedShiftCount": 1,
    "skillGapCount": 1,
    "highRiskEmployeeCount": 0
  },
  "riskIndicators": [
    {
      "type": "Coverage",
      "severity": "Medium",
      "message": "Schedule coverage is 66.67%."
    },
    {
      "type": "UnassignedShifts",
      "severity": "Medium",
      "message": "1 shift(s) are currently unassigned."
    },
    {
      "type": "SkillGap",
      "severity": "Medium",
      "message": "1 unassigned shift(s) require UL."
    },
    {
      "type": "Capacity",
      "severity": "High",
      "message": "UL is required by 1 unassigned shift(s), but no employees have this skill."
    }
  ],
  "skillCapacity": [
    {
      "skill": "CT",
      "employeeCount": 1
    },
    {
      "skill": "MRI",
      "employeeCount": 1
    }
  ],
  "uncoveredRequiredSkills": [
    {
      "skill": "UL",
      "requiredByUnassignedShifts": 1,
      "availableEmployees": 0
    }
  ],
  "capacitySummary": {
    "totalSkills": 2,
    "missingRequiredSkills": 1,
    "criticalSkillGaps": 1
  }
}
```

---

## Schedule Simulation

```http
POST /api/schedule/simulate
```

Simulates whether a potential shift can be covered without saving it to the database.

This is useful for what-if planning, capacity evaluation, and leadership decision support.

### Example Request

```json
{
  "date": "2026-05-15",
  "shiftType": 2,
  "requiredSkill": "UL",
  "requiredStaff": 1,
  "maxAssignmentsPerEmployee": 5
}
```

### Example Failed Simulation Response

```json
{
  "canBeCovered": false,
  "requiredSkill": "UL",
  "riskLevel": "High",
  "suggestedEmployeeId": null,
  "suggestedEmployeeName": null,
  "failureReasons": [
    "Kris: Missing required skill 'UL'."
  ],
  "impactSummary": "This shift cannot be covered because no available employee can satisfy the required skill 'UL' and scheduling rules."
}
```

### Example Successful Simulation Response

```json
{
  "canBeCovered": true,
  "requiredSkill": "CT",
  "riskLevel": "Low",
  "suggestedEmployeeId": "guid",
  "suggestedEmployeeName": "Kris",
  "failureReasons": [],
  "impactSummary": "This shift can be covered by Kris with low scheduling risk."
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

# Swagger UI

After starting the API, Swagger UI is available at:

```text
http://localhost:5026/swagger
```

Swagger provides interactive API documentation and endpoint testing.

---

# CI Pipeline

The repository includes a GitHub Actions pipeline that automatically:

- restores dependencies
- builds the solution
- runs all unit tests

on every push to GitHub.

---

# Test Status

The solution currently includes 43 unit tests covering:

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
- Schedule overview logic
- Capacity summary logic
- Schedule simulation logic
- Simulation impact summary logic

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

# Example Leadership Overview Flow

```text
Current shifts
↓
Assigned and unassigned shift analysis
↓
Scheduling rule evaluation
↓
Failure reason collection
↓
Skill gap grouping
↓
Capacity analysis
↓
Risk indicators
↓
Leadership overview response
```

---

# Example Simulation Flow

```text
Potential shift
↓
Simulation endpoint
↓
Scheduling engine
↓
Rule evaluation
↓
Coverage decision
↓
Suggested employee or failure reasons
↓
Impact summary
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

---

# Project Goals

This project is designed as a backend architecture and scheduling engine portfolio project focused on:

- scalable API design
- healthcare-oriented scheduling logic
- clean separation of responsibilities
- maintainable business rules
- extensible scheduling architecture
- explainable scheduling decisions
- leadership-oriented workforce planning insights
- what-if planning and simulation
- automated testing and validation