import { useState, type FormEvent } from "react";
import {
    confirmShiftImport,
    previewShiftImport,
    ShiftImportError,
    weeklyScheduleExportUrl,
    type ShiftImportIssue,
    type ShiftImportPreview,
    type ShiftImportRow,
} from "../api/shiftImportApi";

type ShiftImportPanelProps = {
    maxAssignmentsPerEmployee: number;
    defaultExportWeek: string;
    onImported: () => void;
};

const shiftTypeLabels: Record<string, string> = {
    Day: "Dag",
    Evening: "Aften",
    Night: "Nat",
    OnCall: "Vagt fra hjemmet",
};

const schedulingRuleMessages: Record<string, string> = {
    "Employee is already assigned to a shift on the same day.":
        "Medarbejderen har allerede en vagt samme dag.",
    "Employee cannot be assigned to a day shift directly after a night shift.":
        "Medarbejderen kan ikke tage en dagvagt lige efter en nattevagt.",
    "Employee has reached the maximum number of assignments.":
        "Medarbejderen har nået det maksimale antal vagter.",
    "Employee projected workload would be too high.":
        "Medarbejderens belastning ville blive for høj.",
};

function translateImportIssue(issue: ShiftImportIssue, row: ShiftImportRow): string {
    switch (issue.code) {
        case "FieldCount":
            return "Rækken har et forkert antal værdier. Tjek for et manglende eller ekstra skilletegn.";
        case "InvalidDate":
            return `'${row.date}' er ikke en dato i formatet ÅÅÅÅ-MM-DD.`;
        case "InvalidShiftType":
            return `'${row.shiftType}' er ikke en vagttype. Brug Day, Evening, Night eller OnCall.`;
        case "MissingRequiredSkill":
            return "Den krævede kompetence mangler.";
        case "ValueTooLong":
            return "En værdi er for lang.";
        case "UnsafeValue":
            return "En værdi starter med et tegn, som regneark opfatter som en formel.";
        case "UnknownEmployee":
            return `Ingen medarbejder hedder '${row.employeeName}'. Lad feltet være tomt for en ubesat vagt.`;
        case "AmbiguousEmployee":
            return `Flere medarbejdere hedder '${row.employeeName}'.`;
        case "EmployeeLacksSkill":
            return `${row.employeeName} har ikke kompetencen ${row.requiredSkill}.`;
        case "DuplicateRow":
            return `Dublet af række ${issue.relatedRowNumber}.`;
        case "AlreadyScheduled":
            return "Vagten findes allerede i vagtplanen.";
        case "SchedulingRule":
            return schedulingRuleMessages[issue.message] ?? issue.message;
        case "RepeatedOpenShift":
            return `Samme ubesatte vagt som række ${issue.relatedRowNumber}; hver række opretter sin egen vagt.`;
        case "OpenShiftExists":
            return "En identisk ubesat vagt findes allerede; rækken tilføjer endnu en.";
        case "SkillNotHeld":
            return `Ingen medarbejdere har kompetencen ${row.requiredSkill}, så vagten forbliver ubesat.`;
        default:
            return issue.message;
    }
}

function translateFileError(error: string): string {
    const alreadyImported = error.match(/already imported on (.+) UTC \((\d+) shifts\)/);

    if (alreadyImported) {
        return `Filen er allerede importeret ${alreadyImported[1]} UTC (${alreadyImported[2]} vagter). En ny import ville give dubletter.`;
    }

    if (error === "The file is empty.") {
        return "Filen er tom.";
    }

    if (error === "The file has a header but no shift rows.") {
        return "Filen har en overskrift, men ingen vagter.";
    }

    const missingColumn = error.match(/The required column '(.+)' is missing\./);

    if (missingColumn) {
        return `Kolonnen '${missingColumn[1]}' mangler.`;
    }

    const unknownColumn = error.match(/Unknown column '(.*)'\./);

    if (unknownColumn) {
        return `Ukendt kolonne '${unknownColumn[1]}'. Brug date, shiftType, requiredSkill og employee.`;
    }

    return error;
}

export function ShiftImportPanel({
    maxAssignmentsPerEmployee,
    defaultExportWeek,
    onImported,
}: ShiftImportPanelProps) {
    const [file, setFile] = useState<File | null>(null);
    const [preview, setPreview] = useState<ShiftImportPreview | null>(null);
    const [fileErrors, setFileErrors] = useState<string[]>([]);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [successMessage, setSuccessMessage] = useState<string | null>(null);
    const [isPreviewing, setIsPreviewing] = useState(false);
    const [isConfirming, setIsConfirming] = useState(false);
    const [exportWeek, setExportWeek] = useState(defaultExportWeek);
    // Changing the key remounts the file input, which clears the file it shows.
    const [fileInputKey, setFileInputKey] = useState(0);

    function resetResult() {
        setPreview(null);
        setFileErrors([]);
        setErrorMessage(null);
        setSuccessMessage(null);
    }

    async function handlePreview(event: FormEvent<HTMLFormElement>) {
        event.preventDefault();

        if (!file) {
            setErrorMessage("Vælg en CSV-fil først.");
            return;
        }

        resetResult();
        setIsPreviewing(true);

        try {
            const csv = await file.text();
            const result = await previewShiftImport(file.name, csv, maxAssignmentsPerEmployee);

            setPreview(result);
            setFileErrors(result.fileErrors);
        } catch (error) {
            if (error instanceof ShiftImportError && error.fileErrors.length > 0) {
                setFileErrors(error.fileErrors);
            } else {
                setErrorMessage("Filen kunne ikke forhåndsvises.");
            }
        } finally {
            setIsPreviewing(false);
        }
    }

    async function handleConfirm() {
        if (!preview || !preview.canConfirm) {
            return;
        }

        setIsConfirming(true);
        setErrorMessage(null);

        try {
            const result = await confirmShiftImport(preview.importId);

            setPreview(null);
            setFile(null);
            setFileInputKey((current) => current + 1);
            setSuccessMessage(`${result.createdShiftCount} vagter er gemt i vagtplanen.`);
            onImported();
        } catch (error) {
            // The schedule changed after the preview: show what is wrong now.
            if (error instanceof ShiftImportError && error.preview) {
                setPreview(error.preview);
                setFileErrors(error.preview.fileErrors);
                setErrorMessage("Vagtplanen er ændret siden forhåndsvisningen. Intet blev gemt.");
            } else if (error instanceof ShiftImportError) {
                setErrorMessage("Importen kunne ikke bekræftes. Intet blev gemt.");
            } else {
                setErrorMessage("Importen kunne ikke bekræftes.");
            }
        } finally {
            setIsConfirming(false);
        }
    }

    const shiftsToSave = preview ? preview.openShiftCount + preview.assignedShiftCount : 0;

    return (
        <section className="panel import-panel">
            <div className="section-header">
                <div>
                    <h2>Importér vagter fra CSV</h2>
                    <p className="simulation-helper-text">
                        Filen skal have kolonnerne date, shiftType, requiredSkill og employee
                        (tom for en ubesat vagt). Alle rækker kontrolleres med de samme regler
                        som vagtplanlægningen. Intet gemmes, før du bekræfter.
                    </p>
                </div>
            </div>

            <form className="import-form" onSubmit={handlePreview}>
                <label>
                    CSV-fil
                    <input
                        accept=".csv,text/csv"
                        key={fileInputKey}
                        onChange={(event) => {
                            setFile(event.target.files?.[0] ?? null);
                            resetResult();
                        }}
                        type="file"
                    />
                </label>

                <button disabled={isPreviewing} type="submit">
                    {isPreviewing ? "Kontrollerer..." : "Forhåndsvis import"}
                </button>
            </form>

            {errorMessage && <p className="error-text">{errorMessage}</p>}
            {successMessage && <p className="status-message">{successMessage}</p>}

            {fileErrors.length > 0 && (
                <div className="import-file-errors" role="alert">
                    <strong>Filen kan ikke importeres:</strong>
                    <ul>
                        {fileErrors.map((error) => (
                            <li key={error}>{translateFileError(error)}</li>
                        ))}
                    </ul>
                </div>
            )}

            {preview && (
                <div className="import-preview">
                    <div className="import-summary">
                        <span className={`badge ${preview.invalidRows === 0 ? "low" : "high"}`}>
                            {preview.validRows} af {preview.totalRows} rækker er gyldige
                        </span>
                        <span>
                            {preview.assignedShiftCount} besatte og {preview.openShiftCount} ubesatte vagter
                        </span>
                        {preview.warningCount > 0 && (
                            <span className="badge medium">{preview.warningCount} advarsler</span>
                        )}
                    </div>

                    <div className="import-table-wrapper">
                        <table className="import-table">
                            <caption>Forhåndsvisning af {preview.fileName}</caption>
                            <thead>
                                <tr>
                                    <th scope="col">Række</th>
                                    <th scope="col">Dato</th>
                                    <th scope="col">Vagt</th>
                                    <th scope="col">Kompetence</th>
                                    <th scope="col">Medarbejder</th>
                                    <th scope="col">Status</th>
                                </tr>
                            </thead>
                            <tbody>
                                {preview.rows.map((row) => (
                                    <tr
                                        className={row.isValid ? "" : "import-row-invalid"}
                                        key={row.rowNumber}
                                    >
                                        <td data-label="Række">{row.rowNumber}</td>
                                        <td className="import-date" data-label="Dato">{row.date}</td>
                                        <td data-label="Vagt">{shiftTypeLabels[row.shiftType] ?? row.shiftType}</td>
                                        <td data-label="Kompetence">{row.requiredSkill}</td>
                                        <td data-label="Medarbejder">{row.employeeName || <em>Ubesat</em>}</td>
                                        <td data-label="Status">
                                            <span className={`badge ${row.isValid ? "low" : "high"}`}>
                                                {row.isValid ? "Gyldig" : "Fejl"}
                                            </span>
                                            {[...row.errors, ...row.warnings].length > 0 && (
                                                <ul className="import-issues">
                                                    {row.errors.map((issue, index) => (
                                                        <li className="import-issue-error" key={`e${index}`}>
                                                            {translateImportIssue(issue, row)}
                                                        </li>
                                                    ))}
                                                    {row.warnings.map((issue, index) => (
                                                        <li className="import-issue-warning" key={`w${index}`}>
                                                            {translateImportIssue(issue, row)}
                                                        </li>
                                                    ))}
                                                </ul>
                                            )}
                                        </td>
                                    </tr>
                                ))}
                            </tbody>
                        </table>
                    </div>

                    <button
                        className="success-button"
                        disabled={!preview.canConfirm || isConfirming}
                        onClick={handleConfirm}
                        type="button"
                    >
                        {isConfirming
                            ? "Gemmer..."
                            : preview.canConfirm
                                ? `Bekræft og gem ${shiftsToSave} vagter`
                                : "Ret fejlene i filen for at kunne gemme"}
                    </button>
                </div>
            )}

            <div className="import-export">
                <h3>Eksportér ugeplan</h3>
                <p className="simulation-helper-text">
                    Ugen mandag-søndag, der indeholder datoen, i samme format som importen.
                </p>
                <div className="import-form">
                    <label>
                        Uge med dato
                        <input
                            onChange={(event) => setExportWeek(event.target.value)}
                            type="date"
                            value={exportWeek}
                        />
                    </label>
                    <a
                        className="button-link"
                        download
                        href={exportWeek ? weeklyScheduleExportUrl(exportWeek) : undefined}
                    >
                        Download CSV
                    </a>
                </div>
            </div>
        </section>
    );
}
