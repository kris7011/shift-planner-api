import { API_BASE_URL } from "./config";

export type ShiftImportIssue = {
    code: string;
    message: string;
    relatedRowNumber: number | null;
};

export type ShiftImportRow = {
    rowNumber: number;
    date: string;
    shiftType: string;
    requiredSkill: string;
    employeeName: string;
    employeeId: string | null;
    isValid: boolean;
    errors: ShiftImportIssue[];
    warnings: ShiftImportIssue[];
};

export type ShiftImportPreview = {
    importId: string;
    fileName: string;
    status: "Pending" | "Confirmed";
    canConfirm: boolean;
    maxAssignmentsPerEmployee: number;
    totalRows: number;
    validRows: number;
    invalidRows: number;
    warningCount: number;
    openShiftCount: number;
    assignedShiftCount: number;
    fileErrors: string[];
    rows: ShiftImportRow[];
};

export type ShiftImportConfirmation = {
    importId: string;
    status: "Confirmed";
    createdShiftCount: number;
    confirmedAt: string;
};

/** An error the API explained; the message and file errors can be shown to the user. */
export class ShiftImportError extends Error {
    readonly fileErrors: string[];
    readonly preview: ShiftImportPreview | null;

    constructor(message: string, fileErrors: string[] = [], preview: ShiftImportPreview | null = null) {
        super(message);
        this.name = "ShiftImportError";
        this.fileErrors = fileErrors;
        this.preview = preview;
    }
}

async function readError(response: Response, fallback: string): Promise<ShiftImportError> {
    try {
        const body = await response.json();

        return new ShiftImportError(body.error ?? fallback, body.fileErrors ?? [], body.preview ?? null);
    } catch {
        return new ShiftImportError(fallback);
    }
}

export async function previewShiftImport(
    fileName: string,
    csv: string,
    maxAssignmentsPerEmployee: number,
): Promise<ShiftImportPreview> {
    const query = new URLSearchParams({
        fileName,
        maxAssignmentsPerEmployee: String(maxAssignmentsPerEmployee),
    });

    const response = await fetch(`${API_BASE_URL}/api/shift-imports/preview?${query}`, {
        method: "POST",
        headers: {
            "Content-Type": "text/csv",
        },
        body: csv,
    });

    if (!response.ok) {
        throw await readError(response, "Failed to preview the import.");
    }

    return response.json();
}

export async function confirmShiftImport(importId: string): Promise<ShiftImportConfirmation> {
    const response = await fetch(`${API_BASE_URL}/api/shift-imports/${importId}/confirm`, {
        method: "POST",
    });

    if (!response.ok) {
        throw await readError(response, "Failed to confirm the import.");
    }

    return response.json();
}

export function weeklyScheduleExportUrl(weekStart: string): string {
    return `${API_BASE_URL}/api/schedule/export?weekStart=${encodeURIComponent(weekStart)}`;
}
