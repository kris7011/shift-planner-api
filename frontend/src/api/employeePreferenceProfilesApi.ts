import type { ShiftType } from "./scheduleSimulationApi";

export type EmployeePreferenceProfileOverviewItem = {
    employeeId: string;
    employeeName: string;
    preferredShiftTypes: ShiftType[];
    dislikedShiftTypes: ShiftType[];
    maxNightShifts: number | null;
    maxEveningShifts: number | null;
    prefersWeekends: boolean;
    avoidsWeekends: boolean;
};

const API_BASE_URL = "http://localhost:5026";

export async function getEmployeePreferenceProfiles(): Promise<
    EmployeePreferenceProfileOverviewItem[]
> {
    const response = await fetch(`${API_BASE_URL}/api/employees/preference-profiles`);

    if (!response.ok) {
        throw new Error("Kunne ikke hente medarbejderpræferencer.");
    }

    return response.json();
}