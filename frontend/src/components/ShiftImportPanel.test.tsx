import { render, screen, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it, vi } from "vitest";
import type { ShiftImportPreview } from "../api/shiftImportApi";
import { jsonResponse, stubApi } from "../test/fakeApi";
import { ShiftImportPanel } from "./ShiftImportPanel";

const csv =
    "date,shiftType,requiredSkill,employee\n" +
    "2026-05-18,Day,CT,Kris\n" +
    "2026-05-18,Day,CT,Kris\n";

function preview(overrides: Partial<ShiftImportPreview> = {}): ShiftImportPreview {
    return {
        importId: "4f1c2b8e-0000-4000-8000-000000000001",
        fileName: "uge-21.csv",
        status: "Pending",
        canConfirm: true,
        maxAssignmentsPerEmployee: 5,
        totalRows: 2,
        validRows: 2,
        invalidRows: 0,
        warningCount: 1,
        openShiftCount: 1,
        assignedShiftCount: 1,
        fileErrors: [],
        rows: [
            {
                rowNumber: 2,
                date: "2026-05-18",
                shiftType: "Day",
                requiredSkill: "CT",
                employeeName: "Kris",
                employeeId: "a",
                isValid: true,
                errors: [],
                warnings: [],
            },
            {
                rowNumber: 3,
                date: "2026-05-19",
                shiftType: "Night",
                requiredSkill: "UL",
                employeeName: "",
                employeeId: null,
                isValid: true,
                errors: [],
                warnings: [{ code: "SkillNotHeld", message: "No employee has the skill 'UL'.", relatedRowNumber: null }],
            },
        ],
        ...overrides,
    };
}

function renderPanel(onImported = vi.fn()) {
    render(
        <ShiftImportPanel
            defaultExportWeek="2026-05-11"
            maxAssignmentsPerEmployee={5}
            onImported={onImported}
        />,
    );

    return { onImported, user: userEvent.setup() };
}

async function chooseFileAndPreview(user: ReturnType<typeof userEvent.setup>) {
    await user.upload(
        screen.getByLabelText("CSV-fil"),
        new File([csv], "uge-21.csv", { type: "text/csv" }),
    );
    await user.click(screen.getByRole("button", { name: "Forhåndsvis import" }));
}

describe("ShiftImportPanel", () => {
    it("sends the file for preview and saves only after confirmation", async () => {
        const requests: string[] = [];
        stubApi([
            (method, path, body) => {
                requests.push(`${method} ${path}`);

                if (path === "/api/shift-imports/preview") {
                    expect(body).toBe(csv);
                    return jsonResponse(preview());
                }

                if (path.endsWith("/confirm")) {
                    return jsonResponse({
                        importId: preview().importId,
                        status: "Confirmed",
                        createdShiftCount: 2,
                        confirmedAt: "2026-11-01T09:00:00Z",
                    });
                }
            },
        ]);
        const { onImported, user } = renderPanel();

        await chooseFileAndPreview(user);

        expect(await screen.findByText("2 af 2 rækker er gyldige")).toBeInTheDocument();
        expect(screen.getByText("Ingen medarbejdere har kompetencen UL, så vagten forbliver ubesat.")).toBeInTheDocument();
        expect(requests).toEqual(["POST /api/shift-imports/preview"]);
        expect(onImported).not.toHaveBeenCalled();

        await user.click(screen.getByRole("button", { name: "Bekræft og gem 2 vagter" }));

        expect(await screen.findByText("2 vagter er gemt i vagtplanen.")).toBeInTheDocument();
        expect(requests[1]).toBe(`POST /api/shift-imports/${preview().importId}/confirm`);
        expect(onImported).toHaveBeenCalledOnce();

        // The file input is cleared, so the same file can be chosen again.
        expect((screen.getByLabelText("CSV-fil") as HTMLInputElement).files).toHaveLength(0);
    });

    it("shows row errors in Danish and does not allow saving", async () => {
        stubApi([
            (_method, path) =>
                path === "/api/shift-imports/preview"
                    ? jsonResponse(
                          preview({
                              canConfirm: false,
                              validRows: 1,
                              invalidRows: 1,
                              rows: [
                                  preview().rows[0],
                                  {
                                      ...preview().rows[0],
                                      rowNumber: 3,
                                      isValid: false,
                                      errors: [{ code: "DuplicateRow", message: "Duplicate of row 2.", relatedRowNumber: 2 }],
                                  },
                              ],
                          }),
                      )
                    : undefined,
        ]);
        const { user } = renderPanel();

        await chooseFileAndPreview(user);

        const invalidRow = (await screen.findByText("Dublet af række 2.")).closest("tr") as HTMLElement;
        expect(within(invalidRow).getByText("Fejl")).toBeInTheDocument();
        expect(screen.getByRole("button", { name: "Ret fejlene i filen for at kunne gemme" })).toBeDisabled();
    });

    it("explains a file that cannot be read", async () => {
        stubApi([
            (_method, path) =>
                path === "/api/shift-imports/preview"
                    ? jsonResponse(
                          {
                              error: "The file could not be read as a shift import.",
                              fileErrors: ["The required column 'shiftType' is missing."],
                          },
                          400,
                      )
                    : undefined,
        ]);
        const { user } = renderPanel();

        await chooseFileAndPreview(user);

        expect(await screen.findByRole("alert")).toHaveTextContent("Kolonnen 'shiftType' mangler.");
    });

    it("shows the fresh result when the schedule changed before confirming", async () => {
        const changed = preview({
            canConfirm: false,
            validRows: 1,
            invalidRows: 1,
            rows: [
                {
                    ...preview().rows[0],
                    isValid: false,
                    errors: [
                        {
                            code: "SchedulingRule",
                            message: "Employee is already assigned to a shift on the same day.",
                            relatedRowNumber: null,
                        },
                    ],
                },
                preview().rows[1],
            ],
        });
        stubApi([
            (_method, path) => {
                if (path === "/api/shift-imports/preview") {
                    return jsonResponse(preview());
                }

                if (path.endsWith("/confirm")) {
                    return jsonResponse({ error: "The import cannot be confirmed.", preview: changed }, 409);
                }
            },
        ]);
        const { onImported, user } = renderPanel();

        await chooseFileAndPreview(user);
        await user.click(await screen.findByRole("button", { name: "Bekræft og gem 2 vagter" }));

        expect(await screen.findByText("Vagtplanen er ændret siden forhåndsvisningen. Intet blev gemt.")).toBeInTheDocument();
        expect(screen.getByText("Medarbejderen har allerede en vagt samme dag.")).toBeInTheDocument();
        expect(onImported).not.toHaveBeenCalled();
    });

    it("links to the CSV export of the chosen week", async () => {
        stubApi();
        const { user } = renderPanel();

        const link = screen.getByRole("link", { name: "Download CSV" });
        expect(link).toHaveAttribute("href", expect.stringContaining("/api/schedule/export?weekStart=2026-05-11"));

        const weekInput = screen.getByLabelText("Uge med dato");
        await user.clear(weekInput);
        await user.type(weekInput, "2026-05-20");

        expect(link).toHaveAttribute("href", expect.stringContaining("weekStart=2026-05-20"));
    });
});
