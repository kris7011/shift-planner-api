import { useState } from "react";
import {
    simulateSchedule,
    type ScheduleRiskLevel,
    type ShiftType,
    type SimulateScheduleResponse,
} from "../api/scheduleSimulationApi";

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

function translateShiftType(shiftType: ShiftType) {
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

function translateImpactIndicatorType(type: string) {
    switch (type) {
        case "Coverage":
            return "Dækning";
        case "Skill":
            return "Kompetence";
        case "RestRule":
            return "Hviletidsregel";
        default:
            return type;
    }
}

function translateImpactMessage(message: string) {
    if (message === "The simulated shift can be covered.") {
        return "Den simulerede vagt kan dækkes.";
    }

    if (message === "The simulated shift cannot be covered.") {
        return "Den simulerede vagt kan ikke dækkes.";
    }

    if (message.includes("No available employee can satisfy the required skill")) {
        const skill = message
            .replace("No available employee can satisfy the required skill", "")
            .replace(".", "")
            .replaceAll("'", "")
            .trim();

        return `Ingen tilgængelige medarbejdere matcher den krævede kompetence ${skill}.`;
    }

    if (message.includes("conflicts with rest-time or shift sequence rules")) {
        return "Den simulerede vagt konflikter med hviletid eller vagtrækkefølge.";
    }

    return message;
}

function translateFailureReason(reason: string) {
    if (reason.includes("Missing required skill")) {
        const skill = reason
            .replace("Missing required skill", "")
            .replace(".", "")
            .replaceAll("'", "")
            .trim();

        return `Mangler krævet kompetence ${skill}.`;
    }

    return reason;
}

function translateImpactSummary(summary: string) {
    if (summary.includes("This shift can be covered by")) {
        const employeeName = summary
            .replace("This shift can be covered by", "")
            .replace("with low scheduling risk.", "")
            .trim();

        return `Vagten kan dækkes af ${employeeName} med lav planlægningsrisiko.`;
    }

    if (summary.includes("This shift cannot be covered")) {
        return "Vagten kan ikke dækkes, fordi ingen tilgængelige medarbejdere matcher kravene og planlægningsreglerne.";
    }

    return summary;
}

export function SimulationPanel() {
    const [date, setDate] = useState("2026-05-15");
    const [shiftType, setShiftType] = useState<ShiftType>("Day");
    const [requiredSkill, setRequiredSkill] = useState("UL");
    const [requiredStaff, setRequiredStaff] = useState(1);
    const [maxAssignmentsPerEmployee, setMaxAssignmentsPerEmployee] = useState(5);

    const [result, setResult] = useState<SimulateScheduleResponse | null>(null);
    const [isSimulating, setIsSimulating] = useState(false);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);

    async function handleSubmit(event: React.FormEvent<HTMLFormElement>) {
        event.preventDefault();

        try {
            setIsSimulating(true);
            setErrorMessage(null);

            const simulationResult = await simulateSchedule({
                date,
                shiftType,
                requiredSkill,
                requiredStaff,
                maxAssignmentsPerEmployee,
            });

            setResult(simulationResult);
        } catch {
            setErrorMessage("Kunne ikke simulere vagten. Kontroller at API'et kører.");
        } finally {
            setIsSimulating(false);
        }
    }

    return (
        <section className="panel simulation-panel">
            <div className="section-header">
                <div>
                    <h2>Simulér vagt</h2>
                    <p>
                        Test om en tænkt vagt kan dækkes, før den gemmes i vagtplanen.
                    </p>
                </div>
            </div>

            <form className="simulation-form" onSubmit={handleSubmit}>
                <label>
                    Dato
                    <input
                        type="date"
                        value={date}
                        onChange={(event) => setDate(event.target.value)}
                    />
                </label>

                <label>
                    Vagttype
                    <select
                        value={shiftType}
                        onChange={(event) => setShiftType(event.target.value as ShiftType)}
                    >
                        <option value="Day">{translateShiftType("Day")}</option>
                        <option value="Evening">{translateShiftType("Evening")}</option>
                        <option value="Night">{translateShiftType("Night")}</option>
                        <option value="OnCall">{translateShiftType("OnCall")}</option>
                    </select>
                </label>

                <label>
                    Krævet kompetence
                    <input
                        type="text"
                        value={requiredSkill}
                        onChange={(event) => setRequiredSkill(event.target.value)}
                    />
                </label>

                <label>
                    Krævet bemanding
                    <input
                        min="1"
                        type="number"
                        value={requiredStaff}
                        onChange={(event) => setRequiredStaff(Number(event.target.value))}
                    />
                </label>

                <label>
                    Maks vagter pr. medarbejder
                    <input
                        min="1"
                        type="number"
                        value={maxAssignmentsPerEmployee}
                        onChange={(event) =>
                            setMaxAssignmentsPerEmployee(Number(event.target.value))
                        }
                    />
                </label>

                <button disabled={isSimulating} type="submit">
                    {isSimulating ? "Simulerer..." : "Simulér vagt"}
                </button>
            </form>

            {errorMessage && <p className="error-text">{errorMessage}</p>}

            {result && (
                <div className="simulation-result">
                    <div className="simulation-summary">
                        <div>
                            <span>Resultat</span>
                            <strong>
                                {result.canBeCovered ? "Vagten kan dækkes" : "Vagten kan ikke dækkes"}
                            </strong>
                        </div>

                        <span className={`badge ${result.riskLevel.toLowerCase()}`}>
                            {translateRiskLevel(result.riskLevel)}
                        </span>
                    </div>

                    <p>{translateImpactSummary(result.impactSummary)}</p>

                    {result.suggestedEmployeeName && (
                        <div className="suggested-employee-card">
                            <span>Foreslået medarbejder</span>

                            <div className="suggested-employee-content">
                                <strong>{result.suggestedEmployeeName}</strong>

                                <span className="badge low">
                                    {translateRiskLevel(result.riskLevel)} risiko
                                </span>
                            </div>

                            <p>
                                Systemet vurderer denne medarbejder som bedste kandidat ud fra
                                kompetencer, eksisterende vagter og planlægningsregler.
                            </p>
                        </div>
                    )}

                    <h3>Simuleringsindikatorer</h3>

                    <div className="list">
                        {result.impactIndicators.map((indicator, index) => (
                            <div className="list-item" key={`${indicator.type}-${index}`}>
                                <div>
                                    <strong>{translateImpactIndicatorType(indicator.type)}</strong>
                                    <p>{translateImpactMessage(indicator.message)}</p>
                                </div>

                                <span className={`badge ${indicator.severity.toLowerCase()}`}>
                                    {translateRiskLevel(indicator.severity)}
                                </span>
                            </div>
                        ))}
                    </div>

                    <h3>Kandidater</h3>

                    <div className="list">
                        {result.candidateResults
                            .toSorted((firstCandidate, secondCandidate) => {
                                if (firstCandidate.canBeAssigned !== secondCandidate.canBeAssigned) {
                                    return firstCandidate.canBeAssigned ? -1 : 1;
                                }

                                return secondCandidate.score - firstCandidate.score;
                            })
                            .slice(0, 5)
                            .map((candidate) => (
                                <div className="list-item" key={candidate.employeeId}>
                                    <div>
                                        <strong>{candidate.employeeName}</strong>
                                        <p>
                                            {candidate.canBeAssigned
                                                ? "Kan tage vagten."
                                                : candidate.reasons.map(translateFailureReason).join(" ")}
                                        </p>
                                    </div>

                                    <span
                                        className={`badge ${candidate.canBeAssigned ? "low" : "high"
                                            }`}
                                    >
                                        Score {candidate.score}
                                    </span>
                                </div>
                            ))}

                        {result.candidateResults.length > 5 && (
                            <p className="more-reasons">
                                + {result.candidateResults.length - 5} flere kandidater
                            </p>
                        )}
                    </div>
                </div>
            )}
        </section>
    );
}