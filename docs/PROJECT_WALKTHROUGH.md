# ShiftPlanner API — project walkthrough

A developer-level guide to the system and, in depth, to the CSV import and export. It describes only what exists in the repository. The README holds the full endpoint reference.

## 1. System overview

ShiftPlanner is a portfolio version of a healthcare-style workforce scheduler. Employees have skills; shifts need a skill and are staffed by one employee each. Schedule generation assigns open shifts using scheduling rules and workload scoring, and explains every rejection. A Danish React dashboard shows the overview, weekly schedule, employees, unassigned shifts, a what-if simulation, and CSV import/export.

```text
React + TypeScript (Vite, :5173)
        │ fetch, JSON / CSV
        ▼
ShiftPlanner.Api            Minimal API endpoints, CORS, error middleware
        ▼
ShiftPlanner.Application    services, scheduling rules, import/export logic
        ▼
ShiftPlanner.Domain         Employee, Shift, ShiftType, load calculation
        ▲
ShiftPlanner.Infrastructure EF Core + SQLite repositories and migrations
```

Dependencies point inwards: Application depends on Domain and defines repository interfaces; Infrastructure implements them.

## 2. CSV import and export

### Problem

Planners often prepare a week in a spreadsheet. Importing it must not corrupt the schedule: every row has to be checked, the planner must see the result before anything is written, and an assignment must meet the same rules as one made by the scheduler. The schedule should also go back out to a spreadsheet.

### Flow

```text
Planner chooses file ──► POST /api/shift-imports/preview   (raw CSV body)
                           ShiftCsvParser.Parse          structure: header, quoting, limits
                           ShiftImportValidator.Validate row checks + scheduling rules
                           ShiftImportRepository.AddAsync stores a *pending* import (no shifts)
                         ◄── preview: rows with errors/warnings, canConfirm

Planner clicks "Bekræft" ─► POST /api/shift-imports/{id}/confirm
                           load stored file → parse + validate again (current schedule)
                           not valid any more → 409 + fresh preview, nothing saved
                           ShiftImportRepository.ConfirmAsync (one transaction):
                             UPDATE import SET Confirmed WHERE Id = @id AND Status = 'Pending'
                             INSERT all shifts
                         ◄── 200 { createdShiftCount }

GET /api/schedule/export?weekStart=… ─► WeeklyScheduleCsvExporter → CSV in the import format
```

### Files

| Layer | File | Responsibility |
| --- | --- | --- |
| Application | `Imports/ShiftCsvFormat.cs` | Column names, RFC 4180 reader, field quoting, formula detection; shared by import and export |
| Application | `Imports/ShiftCsvParser.cs` | BOM, delimiter detection (`,` or `;`), header mapping, size limits, row numbers that match the file |
| Application | `Imports/ShiftImportValidator.cs` | Per-row checks, employee matching, duplicates, **injected `IEnumerable<ISchedulingRule>`** |
| Application | `Imports/ShiftImportService.cs` | Preview and confirmation orchestration, content hash, already-imported check |
| Application | `Imports/WeeklyScheduleCsvExporter.cs` | Monday–Sunday week to CSV |
| Infrastructure | `Persistence/Imports/ShiftImportRepository.cs` | Stores imports; atomic confirmation; translates the unique-index violation |
| Infrastructure | `Migrations/*_AddShiftImports.cs` | `ShiftImports` table and the filtered unique index |
| Api | `Imports/ShiftImportEndpoints.cs` | Body size limit, file name sanitising, HTTP status mapping, CSV file response |
| Frontend | `components/ShiftImportPanel.tsx`, `api/shiftImportApi.ts` | Upload, preview table, Danish messages, confirm, export link |

### Reusing the scheduling rules

`ShiftImportValidator` receives the same `ISchedulingRule` registrations that `ScheduleGeneratorService` uses (`MaxAssignmentsRule`, `SameDayShiftRule`, `NightToDayRule`, `HighLoadRule`). For each assigned row it builds the generator's `SchedulingRuleContext`: the employee, the candidate shift, and the planned shifts (the saved assignments plus the assignments accepted from earlier rows). Rows are therefore checked in file order, exactly as the generator processes open shifts. Rejected rows are not added to the planned shifts, so one bad row does not cause errors in the rows after it. Adding a new rule to the DI container automatically applies it to imports too.

### Duplicates — three levels

1. **Within the file**: the same assignment twice is an error pointing to the first row. Identical *open* rows are allowed with a warning, because the domain models "two employees needed" as two shifts.
2. **Against the schedule**: an assignment that already exists is an error; an identical open shift is a warning.
3. **The whole file**: the SHA-256 of the content (BOM and line endings normalised) is stored. A preview of an already confirmed file shows a file error and cannot be confirmed. A filtered unique index (`ContentHash` where `Status = 'Confirmed'`) enforces this in the database even when two different pending imports of the same file are confirmed at the same time.

### Explicit confirmation and consistency

- The preview never creates shifts; the integration tests count shifts before and after.
- Confirmation re-validates the stored content, because the schedule can change between preview and confirmation. A test adds a conflicting shift after the preview and expects `409`.
- The conditional `UPDATE … WHERE Status = 'Pending'` and the shift inserts run in one transaction. Two confirmations of the same import cannot both insert, and a failure leaves neither the status change nor any shift behind.
- The import is all-or-nothing. A partial import was rejected as a design option because the planner would have to work out which rows still need fixing.

### Security and safety

- 256 KB body limit and 1,000 rows per file; the file name is reduced to its last path segment.
- Values starting with `=`, `+`, `-`, `@` are refused on import and prefixed with `'` on export (CSV/formula injection).
- Employee names are matched exactly (ignoring case); ambiguous names are errors rather than guesses.
- No authentication exists in this public demo, so any caller can import. This is a known limitation of the whole API, not specific to import.

### Export

`WeeklyScheduleCsvExporter.StartOfWeek` turns any date into its Monday. The export is sorted deterministically, uses the import's columns and includes a UTF-8 BOM so Excel shows æ/ø/å correctly; the parser strips it. A unit test and an integration test import an exported week again without errors (round trip).

## 3. Testing

| Suite | Count | Import/export coverage |
| --- | --- | --- |
| `tests/ShiftPlanner.Tests` (xUnit) | 101 (38 new) | Parser structure and errors; every row error and warning; the four rules through the validator; exporter format, formula neutralising and round trip |
| `tests/ShiftPlanner.Api.IntegrationTests` (real API, migrations, in-memory SQLite) | 46 (17 new) | Preview saves nothing; confirm once; invalid rows block; duplicate levels; re-validation; rules; 400/404; repository one-time and unique-content guards; demo reset; export format and round trip |
| `frontend` (Vitest + Testing Library, `fetch` stubbed) | 13 (5 new) | Preview then confirm; Danish errors; blocked button; unreadable file; changed schedule `409`; file input cleared; export link |

Mutation checks were run by hand: removing `Status == Pending` from the confirmation, skipping re-validation, or removing the file-input reset each makes the corresponding test fail.

Mocked: the frontend tests stub `fetch`. Not mocked: the API tests use the real endpoints, EF Core, migrations and SQLite.

## 4. Local demonstration (1 November 2026)

Windows 11, API on `http://localhost:5026` with a separate SQLite file, Vite on `:5173`, Microsoft Edge driven by Playwright:

1. Demo reset: 10 employees, 14 shifts.
2. `docs/samples/shift-import-with-errors.csv`: 4 of 11 rows valid; duplicate, night-to-day, workload, missing skill, invalid date, invalid shift type and unknown employee reported in Danish; button disabled; still 14 shifts.
3. `docs/samples/shift-import-week-21.csv`: 11 of 11 valid; still 14 shifts after the preview; confirmed → 25 shifts.
4. Same file again: "Filen er allerede importeret …", button disabled.
5. Export of the week: the 11 shifts in import format; a real browser download produced `shift-schedule-2026-05-11.csv` for the default week.
6. 390 px width: no horizontal overflow on the import page (rows become labelled cards); no console errors.

The "Ugeplan" page overflows horizontally at 390 px. This is pre-existing and was not changed here.

## 5. Running locally

```bash
dotnet run --project src/ShiftPlanner.Api          # http://localhost:5026, SQLite file shiftplanner.db
cd frontend && npm ci && npm run dev               # http://localhost:5173
```

Use "Nulstil demo-data", open "Import og eksport", and try the files in `docs/samples/`. To keep a demo separate from existing data, set `ConnectionStrings__ShiftPlanner="Data Source=<path>"` before starting the API. Back up `shiftplanner.db` by stopping the API and copying the file (including `-wal`/`-shm` if present).

## 6. Limitations and future work

- One file format; no mapping of other column names or real roster exports (kept for the private product scope).
- Employees are matched by name, not an external id.
- Pending imports are kept until a demo reset; there is no expiry or clean-up job.
- No authentication or authorisation.

## 7. Ownership and AI assistance

This feature was implemented with AI assistance (Claude Code). The owner set the requirements and reviews and merges the work. The design choices above (two-step import, all-or-nothing, rule reuse, three duplicate levels) are documented so they can be explained and challenged.

## 8. Interview preparation — CSV import

**Non-technical:** A planner uploads a spreadsheet of shifts. The system checks every line, shows what is wrong in plain Danish, and only saves when the planner presses confirm. The same file cannot be imported twice.

**Developer:** The parse → validate → store-pending → confirm flow; the validator reuses the injected scheduling rules; confirmation re-validates and writes in one transaction.

**Senior:** Consistency under concurrency is enforced in the database (conditional update, filtered unique index), not only in application checks. The preview is a server-side resource, so the confirmation saves exactly the previewed content. Import and export share one format definition, which makes a round trip testable.

**Questions and answers**

1. *Why store the preview on the server instead of re-uploading on confirm?* The confirmation must save exactly what was reviewed. Storing it gives an id to confirm once and a content hash for duplicate detection.
2. *Why re-validate on confirm?* Time passes between preview and confirm; another user or the generator may have changed the schedule. Re-validation turns a would-be rule violation into a `409` with the new reasons.
3. *How do you stop a double click from importing twice?* The status change is `UPDATE … WHERE Status = 'Pending'` inside the transaction that inserts the shifts; the second request updates zero rows and returns `409`.
4. *Why are repeated open rows only a warning?* A shift has exactly one employee, so two open rows are how the file asks for two people. Blocking them would make that impossible.
5. *What is CSV injection and how is it handled?* Cells starting with `=`, `+`, `-` or `@` can run as formulas in spreadsheets. Import refuses them; export prefixes them with `'`.

**Hands-on exercise:** Add an optional `employeeId` column so rows can be matched by id instead of name. Prefer the id when present, report a mismatch between id and name as an error, cover it with validator and integration tests, and include the column in the export.
