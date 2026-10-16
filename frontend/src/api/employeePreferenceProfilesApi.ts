import type { ShiftType } from "./scheduleSimulationApi";
import { API_BASE_URL } from "./config";

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

export async function getEmployeePreferenceProfiles(): Promise<
    EmployeePreferenceProfileOverviewItem[]
> {
    const response = await fetch(`${API_BASE_URL}/api/employees/preference-profiles`);

    if (!response.ok) {
        throw new Error("Kunne ikke hente medarbejderpræferencer.");
    }

    return response.json();
}