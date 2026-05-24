export type EmployeeAssignedShiftLoadItem = {
    shiftId: string;
    date: string;
    shiftType: string;
    requiredSkill: string;
    loadScore: number;
};

export type EmployeeLoadDetailsResponse = {
    employeeId: string;
    employeeName: string;
    skills: string[];
    totalLoad: number;
    loadStatus: string;
    isHighRisk: boolean;
    assignedShifts: EmployeeAssignedShiftLoadItem[];
};

const API_BASE_URL = "http://localhost:5026";

export async function getEmployeeLoadDetails(
    employeeId: string
): Promise<EmployeeLoadDetailsResponse> {
    const response = await fetch(
        `${API_BASE_URL}/api/employees/${employeeId}/load-details`
    );

    if (!response.ok) {
        throw new Error("Kunne ikke hente medarbejderens belastningsdetaljer.");
    }

    return response.json();
}