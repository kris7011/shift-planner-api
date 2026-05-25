export type ShiftAssignmentCandidateResult = {
    employeeId: string;
    employeeName: string;
    canBeAssigned: boolean;
    reasons: string[];
};

export type ShiftAssignmentAnalysisResponse = {
    shiftId: string;
    date: string;
    shiftType: string;
    requiredSkill: string;
    isAssigned: boolean;
    canBeCovered: boolean;
    summaryReasons: string[];
    candidateResults: ShiftAssignmentCandidateResult[];
};

const API_BASE_URL = "http://localhost:5026";

export async function getShiftAssignmentAnalysis(
    shiftId: string
): Promise<ShiftAssignmentAnalysisResponse> {
    const response = await fetch(
        `${API_BASE_URL}/api/shifts/${shiftId}/assignment-analysis`
    );

    if (!response.ok) {
        throw new Error("Kunne ikke hente forklaring for ubesat vagt.");
    }

    return response.json();
}