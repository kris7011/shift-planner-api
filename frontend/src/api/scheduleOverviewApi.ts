export type ScheduleRiskLevel = "Low" | "Medium" | "High";

export type RiskIndicator = {
    type: string;
    severity: ScheduleRiskLevel;
    message: string;
};

export type CapacitySummary = {
    totalSkills: number;
    missingRequiredSkills: number;
    criticalSkillGaps: number;
};

export type UncoveredRequiredSkill = {
    skill: string;
    requiredByUnassignedShifts: number;
    availableEmployees: number;
};

export type SkillCapacity = {
    skill: string;
    employeeCount: number;
};

export type ScheduleOverviewResponse = {
    totalShifts: number;
    assignedShifts: number;
    unassignedShifts: number;
    coverageRate: number;
    employeeCount: number;
    highRiskEmployeeCount: number;
    capacitySummary: CapacitySummary;
    riskIndicators: RiskIndicator[];
    uncoveredRequiredSkills: UncoveredRequiredSkill[];
    skillCapacity: SkillCapacity[];
};

const API_BASE_URL = "http://localhost:5026";

export async function getScheduleOverview(): Promise<ScheduleOverviewResponse> {
    const response = await fetch(`${API_BASE_URL}/api/schedule/overview`);

    if (!response.ok) {
        throw new Error("Failed to fetch schedule overview.");
    }

    return response.json();
}