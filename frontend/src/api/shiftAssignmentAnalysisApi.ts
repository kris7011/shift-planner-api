import { API_BASE_URL } from "./config";

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

export async function getShiftAssignmentAnalysis(
    shiftId: string,
    maxAssignmentsPerEmployee: number
): Promise<ShiftAssignmentAnalysisResponse> {
    const response = await fetch(
        `${API_BASE_URL}/api/shifts/${shiftId}/assignment-analysis?maxAssignmentsPerEmployee=${maxAssignmentsPerEmployee}`
    );

    if (!response.ok) {
        throw new Error("Kunne ikke hente forklaring for ubesat vagt.");
    }

    return response.json();
}