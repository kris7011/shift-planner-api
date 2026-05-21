import { useEffect, useState } from "react";
import "./App.css";
import {
  getScheduleOverview,
  type ScheduleOverviewResponse,
  type ScheduleRiskLevel,
} from "./api/scheduleOverviewApi";
import { resetDemoData } from "./api/demoDataApi";
import { SimulationPanel } from "./components/SimulationPanel";

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

function App() {
  const [overview, setOverview] = useState<ScheduleOverviewResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);
  const [isResettingDemoData, setIsResettingDemoData] = useState(false);
  const [statusMessage, setStatusMessage] = useState<string | null>(null);

  async function loadOverview() {
    try {
      setIsLoading(true);
      setErrorMessage(null);

      const data = await getScheduleOverview();

      setOverview(data);
    } catch {
      setErrorMessage("Kunne ikke hente vagtplanens overblik fra API'et.");
    } finally {
      setIsLoading(false);
    }
  }

  async function handleResetDemoData() {
    try {
      setIsResettingDemoData(true);
      setErrorMessage(null);
      setStatusMessage(null);

      const result = await resetDemoData();

      setStatusMessage(
        `Demo-data blev nulstillet. ${result.employeeCount} medarbejdere og ${result.shiftCount} vagter blev oprettet.`,
      );

      await loadOverview();
    } catch {
      setErrorMessage("Kunne ikke nulstille demo-data. Kontroller at API'et kører.");
    } finally {
      setIsResettingDemoData(false);
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
          <button onClick={loadOverview}>Opdater data</button>

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

        <article className="panel full-width-panel">
          <h2>Ubesatte vagter</h2>

          {overview.unassignedShiftDetails.length === 0 ? (
            <p>Der er ingen ubesatte vagter.</p>
          ) : (
            <div className="unassigned-shift-list">
              {overview.unassignedShiftDetails.map((shift) => (
                <div className="unassigned-shift-card" key={shift.shiftId}>
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
        </article>

        <SimulationPanel />

      </section>
    </main>
  );
}

export default App;