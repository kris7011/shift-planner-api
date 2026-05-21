export type ScheduleAssignmentResult = {
    shiftId: string;
    employeeId: string | null;
    employeeName: string | null;
    requiredSkill: string;
    wasAssigned: boolean;
    failureReasons: string[];
};

export type GenerateScheduleRequest = {
    maxAssignmentsPerEmployee: number;
};

export type GenerateScheduleResponse = {
    message: string;
    employeeCount: number;
    shiftCount: number;
    assignments: ScheduleAssignmentResult[];
};

const API_BASE_URL = "http://localhost:5026";

export async function generateSchedule(
    request: GenerateScheduleRequest,
): Promise<GenerateScheduleResponse> {
    const response = await fetch(`${API_BASE_URL}/api/schedule/generate`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(request),
    });

    if (!response.ok) {
        throw new Error("Failed to generate schedule.");
    }

    return response.json();
}