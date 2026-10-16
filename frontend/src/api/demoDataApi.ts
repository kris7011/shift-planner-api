import { API_BASE_URL } from "./config";

export type DemoSeedResult = {
    wasSeeded: boolean;
    message: string;
    employeeCount: number;
    shiftCount: number;
};

export async function resetDemoData(): Promise<DemoSeedResult> {
    const response = await fetch(`${API_BASE_URL}/api/demo/reset`, {
        method: "POST",
    });

    if (!response.ok) {
        throw new Error("Failed to reset demo data.");
    }

    return response.json();
}