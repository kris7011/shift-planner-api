export type DemoSeedResult = {
    wasSeeded: boolean;
    message: string;
    employeeCount: number;
    shiftCount: number;
};

const API_BASE_URL = "http://localhost:5026";

export async function resetDemoData(): Promise<DemoSeedResult> {
    const response = await fetch(`${API_BASE_URL}/api/demo/reset`, {
        method: "POST",
    });

    if (!response.ok) {
        throw new Error("Failed to reset demo data.");
    }

    return response.json();
}