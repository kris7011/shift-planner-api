export type ShiftType = "Day" | "Evening" | "Night" | "OnCall";

export type Employee = {
    id: string;
    name: string;
    skills: string[];
};

export type Shift = {
    id: string;
    employeeId: string | null;
    date: string;
    shiftType: ShiftType;
    requiredSkill: string;
    requiredStaff: number;
};

const API_BASE_URL = "http://localhost:5026";

export async function getEmployees(): Promise<Employee[]> {
    const response = await fetch(`${API_BASE_URL}/api/employees`);

    if (!response.ok) {
        throw new Error("Failed to fetch employees.");
    }

    return response.json();
}

export async function getShifts(): Promise<Shift[]> {
    const response = await fetch(`${API_BASE_URL}/api/shifts`);

    if (!response.ok) {
        throw new Error("Failed to fetch shifts.");
    }

    return response.json();
}