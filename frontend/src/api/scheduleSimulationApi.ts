export type ScheduleRiskLevel = "Low" | "Medium" | "High";

export type ShiftType = "Day" | "Evening" | "Night" | "OnCall";

export type SimulateScheduleRequest = {
    date: string;
    shiftType: ShiftType;
    requiredSkill: string;
    requiredStaff: number;
    maxAssignmentsPerEmployee: number;
};

export type SimulationImpactIndicator = {
    type: string;
    severity: ScheduleRiskLevel;
    message: string;
};

export type SimulationCandidateResult = {
    employeeId: string;
    employeeName: string;
    canBeAssigned: boolean;
    score: number;
    reasons: string[];
};

export type SimulateScheduleResponse = {
    canBeCovered: boolean;
    requiredSkill: string;
    riskLevel: ScheduleRiskLevel;
    suggestedEmployeeId: string | null;
    suggestedEmployeeName: string | null;
    failureReasons: string[];
    impactSummary: string;
    impactIndicators: SimulationImpactIndicator[];
    candidateResults: SimulationCandidateResult[];
};

const API_BASE_URL = "http://localhost:5026";

export async function simulateSchedule(
    request: SimulateScheduleRequest,
): Promise<SimulateScheduleResponse> {
    const response = await fetch(`${API_BASE_URL}/api/schedule/simulate`, {
        method: "POST",
        headers: {
            "Content-Type": "application/json",
        },
        body: JSON.stringify(request),
    });

    if (!response.ok) {
        throw new Error("Failed to simulate schedule.");
    }

    return response.json();
}