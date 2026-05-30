# ShiftPlanner API

A simplified portfolio version of a workforce planning and scheduling system built with C# and .NET.

This repository demonstrates backend architecture, scheduling logic, testable business rules, API design, and frontend dashboard implementation using demo data.

The project simulates healthcare-oriented workforce scheduling by combining:

- employee skills
- shift requirements
- workload scoring
- overload prevention
- rule-based scheduling
- explainable scheduling decisions
- schedule overview insights
- employee load overview
- employee load details
- employee preference profiles
- preference-based candidate scoring
- shift assignment analysis
- skill gap identification
- what-if schedule simulation
- weekly schedule visualization

The solution is built using Clean Architecture principles with strong focus on:

- maintainability
- separation of concerns
- testable business logic
- scalable backend architecture
- explainable scheduling pipelines
- frontend decision support

> This is a public portfolio version. It uses simplified demo logic and demo data to show technical implementation, architecture, testing, and UI capabilities.

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
- Employee load overview endpoint
- Employee load details endpoint
- Employee load overview in frontend dashboard
- Employee load summary cards
- Employee skills and workload score visualization
- Shift-level workload score explanation
- Clickable employee workload rows
- Selected employee row highlighting
- Danish workload status labels
- Employee preference profile model
- Employee preference profile provider
- Employee preference profile overview endpoint
- Employee preference scoring
- Preference-based simulation candidate scoring
- Employee preference profiles in frontend dashboard
- Danish preference reason explanations in simulation
- Schedule overview endpoint
- Skill gap overview for unassigned shifts
- Skill capacity overview for department competencies
- Uncovered required skills overview
- Capacity summary for department skill coverage
- Schedule risk summary with Low, Medium, and High risk levels
- Schedule risk indicators for dashboard-ready warnings
- Shift assignment analysis endpoint
- Clickable unassigned shift cards
- Candidate-level assignment analysis
- Danish assignment failure explanations
- Schedule simulation endpoint for what-if planning
- Simulation impact summary for leadership decision support
- Simulation impact indicators for dashboard-ready what-if warnings
- Simulation candidate results with employee-specific assignment explanations
- Simulation candidate scoring
- Configurable assignment limit
- Shared assignment limit across schedule generation, simulation, and assignment analysis
- Demo seed and reset endpoints for quick local testing
- React frontend dashboard
- Danish frontend UI
- Weekly schedule table from Monday to Sunday
- Weekend shift visualization
- Schedule generation from frontend
- Demo data reset from frontend
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
- React
- Vite
- TypeScript

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

Frontend
↓
API
```

---

# Layer Responsibilities

## Domain

Contains:

- entities
- enums
- business rules
- core scheduling concepts
- employee preference profile model

The Domain layer has no external dependencies.

---

## Application

Contains:

- scheduling engine
- scheduling rules
- scheduling overview services
- schedule simulation services
- shift assignment analysis services
- workload calculation services
- employee load overview services
- employee load details services
- employee preference scoring services
- employee preference profile overview services
- overload detection services
- orchestration logic
- demo data seeding logic

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
- demo endpoints
- CORS configuration for local frontend development

---

## Frontend

Contains:

- Danish dashboard UI
- schedule overview display
- weekly schedule table
- schedule generation actions
- demo data reset actions
- employee workload overview
- workload status badges
- workload summary cards
- employee skill chips
- clickable employee workload rows
- selected employee highlight
- employee workload detail panel
- shift-level load score explanation
- employee preference profile cards
- clickable unassigned shift cards
- unassigned shift explanation panel
- candidate-level assignment analysis
- Danish assignment failure explanations
- shift simulation panel
- preference-based candidate scoring display
- Danish preference reason explanations
- configurable assignment limit control
- shared assignment limit for scheduling, simulation, and assignment analysis

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

# Configurable Assignment Limit

The dashboard includes a shared assignment limit setting.

This setting controls how many shifts each employee may be assigned during scheduling.

The same limit is used by:

- schedule generation
- shift simulation
- shift assignment analysis

This keeps planning, simulation, and explanation logic aligned in the frontend.

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
- explainable scheduling
- frontend decision visibility
- manager tooling

---

# Employee Load Overview

The employee load overview provides a dashboard-ready view of each employee's current workload.

It includes:

- employee name
- employee skills
- total workload score
- workload status
- high-risk flag

The frontend also shows summary cards for:

- high workload employees
- medium workload employees
- low workload employees
- average workload score

This makes it easier to identify whether the schedule is balanced across employees.

---

# Employee Load Details

The employee load details feature explains why an employee has a specific workload score.

It includes:

- selected employee
- employee skills
- total workload score
- workload status
- assigned shifts
- shift type
- required skill
- load score per shift

This makes the workload score explainable instead of showing it only as a number.

Example:

```text
Henrik has a total workload score of 4 because he has one Night shift with a load score of 4.
```

In the frontend, employee rows in the workload table are clickable. When a row is selected, the dashboard shows a detail panel with the assigned shifts that contribute to the employee's total workload score.

---

# Employee Preference Profiles

The employee preference profile feature demonstrates how individual employee preferences can influence candidate scoring.

A preference profile can include:

- preferred shift types
- disliked shift types
- maximum preferred night shifts
- maximum preferred evening shifts
- weekend preference
- weekend avoidance

Example demo profiles:

```text
Henrik
- Prefers Night shifts
- Maximum preferred night shifts: 3

Mette
- Dislikes Night shifts
- Maximum preferred night shifts: 1

Peter
- Prefers to avoid weekend shifts
```

Preference profiles are currently demo-based and read-only.

They are used to adjust candidate scores in schedule simulation without acting as hard blocking rules.

Example:

```text
Henrik prefers Night shifts.
↓
Henrik receives a positive score adjustment when a Night shift is simulated.
```

This keeps the system explainable while showing how individual preferences can support better workforce planning.

---

# Shift Assignment Analysis

The shift assignment analysis feature explains why a shift can or cannot be covered.

It includes:

- selected shift
- shift date
- shift type
- required skill
- assignment status
- coverage status
- summary reasons
- candidate-level assignment results
- blocking reasons per employee

This makes unassigned shifts explainable instead of only showing that they are uncovered.

Example:

```text
The UL shift cannot be covered because no employees have the required skill 'UL'.
```

In the frontend, unassigned shift cards are clickable. When a card is selected, the dashboard shows an explanation panel with summary reasons and candidate-level blocking reasons.

---

# Schedule Simulation

The schedule simulation feature allows a potential shift to be tested before it is saved.

It evaluates:

- employee skills
- scheduling rules
- current assignments
- assignment limits
- employee preferences
- candidate scores

The simulation result includes:

- whether the shift can be covered
- risk level
- suggested employee
- impact summary
- impact indicators
- candidate-level scores
- employee-specific reasons

This makes it possible to test what-if scenarios without changing the actual schedule.

---

# Demo Data

The demo dataset is designed to show a realistic healthcare-oriented scheduling scenario.

It includes:

- 10 employees
- 14 shifts
- shifts from Monday to Sunday
- weekday and weekend shifts
- assigned and unassigned shifts
- CT, MRI, XR, and Night competencies
- deliberate skill gaps for UL and Intervention
- demo preference profiles for selected employees

The deliberate skill gaps and preference profiles make it possible to demonstrate:

- uncovered required skills
- capacity risk
- unassigned shifts
- shift assignment analysis
- schedule simulation
- candidate scoring
- preference-based candidate scoring
- employee workload distribution
- employee load details
- leadership-oriented decision support

---

# API Endpoints

## Health Check

```http
GET /health
```

Returns API health status.

---

## Demo Data

```http
POST /api/demo/seed
POST /api/demo/reset
```

The demo endpoints make it easy to test the API locally with realistic sample data.

### Seed Demo Data

```http
POST /api/demo/seed
```

Creates demo employees and shifts if the database is empty.

Example response when data is created:

```json
{
  "wasSeeded": true,
  "message": "Demo data was seeded.",
  "employeeCount": 10,
  "shiftCount": 14
}
```

Example response when data already exists:

```json
{
  "wasSeeded": false,
  "message": "Demo data was skipped because the database already contains data.",
  "employeeCount": 10,
  "shiftCount": 14
}
```

### Reset Demo Data

```http
POST /api/demo/reset
```

Deletes all employees and shifts, then creates fresh demo data.

```json
{
  "wasSeeded": true,
  "message": "Demo data was reset and seeded.",
  "employeeCount": 10,
  "shiftCount": 14
}
```

---

## Employees

```http
POST /api/employees
GET /api/employees
GET /api/employees/load-overview
GET /api/employees/preference-profiles
GET /api/employees/{id}/load
GET /api/employees/{id}/overload-status
GET /api/employees/{id}/load-details
```

### Example Employee Request

```json
{
  "name": "Kris",
  "skills": ["CT", "MRI"]
}
```

---

## Employee Load Overview

```http
GET /api/employees/load-overview
```

Returns the current workload overview for all employees.

The response is ordered by highest workload first.

### Example Response

```json
[
  {
    "employeeId": "guid",
    "employeeName": "Henrik",
    "skills": ["CT", "Night"],
    "totalLoad": 4,
    "loadStatus": "Medium",
    "isHighRisk": false
  },
  {
    "employeeId": "guid",
    "employeeName": "Mette",
    "skills": ["MRI", "XR"],
    "totalLoad": 2,
    "loadStatus": "Low",
    "isHighRisk": false
  }
]
```

---

## Employee Load Details

```http
GET /api/employees/{id}/load-details
```

Returns detailed workload information for one employee.

The response explains which assigned shifts contribute to the employee's total workload score.

### Example Response

```json
{
  "employeeId": "guid",
  "employeeName": "Henrik",
  "skills": ["CT", "Night"],
  "totalLoad": 4,
  "loadStatus": "Medium",
  "isHighRisk": false,
  "assignedShifts": [
    {
      "shiftId": "guid",
      "date": "2026-05-12",
      "shiftType": "Night",
      "requiredSkill": "Night",
      "loadScore": 4
    }
  ]
}
```

---

## Employee Preference Profiles

```http
GET /api/employees/preference-profiles
```

Returns demo preference profiles for all employees.

The response is ordered by employee name.

### Example Response

```json
[
  {
    "employeeId": "guid",
    "employeeName": "Henrik",
    "preferredShiftTypes": ["Night"],
    "dislikedShiftTypes": [],
    "maxNightShifts": 3,
    "maxEveningShifts": null,
    "prefersWeekends": false,
    "avoidsWeekends": false
  },
  {
    "employeeId": "guid",
    "employeeName": "Mette",
    "preferredShiftTypes": [],
    "dislikedShiftTypes": ["Night"],
    "maxNightShifts": 1,
    "maxEveningShifts": null,
    "prefersWeekends": false,
    "avoidsWeekends": false
  },
  {
    "employeeId": "guid",
    "employeeName": "Peter",
    "preferredShiftTypes": [],
    "dislikedShiftTypes": [],
    "maxNightShifts": null,
    "maxEveningShifts": null,
    "prefersWeekends": false,
    "avoidsWeekends": true
  }
]
```

---

## Shifts

```http
POST /api/shifts
GET /api/shifts
GET /api/shifts/{id}/assignment-analysis
```

### Example Shift Request

```json
{
  "date": "2026-05-12",
  "shiftType": "Night",
  "requiredSkill": "Night",
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
- assignment limit

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
  "employeeCount": 10,
  "shiftCount": 14,
  "assignments": [
    {
      "shiftId": "guid",
      "employeeId": "guid",
      "employeeName": "Kris",
      "requiredSkill": "CT",
      "wasAssigned": true,
      "failureReasons": []
    },
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
  "totalShifts": 14,
  "assignedShifts": 12,
  "unassignedShifts": 2,
  "coverageRate": 85.71,
  "employeeCount": 10,
  "highRiskEmployeeCount": 0,
  "unassignedShiftDetails": [
    {
      "shiftId": "guid",
      "date": "2026-05-15",
      "shiftType": "Day",
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
    },
    {
      "requiredSkill": "Intervention",
      "unassignedShiftCount": 1
    }
  ],
  "riskSummary": {
    "coverageRisk": "Medium",
    "unassignedShiftCount": 2,
    "skillGapCount": 2,
    "highRiskEmployeeCount": 0
  },
  "riskIndicators": [
    {
      "type": "Coverage",
      "severity": "Medium",
      "message": "Schedule coverage is 85.71%."
    },
    {
      "type": "UnassignedShifts",
      "severity": "Medium",
      "message": "2 shift(s) are currently unassigned."
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
      "employeeCount": 4
    },
    {
      "skill": "XR",
      "employeeCount": 4
    },
    {
      "skill": "MRI",
      "employeeCount": 4
    },
    {
      "skill": "Night",
      "employeeCount": 3
    }
  ],
  "uncoveredRequiredSkills": [
    {
      "skill": "UL",
      "requiredByUnassignedShifts": 1,
      "availableEmployees": 0
    },
    {
      "skill": "Intervention",
      "requiredByUnassignedShifts": 1,
      "availableEmployees": 0
    }
  ],
  "capacitySummary": {
    "totalSkills": 4,
    "missingRequiredSkills": 2,
    "criticalSkillGaps": 2
  }
}
```

---

## Shift Assignment Analysis

```http
GET /api/shifts/{id}/assignment-analysis
```

Returns an assignment analysis for a specific shift.

The analysis explains whether the shift can be covered and why each employee can or cannot be assigned.

The endpoint also supports assignment limit configuration:

```http
GET /api/shifts/{id}/assignment-analysis?maxAssignmentsPerEmployee=5
```

### Example Response

```json
{
  "shiftId": "guid",
  "date": "2026-05-15",
  "shiftType": "Day",
  "requiredSkill": "UL",
  "isAssigned": false,
  "canBeCovered": false,
  "summaryReasons": [
    "No employees have the required skill 'UL'."
  ],
  "candidateResults": [
    {
      "employeeId": "guid",
      "employeeName": "Kris",
      "canBeAssigned": false,
      "reasons": [
        "Missing required skill 'UL'."
      ]
    }
  ]
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
  "shiftType": "Day",
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
  "impactSummary": "This shift cannot be covered because no available employee can satisfy the required skill 'UL' and scheduling rules.",
  "impactIndicators": [
    {
      "type": "Coverage",
      "severity": "High",
      "message": "The simulated shift cannot be covered."
    },
    {
      "type": "Skill",
      "severity": "High",
      "message": "No available employee can satisfy the required skill 'UL'."
    }
  ],
  "candidateResults": [
    {
      "employeeId": "guid",
      "employeeName": "Kris",
      "canBeAssigned": false,
      "score": 0,
      "reasons": [
        "Missing required skill 'UL'."
      ]
    }
  ]
}
```

### Example Successful Simulation Response

```json
{
  "canBeCovered": true,
  "requiredSkill": "Night",
  "riskLevel": "Low",
  "suggestedEmployeeId": "guid",
  "suggestedEmployeeName": "Henrik",
  "failureReasons": [],
  "impactSummary": "This shift can be covered by Henrik with low scheduling risk.",
  "impactIndicators": [
    {
      "type": "Coverage",
      "severity": "Low",
      "message": "The simulated shift can be covered."
    }
  ],
  "candidateResults": [
    {
      "employeeId": "guid",
      "employeeName": "Henrik",
      "canBeAssigned": true,
      "score": 115,
      "reasons": [
        "Employee prefers Night shifts."
      ]
    },
    {
      "employeeId": "guid",
      "employeeName": "Anna",
      "canBeAssigned": true,
      "score": 100,
      "reasons": []
    }
  ]
}
```

---

# Running the Project

## Build Backend

```bash
dotnet build
```

## Run Backend Tests

```bash
dotnet test
```

## Start API

```bash
dotnet run --project src/ShiftPlanner.Api
```

---

# Running the Frontend

From the `frontend` folder:

```bash
npm install
npm run dev
```

The frontend is available at:

```text
http://localhost:5173
```

---

# Seed Demo Data

After starting the API, create a fresh demo dataset:

```bash
curl -X POST http://localhost:5026/api/demo/reset
```

This creates:

- 10 demo employees
- 14 demo shifts
- weekday and weekend shifts
- assigned and unassigned shifts
- deliberate skill gaps for UL and Intervention
- demo preference profiles for selected employees

Then open the schedule overview:

```bash
curl http://localhost:5026/api/schedule/overview
```

You can also inspect the employee load overview:

```bash
curl http://localhost:5026/api/employees/load-overview
```

Or inspect employee preference profiles:

```bash
curl http://localhost:5026/api/employees/preference-profiles
```

Or inspect load details for a specific employee:

```bash
curl http://localhost:5026/api/employees/{id}/load-details
```

Or inspect assignment analysis for a specific shift:

```bash
curl "http://localhost:5026/api/shifts/{id}/assignment-analysis?maxAssignmentsPerEmployee=5"
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

The solution currently includes 59 unit tests covering:

- Shift staffing rules
- Skill validation
- Workload calculations
- Employee load aggregation
- Employee load overview logic
- Employee load details logic
- Employee preference scoring logic
- Employee preference profile provider logic
- Employee preference profile overview logic
- Preference-based simulation candidate scoring
- Shift assignment analysis logic
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
- Simulation impact indicator logic
- Simulation candidate result logic
- Simulation candidate scoring logic
- Demo data seeding and reset support

---

# Example Frontend Demo Flow

```text
Start API
↓
Start frontend
↓
Click "Nulstil demo-data"
↓
Set max shifts per employee
↓
Review dashboard status
↓
Review weekly schedule from Monday to Sunday
↓
Review employee load summary cards
↓
Review employee workload table
↓
Click an employee row
↓
Review employee load details
↓
Review employee preference profiles
↓
Click "Generér vagtplan"
↓
Review assigned and unassigned shifts
↓
Click an unassigned shift
↓
Review assignment analysis and candidate blocking reasons
↓
Simulate a new shift
↓
Review suggested employee, preference reasons, and candidate scores
```

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

# Example Preference Scoring Flow

```text
Employee
↓
Demo preference profile provider
↓
Preference scoring service
↓
Score adjustment and preference reasons
↓
Simulation candidate score
↓
Frontend explanation
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
Employee workload overview
↓
Employee workload details
↓
Employee preference profiles
↓
Shift assignment analysis
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
Preference scoring
↓
Coverage decision
↓
Suggested employee or failure reasons
↓
Impact summary
↓
Impact indicators
↓
Candidate results
↓
Candidate scoring
```

---

# Demo Data Flow

```text
POST /api/demo/reset
↓
Delete existing shifts
↓
Delete existing employees
↓
Create demo employees
↓
Create demo shifts
↓
Return seed result
```

---

# Public Portfolio Scope

This repository is intended as a public portfolio project.

It demonstrates:

- Clean Architecture
- API design
- testable business logic
- scheduling rules
- explainable decisions
- frontend dashboard design
- TypeScript API clients
- automated tests
- CI pipeline

It intentionally uses simplified demo logic and demo data.

Advanced product logic, commercial algorithms, real-world configuration, and AI-driven analysis are outside the scope of this public version.

---

# Future Improvements

Public portfolio improvements could include:

- Docker support
- improved OpenAPI documentation
- better frontend component structure
- screenshot-based README section
- deployment instructions
- authentication demonstration
- CSV import/export demo

Product-oriented improvements should be developed separately in a private repository, for example:

- advanced workload modelling
- individualized reference profiles
- absence pattern analysis
- AI-assisted leadership summaries
- department-specific configuration
- real schedule import mapping
- commercial SaaS features

---

# Project Goals

This project is designed as a backend and frontend portfolio project focused on:

- scalable API design
- healthcare-oriented scheduling logic
- clean separation of responsibilities
- maintainable business rules
- extensible scheduling architecture
- explainable scheduling decisions
- employee workload visibility
- employee preference visibility
- preference-based candidate scoring
- shift-level workload explanation
- shift assignment explanation
- leadership-oriented workforce planning insights
- what-if planning and simulation
- automated testing and validation
- practical frontend decision support