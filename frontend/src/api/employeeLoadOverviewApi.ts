import { API_BASE_URL } from "./config";

export type EmployeeLoadOverviewItem = {
    employeeId: string;
    employeeName: string;
    skills: string[];
    totalLoad: number;
    loadStatus: string;
    isHighRisk: boolean;
};

export async function getEmployeeLoadOverview(): Promise<EmployeeLoadOverviewItem[]> {
    const response = await fetch(`${API_BASE_URL}/api/employees/load-overview`);

    if (!response.ok) {
        throw new Error("Kunne ikke hente medarbejderbelastning.");
    }

    return response.json();
}