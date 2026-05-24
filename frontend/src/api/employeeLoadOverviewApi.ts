export type EmployeeLoadOverviewItem = {
    employeeId: string;
    employeeName: string;
    skills: string[];
    totalLoad: number;
    loadStatus: string;
    isHighRisk: boolean;
};

const API_BASE_URL = "http://localhost:5026";

export async function getEmployeeLoadOverview(): Promise<EmployeeLoadOverviewItem[]> {
    const response = await fetch(`${API_BASE_URL}/api/employees/load-overview`);

    if (!response.ok) {
        throw new Error("Kunne ikke hente medarbejderbelastning.");
    }

    return response.json();
}