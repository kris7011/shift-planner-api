import { useEffect, useMemo, useState } from "react";
import {
    getEmployees,
    getShifts,
    type Employee,
    type Shift,
    type ShiftType,
} from "../api/scheduleDataApi";

type ScheduleFilter = "all" | "assigned" | "unassigned";

const weekDays = [
    { label: "Mandag", date: "2026-05-11" },
    { label: "Tirsdag", date: "2026-05-12" },
    { label: "Onsdag", date: "2026-05-13" },
    { label: "Torsdag", date: "2026-05-14" },
    { label: "Fredag", date: "2026-05-15" },
    { label: "Lørdag", date: "2026-05-16" },
    { label: "Søndag", date: "2026-05-17" },
];

type WeeklyScheduleTableProps = {
    refreshKey: number;
};

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

function formatDate(date: string) {
    return new Intl.DateTimeFormat("da-DK", {
        day: "2-digit",
        month: "2-digit",
    }).format(new Date(date));
}

export function WeeklyScheduleTable({ refreshKey }: WeeklyScheduleTableProps) {
    const [employees, setEmployees] = useState<Employee[]>([]);
    const [shifts, setShifts] = useState<Shift[]>([]);
    const [isLoading, setIsLoading] = useState(true);
    const [errorMessage, setErrorMessage] = useState<string | null>(null);
    const [scheduleFilter, setScheduleFilter] = useState<ScheduleFilter>("all");

    async function loadScheduleData() {
        try {
            setIsLoading(true);
            setErrorMessage(null);

            const [employeeData, shiftData] = await Promise.all([
                getEmployees(),
                getShifts(),
            ]);

            setEmployees(employeeData);
            setShifts(shiftData);
        } catch {
            setErrorMessage("Kunne ikke hente vagtplanens skemadata.");
        } finally {
            setIsLoading(false);
        }
    }

    useEffect(() => {
        loadScheduleData();
    }, [refreshKey]);

    const employeeNameById = useMemo(() => {
        return new Map(employees.map((employee) => [employee.id, employee.name]));
    }, [employees]);

    const shiftsByDate = useMemo(() => {
        return shifts.reduce<Record<string, Shift[]>>((result, shift) => {
            result[shift.date] = result[shift.date] ?? [];
            result[shift.date].push(shift);

            return result;
        }, {});
    }, [shifts]);

    const assignedShiftCount = shifts.filter((shift) => shift.employeeId).length;
    const unassignedShiftCount = shifts.length - assignedShiftCount;

    function shouldShowShift(shift: Shift) {
        if (scheduleFilter === "assigned") {
            return Boolean(shift.employeeId);
        }

        if (scheduleFilter === "unassigned") {
            return !shift.employeeId;
        }

        return true;
    }

    if (isLoading) {
        return (
            <article className="panel full-width-panel">
                <h2>Ugeplan</h2>
                <p>Henter ugeplan...</p>
            </article>
        );
    }

    if (errorMessage) {
        return (
            <article className="panel full-width-panel">
                <h2>Ugeplan</h2>
                <p className="error-text">{errorMessage}</p>
                <button onClick={loadScheduleData}>Prøv igen</button>
            </article>
        );
    }

    return (
        <article className="panel full-width-panel">
            <div className="section-header schedule-section-header">
                <div>
                    <h2>Ugeplan</h2>
                    <p>
                        Skemavisning fra mandag til søndag med tildelte og ubesatte vagter.
                    </p>
                </div>

                <div className="schedule-header-actions">
                    <div className="schedule-filter">
                        <button
                            className={scheduleFilter === "all" ? "active-filter" : ""}
                            onClick={() => setScheduleFilter("all")}
                            type="button"
                        >
                            Alle vagter
                        </button>

                        <button
                            className={scheduleFilter === "assigned" ? "active-filter" : ""}
                            onClick={() => setScheduleFilter("assigned")}
                            type="button"
                        >
                            Kun tildelte
                        </button>

                        <button
                            className={scheduleFilter === "unassigned" ? "active-filter" : ""}
                            onClick={() => setScheduleFilter("unassigned")}
                            type="button"
                        >
                            Kun ubesatte
                        </button>
                    </div>

                    <div className="schedule-legend">
                        <span>
                            <i className="legend-dot assigned-dot" />
                            Tildelt
                        </span>

                        <span>
                            <i className="legend-dot unassigned-dot" />
                            Ubesat
                        </span>
                    </div>
                </div>
            </div>

            <div className="weekly-schedule-summary">
                <div>
                    <span>Vagter i ugeplanen</span>
                    <strong>{shifts.length}</strong>
                </div>

                <div>
                    <span>Tildelte vagter</span>
                    <strong>{assignedShiftCount}</strong>
                </div>

                <div>
                    <span>Ubesatte vagter</span>
                    <strong>{unassignedShiftCount}</strong>
                </div>
            </div>

            <div className="weekly-schedule-grid">
                {weekDays.map((day) => {
                    const dayShifts = (shiftsByDate[day.date] ?? []).filter(shouldShowShift);

                    return (
                        <div className="schedule-day-card" key={day.date}>
                            <div className="schedule-day-header">
                                <strong>{day.label}</strong>
                                <span>{formatDate(day.date)}</span>
                            </div>

                            {dayShifts.length === 0 ? (
                                <p className="empty-day-text">Ingen vagter</p>
                            ) : (
                                <div className="schedule-shift-list">
                                    {dayShifts.map((shift) => {
                                        const employeeName = shift.employeeId
                                            ? employeeNameById.get(shift.employeeId) ??
                                            "Ukendt medarbejder"
                                            : null;

                                        return (
                                            <div
                                                className={`schedule-shift-card ${employeeName ? "assigned" : "unassigned"
                                                    }`}
                                                key={shift.id}
                                            >
                                                <div>
                                                    <strong>{translateShiftType(shift.shiftType)}</strong>
                                                    <p>Kræver {shift.requiredSkill}</p>
                                                    <p>
                                                        {employeeName
                                                            ? `Tildelt: ${employeeName}`
                                                            : "Ikke tildelt"}
                                                    </p>
                                                </div>

                                                <span
                                                    className={`badge ${employeeName ? "low" : "high"}`}
                                                >
                                                    {employeeName ? "Tildelt" : "Ubesat"}
                                                </span>
                                            </div>
                                        );
                                    })}
                                </div>
                            )}
                        </div>
                    );
                })}
            </div>
        </article>
    );
}