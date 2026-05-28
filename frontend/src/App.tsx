import { useEffect, useState } from "react";
import "./App.css";
import {
  getShiftAssignmentAnalysis,
  type ShiftAssignmentAnalysisResponse
} from "./api/shiftAssignmentAnalysisApi";
import {
  getEmployeeLoadDetails,
  type EmployeeLoadDetailsResponse
} from "./api/employeeLoadDetailsApi";
import {
  getScheduleOverview,
  type ScheduleOverviewResponse,
  type ScheduleRiskLevel,
} from "./api/scheduleOverviewApi";
import { resetDemoData } from "./api/demoDataApi";
import {
  getEmployeeLoadOverview,
  type EmployeeLoadOverviewItem
} from "./api/employeeLoadOverviewApi";
import {
  generateSchedule,
  type GenerateScheduleResponse,
} from "./api/scheduleGenerationApi";
import { SimulationPanel } from "./components/SimulationPanel";
import { WeeklyScheduleTable } from "./components/WeeklyScheduleTable";

function translateRiskLevel(riskLevel: ScheduleRiskLevel) {
  switch (riskLevel) {
    case "Low":
      return "Lav";
    case "Medium":
      return "Mellem";
    case "High":
      return "Høj";
    default:
      return riskLevel;
  }
}

function translateRiskType(type: string) {
  switch (type) {
    case "Coverage":
      return "Dækning";
    case "UnassignedShifts":
      return "Ubesatte vagter";
    case "SkillGap":
      return "Kompetencegab";
    case "Capacity":
      return "Kapacitet";
    default:
      return type;
  }
}

function translateRiskMessage(message: string) {
  if (message.startsWith("Schedule coverage is")) {
    const percentage = message
      .replace("Schedule coverage is", "")
      .replace(".", "")
      .trim();

    return `Vagtplanens dækningsgrad er ${percentage}.`;
  }

  if (message.includes("shift(s) are currently unassigned")) {
    const count = message.split(" ")[0];

    return `${count} vagt(er) er aktuelt ubesatte.`;
  }

  if (message.includes("unassigned shift(s) require")) {
    const count = message.split(" ")[0];
    const skill = message
      .replace(`${count} unassigned shift(s) require`, "")
      .replace(".", "")
      .trim();

    return `${count} ubesat vagt kræver ${skill}.`;
  }

  if (message.includes("is required by") && message.includes("but no employees have this skill")) {
    const skill = message.split(" is required by ")[0];
    const count = message
      .split(" is required by ")[1]
      .split(" unassigned shift(s)")[0];

    return `${skill} kræves af ${count} ubesat vagt, men ingen medarbejdere har denne kompetence.`;
  }

  return message;
}

function translateShiftType(shiftType: string) {
  switch (shiftType) {
    case "Day":
      return "Dag";
    case "Evening":
      return "Aften";
    case "Night":
      return "Nat";
    case "OnCall":
      return "Vagt fra hjemmet";
    default:
      return shiftType;
  }
}

function formatDate(date: string) {
  return new Intl.DateTimeFormat("da-DK", {
    day: "2-digit",
    month: "2-digit",
    year: "numeric",
  }).format(new Date(date));
}

function translateFailureReason(reason: string) {
  if (reason.includes("Missing required skill")) {
    const skill = reason
      .split("Missing required skill")[1]
      .replace(".", "")
      .replaceAll("'", "")
      .trim();

    const employeeName = reason.split(":")[0];

    return `${employeeName} mangler kompetencen ${skill}.`;
  }

  if (
    reason.includes("day shift") ||
    reason.includes("night shift")
  ) {
    return reason
      .replace("day shift", "dagvagt")
      .replace("night shift", "nattevagt");
  }

  return reason;
}

function translateAssignmentReason(reason: string) {
  if (reason.startsWith("Missing required skill")) {
    const skill = reason.match(/'(.+)'/)?.[1];

    return skill
      ? `Mangler nødvendig kompetence '${skill}'.`
      : "Mangler nødvendig kompetence.";
  }

  if (reason.startsWith("No employees have the required skill")) {
    const skill = reason.match(/'(.+)'/)?.[1];

    return skill
      ? `Ingen medarbejdere har den nødvendige kompetence '${skill}'.`
      : "Ingen medarbejdere har den nødvendige kompetence.";
  }

  if (reason.startsWith("Employee has reached the maximum")) {
    return "Medarbejderen har nået det maksimale antal tildelinger.";
  }

  if (reason === "Employee is already assigned to a shift on the same day.") {
    return "Medarbejderen er allerede tildelt en vagt samme dag.";
  }

  if (reason === "Employee cannot work a day shift immediately after a night shift.") {
    return "Medarbejderen kan ikke arbejde dagvagt direkte efter nattevagt.";
  }

  if (reason === "At least one employee can cover this shift.") {
    return "Mindst én medarbejder kan dække denne vagt.";
  }

  if (reason === "Employees with the required skill are blocked by scheduling rules.") {
    return "Medarbejdere med den nødvendige kompetence er blokeret af planlægningsregler.";
  }

  return reason;
}

function getDashboardStatus(overview: ScheduleOverviewResponse) {
  if (overview.capacitySummary.criticalSkillGaps > 0) {
    return {
      title: "Kræver opmærksomhed",
      description:
        "Der er kritiske kompetencegab, som kan påvirke vagtplanens dækning.",
      className: "high",
    };
  }

  if (overview.unassignedShifts > 0) {
    return {
      title: "Delvist dækket",
      description:
        "Der er ubesatte vagter, men de krævede kompetencer findes i medarbejdergruppen.",
      className: "medium",
    };
  }

  return {
    title: "Stabil vagtplan",
    description: "Alle vagter er dækket uden aktuelle kapacitetsalarmer.",
    className: "low",
  };
}

function translateLoadStatus(status: string) {
  switch (status) {
    case "Low":
      return "Lav";
    case "Medium":
      return "Mellem";
    case "High":
      return "Høj";
    default:
      return status;
  }
}

function App() {
  const [overview, setOverview] = useState<ScheduleOverviewResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isResettingDemoData, setIsResettingDemoData] = useState(false);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);
  const [isGeneratingSchedule, setIsGeneratingSchedule] = useState(false);
  const [generationResult, setGenerationResult] =
    useState<GenerateScheduleResponse | null>(null);
  const [scheduleRefreshKey, setScheduleRefreshKey] = useState(0);
  const [employeeLoadOverview, setEmployeeLoadOverview] = useState<
    EmployeeLoadOverviewItem[]
  >([]);
  const [selectedEmployeeLoadDetails, setSelectedEmployeeLoadDetails] =
    useState<EmployeeLoadDetailsResponse | null>(null);
  const [isLoadingEmployeeDetails, setIsLoadingEmployeeDetails] = useState(false);
  const [employeeDetailsErrorMessage, setEmployeeDetailsErrorMessage] =
    useState<string | null>(null);

  const highLoadEmployeeCount = employeeLoadOverview.filter(
    (employee) => employee.loadStatus === "High"
  ).length;

  const mediumLoadEmployeeCount = employeeLoadOverview.filter(
    (employee) => employee.loadStatus === "Medium"
  ).length;

  const lowLoadEmployeeCount = employeeLoadOverview.filter(
    (employee) => employee.loadStatus === "Low"
  ).length;

  const averageLoadScore =
    employeeLoadOverview.length === 0
      ? 0
      : employeeLoadOverview.reduce(
        (total, employee) => total + employee.totalLoad,
        0
      ) / employeeLoadOverview.length;

  const [
    selectedShiftAssignmentAnalysis,
    setSelectedShiftAssignmentAnalysis
  ] = useState<ShiftAssignmentAnalysisResponse | null>(null);

  const [isLoadingShiftAnalysis, setIsLoadingShiftAnalysis] = useState(false);

  const [shiftAnalysisErrorMessage, setShiftAnalysisErrorMessage] =
    useState<string | null>(null);

  const [maxAssignmentsPerEmployee, setMaxAssignmentsPerEmployee] = useState(5);

  async function loadOverview() {
    try {
      setIsLoading(true);
      setErrorMessage(null);

      const [overviewData, employeeLoadData] = await Promise.all([
        getScheduleOverview(),
        getEmployeeLoadOverview(),
      ]);

      setOverview(overviewData);
      setEmployeeLoadOverview(employeeLoadData);
    } catch {
      setErrorMessage("Kunne ikke hente vagtplanens overblik fra API'et.");
    } finally {
      setIsLoading(false);
    }
  }

  async function handleEmployeeLoadClick(employeeId: string) {
    try {
      setIsLoadingEmployeeDetails(true);
      setEmployeeDetailsErrorMessage(null);

      const details = await getEmployeeLoadDetails(employeeId);

      setSelectedEmployeeLoadDetails(details);
    } catch {
      setEmployeeDetailsErrorMessage(
        "Kunne ikke hente belastningsdetaljer for medarbejderen."
      );
    } finally {
      setIsLoadingEmployeeDetails(false);
    }
  }

  async function handleUnassignedShiftClick(shiftId: string) {
    try {
      setIsLoadingShiftAnalysis(true);
      setShiftAnalysisErrorMessage(null);

      const analysis = await getShiftAssignmentAnalysis(shiftId);

      setSelectedShiftAssignmentAnalysis(analysis);
    } catch {
      setShiftAnalysisErrorMessage(
        "Kunne ikke hente forklaring for den ubesatte vagt."
      );
    } finally {
      setIsLoadingShiftAnalysis(false);
    }
  }

  async function handleResetDemoData() {
    try {
      setIsResettingDemoData(true);
      setErrorMessage(null);
      setStatusMessage(null);
      setGenerationResult(null);

      const result = await resetDemoData();

      setStatusMessage(
        `Demo-data blev nulstillet. ${result.employeeCount} medarbejdere og ${result.shiftCount} vagter blev oprettet.`,
      );

      await loadOverview();
      setScheduleRefreshKey((current) => current + 1);
    } catch {
      setErrorMessage("Kunne ikke nulstille demo-data. Kontroller at API'et kører.");
    } finally {
      setIsResettingDemoData(false);
    }
  }

  async function handleGenerateSchedule() {
    try {
      setIsGeneratingSchedule(true);
      setErrorMessage(null);
      setStatusMessage(null);

      const result = await generateSchedule({
        maxAssignmentsPerEmployee,
      });

      setGenerationResult(result);

      const assignedCount = result.assignments.filter(
        (assignment) => assignment.wasAssigned,
      ).length;

      const unassignedCount = result.assignments.filter(
        (assignment) => !assignment.wasAssigned,
      ).length;

      setStatusMessage(
        `Vagtplanen blev genereret. ${assignedCount} vagt(er) blev tildelt, og ${unassignedCount} vagt(er) kunne ikke tildeles.`,
      );

      await loadOverview();
      setScheduleRefreshKey((current) => current + 1);
    } catch {
      setErrorMessage("Kunne ikke generere vagtplanen. Kontroller at API'et kører.");
    } finally {
      setIsGeneratingSchedule(false);
    }
  }

  useEffect(() => {
    loadOverview();
  }, []);

  if (isLoading) {
    return (
      <main className="page">
        <section className="panel">
          <h1>Vagtplan overblik</h1>
          <p>Henter data...</p>
        </section>
      </main>
    );
  }

  if (errorMessage || overview === null) {
    return (
      <main className="page">
        <section className="panel">
          <h1>Vagtplan overblik</h1>
          <p className="error-text">{errorMessage}</p>
          <button onClick={loadOverview}>Prøv igen</button>
        </section>
      </main>
    );
  }

  const dashboardStatus = getDashboardStatus(overview);

  return (
    <main className="page">
      <header className="hero">
        <div>
          <p className="eyebrow">ShiftPlanner</p>
          <h1>Vagtplan overblik</h1>
          <p>
            Ledelsesoverblik over dækning, ubesatte vagter, kapacitetsrisiko og
            manglende kompetencer.
          </p>
        </div>

        <div className="hero-actions">
          <label className="settings-field">
            <span>Maks vagter pr. medarbejder</span>

            <select
              value={maxAssignmentsPerEmployee}
              onChange={(event) =>
                setMaxAssignmentsPerEmployee(Number(event.target.value))
              }
            >
              <option value={3}>3</option>
              <option value={4}>4</option>
              <option value={5}>5</option>
              <option value={6}>6</option>
              <option value={7}>7</option>
            </select>
          </label>

          <button onClick={loadOverview}>Opdater data</button>

          <button
            className="success-button"
            disabled={isGeneratingSchedule}
            onClick={handleGenerateSchedule}
          >
            {isGeneratingSchedule ? "Genererer..." : "Generér vagtplan"}
          </button>

          <button
            className="secondary-button"
            disabled={isResettingDemoData}
            onClick={handleResetDemoData}
          >
            {isResettingDemoData ? "Nulstiller..." : "Nulstil demo-data"}
          </button>
        </div>
      </header>

      {statusMessage && <p className="status-message">{statusMessage}</p>}

      {generationResult && (
        <section className="generation-result-panel">
          <div className="generation-result-header">
            <div>
              <span>Seneste generering</span>
              <strong>Resultat af vagtplanlægning</strong>
            </div>

            <span className="badge medium">
              {generationResult.assignments.length} vurderede vagter
            </span>
          </div>

          <div className="generation-result-grid">
            <div>
              <span>Tildelte vagter</span>
              <strong>
                {
                  generationResult.assignments.filter(
                    (assignment) => assignment.wasAssigned,
                  ).length
                }
              </strong>
            </div>

            <div>
              <span>Ikke tildelte vagter</span>
              <strong>
                {
                  generationResult.assignments.filter(
                    (assignment) => !assignment.wasAssigned,
                  ).length
                }
              </strong>
            </div>
          </div>

          <div className="generation-assignment-list">
            {generationResult.assignments.slice(0, 6).map((assignment) => (
              <div className="generation-assignment-item" key={assignment.shiftId}>
                <div>
                  <strong>
                    {assignment.wasAssigned
                      ? assignment.employeeName
                      : "Ikke tildelt"}
                  </strong>

                  <p>Krævet kompetence: {assignment.requiredSkill}</p>

                  {!assignment.wasAssigned &&
                    assignment.failureReasons.length > 0 && (
                      <p>
                        {assignment.failureReasons
                          .slice(0, 2)
                          .map(translateFailureReason)
                          .join(" ")}
                      </p>
                    )}
                </div>

                <span
                  className={`badge ${assignment.wasAssigned ? "low" : "high"}`}
                >
                  {assignment.wasAssigned ? "Tildelt" : "Ikke tildelt"}
                </span>
              </div>
            ))}

            {generationResult.assignments.length > 6 && (
              <p className="more-reasons">
                + {generationResult.assignments.length - 6} flere vurderinger
              </p>
            )}
          </div>
        </section>
      )}

      <section className={`status-banner ${dashboardStatus.className}`}>
        <div>
          <span>Aktuel status</span>
          <strong>{dashboardStatus.title}</strong>
          <p>{dashboardStatus.description}</p>
        </div>

        <div className="status-banner-metrics">
          <span>{overview.coverageRate}% dækket</span>
          <span>{overview.unassignedShifts} ubesatte vagter</span>
          <span>
            {overview.capacitySummary.criticalSkillGaps} kritiske kompetencegab
          </span>
        </div>
      </section>

      <section className="summary-grid">
        <article className="summary-card">
          <span>Dækningsgrad</span>
          <strong>{overview.coverageRate}%</strong>
        </article>

        <article className="summary-card">
          <span>Medarbejdere</span>
          <strong>{overview.employeeCount}</strong>
        </article>

        <article className="summary-card">
          <span>Tildelte vagter</span>
          <strong>{overview.assignedShifts}</strong>
        </article>

        <article className="summary-card warning">
          <span>Ubesatte vagter</span>
          <strong>{overview.unassignedShifts}</strong>
        </article>
      </section>

      <section className="content-grid">
        <article className="panel">
          <h2>Kapacitetsoversigt</h2>

          <div className="metric-row">
            <span>Registrerede kompetencer</span>
            <strong>{overview.capacitySummary.totalSkills}</strong>
          </div>

          <div className="metric-row">
            <span>Manglende krævede kompetencer</span>
            <strong>{overview.capacitySummary.missingRequiredSkills}</strong>
          </div>

          <div className="metric-row">
            <span>Kritiske kompetencegab</span>
            <strong>{overview.capacitySummary.criticalSkillGaps}</strong>
          </div>
        </article>

        <article className="panel">
          <h2>Risikoindikatorer</h2>

          {overview.riskIndicators.length === 0 ? (
            <p>Der er ingen aktuelle risikoindikatorer.</p>
          ) : (
            <div className="list">
              {overview.riskIndicators.map((indicator, index) => (
                <div className="list-item" key={`${indicator.type}-${index}`}>
                  <div>
                    <strong>{translateRiskType(indicator.type)}</strong>
                    <p>{translateRiskMessage(indicator.message)}</p>
                  </div>

                  <span className={`badge ${indicator.severity.toLowerCase()}`}>
                    {translateRiskLevel(indicator.severity)}
                  </span>
                </div>
              ))}
            </div>
          )}
        </article>

        <article className="panel">
          <h2>Manglende kompetencer</h2>

          {overview.uncoveredRequiredSkills.length === 0 ? (
            <p>Alle krævede kompetencer er dækket.</p>
          ) : (
            <div className="list">
              {overview.uncoveredRequiredSkills.map((skill) => (
                <div className="list-item" key={skill.skill}>
                  <div>
                    <strong>{skill.skill}</strong>
                    <p>
                      Kræves af {skill.requiredByUnassignedShifts} ubesat vagt.
                    </p>
                  </div>

                  <span className="badge high">
                    {skill.availableEmployees} tilgængelige
                  </span>
                </div>
              ))}
            </div>
          )}
        </article>

        <article className="panel">
          <h2>Kompetencekapacitet</h2>

          <div className="list">
            {overview.skillCapacity.map((skill) => (
              <div className="list-item" key={skill.skill}>
                <strong>{skill.skill}</strong>
                <span>{skill.employeeCount} medarbejder(e)</span>
              </div>
            ))}
          </div>
        </article>

        <WeeklyScheduleTable refreshKey={scheduleRefreshKey} />

        <article className="panel full-width-panel">
          <div className="panel-header">
            <div>
              <h2>Belastning pr. medarbejder</h2>
              <p>
                Overblik over samlet belastningsscore, kompetencer og aktuel
                belastningsstatus.
              </p>

              <div className="employee-load-summary">
                <div className="employee-load-summary-card">
                  <span>Høj belastning</span>
                  <strong>{highLoadEmployeeCount}</strong>
                </div>

                <div className="employee-load-summary-card">
                  <span>Mellem belastning</span>
                  <strong>{mediumLoadEmployeeCount}</strong>
                </div>

                <div className="employee-load-summary-card">
                  <span>Lav belastning</span>
                  <strong>{lowLoadEmployeeCount}</strong>
                </div>

                <div className="employee-load-summary-card">
                  <span>Gennemsnitlig score</span>
                  <strong>{averageLoadScore.toFixed(1).replace(".", ",")}</strong>
                </div>
              </div>
            </div>
          </div>

          {employeeLoadOverview.length === 0 ? (
            <p>Der er ingen medarbejderbelastning at vise.</p>
          ) : (
            <div className="employee-load-table-wrapper">
              <table className="employee-load-table">
                <thead>
                  <tr>
                    <th>Medarbejder</th>
                    <th>Kompetencer</th>
                    <th>Belastningsscore</th>
                    <th>Status</th>
                  </tr>
                </thead>

                <tbody>
                  {employeeLoadOverview.map((employee) => (
                    <tr
                      className={`clickable-row ${selectedEmployeeLoadDetails?.employeeId === employee.employeeId
                        ? "selected-row"
                        : ""
                        }`}
                      key={employee.employeeId}
                      onClick={() => handleEmployeeLoadClick(employee.employeeId)}
                    >
                      <td>
                        <strong>{employee.employeeName}</strong>
                      </td>

                      <td>
                        <div className="skill-chip-list">
                          {employee.skills.map((skill) => (
                            <span className="skill-chip" key={`${employee.employeeId}-${skill}`}>
                              {skill}
                            </span>
                          ))}
                        </div>
                      </td>

                      <td>
                        <strong>{employee.totalLoad}</strong>
                      </td>

                      <td>
                        <span className={`badge ${employee.loadStatus.toLowerCase()}`}>
                          {translateLoadStatus(employee.loadStatus)}
                        </span>
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}

          {isLoadingEmployeeDetails && (
            <p className="employee-load-details-message">
              Henter belastningsdetaljer...
            </p>
          )}

          {employeeDetailsErrorMessage && (
            <p className="error-text">{employeeDetailsErrorMessage}</p>
          )}

          {selectedEmployeeLoadDetails && (
            <div className="employee-load-details-panel">
              <div className="employee-load-details-header">
                <div>
                  <span>Valgt medarbejder</span>
                  <h3>{selectedEmployeeLoadDetails.employeeName}</h3>
                  <p>
                    Samlet belastningsscore:{" "}
                    <strong>{selectedEmployeeLoadDetails.totalLoad}</strong>
                  </p>
                </div>

                <span
                  className={`badge ${selectedEmployeeLoadDetails.loadStatus.toLowerCase()}`}
                >
                  {translateLoadStatus(selectedEmployeeLoadDetails.loadStatus)}
                </span>
              </div>

              {selectedEmployeeLoadDetails.assignedShifts.length === 0 ? (
                <p>Medarbejderen har ingen tildelte vagter i den aktuelle plan.</p>
              ) : (
                <div className="employee-load-details-list">
                  {selectedEmployeeLoadDetails.assignedShifts.map((shift) => (
                    <div className="employee-load-details-item" key={shift.shiftId}>
                      <div>
                        <strong>{shift.date}</strong>
                        <p>
                          {translateShiftType(shift.shiftType)} · {shift.requiredSkill}
                        </p>
                      </div>

                      <span>Score {shift.loadScore}</span>
                    </div>
                  ))}
                </div>
              )}
            </div>
          )}
        </article>

        <article className="panel full-width-panel">
          <h2>Ubesatte vagter</h2>

          {overview.unassignedShiftDetails.length === 0 ? (
            <p>Der er ingen ubesatte vagter.</p>
          ) : (
            <div className="unassigned-shift-list">
              {overview.unassignedShiftDetails.map((shift) => (
                <div
                  className={`unassigned-shift-card clickable-card ${selectedShiftAssignmentAnalysis?.shiftId === shift.shiftId
                    ? "selected-card"
                    : ""
                    }`}
                  key={shift.shiftId}
                  onClick={() => handleUnassignedShiftClick(shift.shiftId)}
                >
                  <div className="unassigned-shift-header">
                    <div>
                      <strong>{formatDate(shift.date)}</strong>
                      <p>
                        {translateShiftType(shift.shiftType)} · Kræver{" "}
                        {shift.requiredSkill}
                      </p>
                    </div>

                    <span className="badge high">Ubesat</span>
                  </div>

                  {shift.failureReasons.length > 0 && (
                    <div className="failure-reasons">
                      <span>Årsager</span>

                      <ul>
                        {shift.failureReasons.slice(0, 3).map((reason, index) => (
                          <li key={`${shift.shiftId}-${index}`}>
                            {translateFailureReason(reason)}
                          </li>
                        ))}
                      </ul>

                      {shift.failureReasons.length > 3 && (
                        <p className="more-reasons">
                          + {shift.failureReasons.length - 3} flere årsager
                        </p>
                      )}
                    </div>
                  )}
                </div>
              ))}
            </div>
          )}

          {isLoadingShiftAnalysis && (
            <p className="shift-analysis-message">
              Henter forklaring for ubesat vagt...
            </p>
          )}

          {shiftAnalysisErrorMessage && (
            <p className="error-text">{shiftAnalysisErrorMessage}</p>
          )}

          {selectedShiftAssignmentAnalysis && (
            <div className="shift-analysis-panel">
              <div className="shift-analysis-header">
                <div>
                  <span>Valgt ubesat vagt</span>
                  <h3>
                    {translateShiftType(selectedShiftAssignmentAnalysis.shiftType)} ·{" "}
                    {selectedShiftAssignmentAnalysis.requiredSkill}
                  </h3>
                  <p>{selectedShiftAssignmentAnalysis.date}</p>
                </div>

                <span
                  className={`badge ${selectedShiftAssignmentAnalysis.canBeCovered ? "low" : "high"
                    }`}
                >
                  {selectedShiftAssignmentAnalysis.canBeCovered
                    ? "Kan dækkes"
                    : "Kan ikke dækkes"}
                </span>
              </div>

              <div className="shift-analysis-summary">
                <strong>Overordnet forklaring</strong>

                <ul>
                  {selectedShiftAssignmentAnalysis.summaryReasons.map((reason) => (
                    <li key={reason}>{translateAssignmentReason(reason)}</li>
                  ))}
                </ul>
              </div>

              <div className="shift-analysis-candidates">
                <h4>Kandidater</h4>

                {selectedShiftAssignmentAnalysis.candidateResults.map((candidate) => (
                  <div
                    className="shift-analysis-candidate"
                    key={candidate.employeeId}
                  >
                    <div>
                      <strong>{candidate.employeeName}</strong>

                      {candidate.reasons.length === 0 ? (
                        <p>Kan tage vagten.</p>
                      ) : (
                        <ul>
                          {candidate.reasons.map((reason) => (
                            <li key={`${candidate.employeeId}-${reason}`}>
                              {translateAssignmentReason(reason)}
                            </li>
                          ))}
                        </ul>
                      )}
                    </div>

                    <span
                      className={`badge ${candidate.canBeAssigned ? "low" : "high"
                        }`}
                    >
                      {candidate.canBeAssigned ? "Mulig" : "Blokeret"}
                    </span>
                  </div>
                ))}
              </div>
            </div>
          )}
        </article>

        <SimulationPanel />

      </section>
    </main>
  );
}

export default App;