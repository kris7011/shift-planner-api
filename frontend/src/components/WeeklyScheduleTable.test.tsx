import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import { deferred, fixtures, jsonResponse, stubApi } from "../test/fakeApi";
import { WeeklyScheduleTable } from "./WeeklyScheduleTable";

function summaryValue(label: string) {
  return screen.getByText(label).nextElementSibling;
}

function dayCard(dayLabel: string) {
  return screen.getByText(dayLabel).closest(".schedule-day-card") as HTMLElement;
}

describe("WeeklyScheduleTable", () => {
  it("places shifts on the right weekday and shows who is assigned", async () => {
    stubApi();

    render(<WeeklyScheduleTable refreshKey={0} />);

    expect(screen.getByText("Henter ugeplan...")).toBeInTheDocument();
    expect(await screen.findByText("Vagter i ugeplanen")).toBeInTheDocument();
    expect(summaryValue("Vagter i ugeplanen")).toHaveTextContent("14");
    expect(summaryValue("Tildelte vagter")).toHaveTextContent("4");
    expect(summaryValue("Ubesatte vagter")).toHaveTextContent("10");

    const monday = dayCard("Mandag");
    expect(within(monday).getByText("Tildelt: Kris")).toBeInTheDocument();
    expect(within(monday).getByText("Tildelt: Mette")).toBeInTheDocument();

    const friday = dayCard("Fredag");
    expect(within(friday).getByText("Kræver UL")).toBeInTheDocument();
    expect(within(friday).getAllByText("Ikke tildelt")).toHaveLength(2);

    expect(within(dayCard("Lørdag")).getAllByText("Weekend")).toHaveLength(1);
  });

  it("filters the week to only unassigned or only assigned shifts", async () => {
    stubApi();
    const user = userEvent.setup();

    render(<WeeklyScheduleTable refreshKey={0} />);
    await screen.findByText("Vagter i ugeplanen");

    await user.click(screen.getByRole("button", { name: "Kun ubesatte" }));
    expect(screen.queryByText(/^Tildelt: /)).not.toBeInTheDocument();
    expect(screen.getAllByText("Ikke tildelt")).toHaveLength(10);
    expect(within(dayCard("Mandag")).getByText("Ingen vagter")).toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Kun tildelte" }));
    expect(screen.queryByText("Ikke tildelt")).not.toBeInTheDocument();
    expect(screen.getAllByText(/^Tildelt: /)).toHaveLength(4);
  });

  it("keeps the current week visible while refreshing after refreshKey changes", async () => {
    let refreshed = false;
    const refreshedShifts = deferred<Response>();
    stubApi([
      (method, path) => {
        if (refreshed && method === "GET" && path === "/api/shifts") {
          return refreshedShifts.promise;
        }
      },
    ]);

    const { rerender } = render(<WeeklyScheduleTable refreshKey={0} />);
    await screen.findByText("Tildelt: Kris");

    refreshed = true;
    rerender(<WeeklyScheduleTable refreshKey={1} />);

    expect(screen.queryByText("Henter ugeplan...")).not.toBeInTheDocument();
    expect(screen.getByText("Tildelt: Kris")).toBeInTheDocument();

    const allAssignedToKris = fixtures.shifts.map((shift) => ({
      ...shift,
      employeeId: fixtures.employees[0].id,
    }));
    refreshedShifts.resolve(jsonResponse(allAssignedToKris));

    await waitFor(() => expect(summaryValue("Tildelte vagter")).toHaveTextContent("14"));
  });

  it("shows an error and can retry when the API is unavailable", async () => {
    let failing = true;
    stubApi([
      (_method, path) => {
        if (failing && path === "/api/shifts") {
          return new Response("Service unavailable", { status: 503 });
        }
      },
    ]);
    const user = userEvent.setup();

    render(<WeeklyScheduleTable refreshKey={0} />);

    expect(await screen.findByText("Kunne ikke hente vagtplanens skemadata.")).toBeInTheDocument();

    failing = false;
    await user.click(screen.getByRole("button", { name: "Prøv igen" }));

    expect(await screen.findByText("Tildelt: Kris")).toBeInTheDocument();
    expect(screen.queryByText("Kunne ikke hente vagtplanens skemadata.")).not.toBeInTheDocument();
  });
});
