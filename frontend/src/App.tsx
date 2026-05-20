import { useEffect, useState } from "react";
import "./App.css";
import {
  getScheduleOverview,
  type ScheduleOverviewResponse,
  type ScheduleRiskLevel,
} from "./api/scheduleOverviewApi";

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

function App() {
  const [overview, setOverview] = useState<ScheduleOverviewResponse | null>(null);
  const [isLoading, setIsLoading] = useState(true);
  const [errorMessage, setErrorMessage] = useState<string | null>(null);

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

        <button onClick={loadOverview}>Opdater data</button>
      </header>

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
                    <strong>{indicator.type}</strong>
                    <p>{indicator.message}</p>
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
      </section>
    </main>
  );
}

export default App;