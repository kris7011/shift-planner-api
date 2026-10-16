import { render, screen, waitFor, within } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, expect, it } from "vitest";
import App from "./App";
import { deferred, fixtures, jsonResponse, stubApi } from "./test/fakeApi";

function preferencesSection() {
  return screen
    .getByRole("heading", { name: "Medarbejderpræferencer" })
    .closest("section") as HTMLElement;
}

describe("App", () => {
  it("renders the schedule overview with translated risk indicators", async () => {
    stubApi();

    render(<App />);

    expect(screen.getByText("Henter data...")).toBeInTheDocument();
    expect(await screen.findByText("Kræver opmærksomhed")).toBeInTheDocument();
    // The API formats the coverage rate with the server culture; a decimal point must survive translation.
    expect(screen.getByText("Vagtplanens dækningsgrad er 78.57%.")).toBeInTheDocument();
    expect(screen.getByText("3 vagt(er) er aktuelt ubesatte.")).toBeInTheDocument();
    expect(
      screen.getByText("UL kræves af 1 ubesat vagt, men ingen medarbejdere har denne kompetence."),
    ).toBeInTheDocument();
  });

  it("shows an error page and recovers when retrying", async () => {
    let failing = true;
    stubApi([
      (_method, path) => {
        if (failing && path === "/api/schedule/overview") {
          return new Response("Service unavailable", { status: 503 });
        }
      },
    ]);
    const user = userEvent.setup();

    render(<App />);

    expect(
      await screen.findByText("Kunne ikke hente vagtplanens overblik fra API'et."),
    ).toBeInTheDocument();

    failing = false;
    await user.click(screen.getByRole("button", { name: "Prøv igen" }));

    expect(await screen.findByText("Kræver opmærksomhed")).toBeInTheDocument();
  });

  it("keeps the dashboard visible while refreshing after generating a schedule", async () => {
    let generated = false;
    const refreshedOverview = deferred<Response>();
    const fetchMock = stubApi([
      (method, path, body) => {
        if (method === "POST" && path === "/api/schedule/generate") {
          expect(body).toEqual({ maxAssignmentsPerEmployee: 5 });
          generated = true;
          return jsonResponse({
            message: "Schedule generation completed.",
            employeeCount: 10,
            shiftCount: 14,
            assignments: [
              { shiftId: "1", employeeId: "e1", employeeName: "Jonas", requiredSkill: "CT", wasAssigned: true, failureReasons: [] },
              { shiftId: "2", employeeId: null, employeeName: null, requiredSkill: "UL", wasAssigned: false, failureReasons: [] },
            ],
          });
        }
        if (generated && method === "GET" && path === "/api/schedule/overview") {
          return refreshedOverview.promise;
        }
      },
    ]);
    const user = userEvent.setup();

    render(<App />);
    await user.click(await screen.findByRole("button", { name: "Generér vagtplan" }));

    await waitFor(() =>
      expect(fetchMock).toHaveBeenCalledWith(
        expect.stringContaining("/api/schedule/generate"),
        expect.objectContaining({ method: "POST" }),
      ),
    );
    expect(screen.queryByText("Henter data...")).not.toBeInTheDocument();
    expect(screen.getByRole("navigation", { name: "Dashboard navigation" })).toBeInTheDocument();

    refreshedOverview.resolve(jsonResponse(fixtures.scheduleOverview));

    expect(
      await screen.findByText(
        "Vagtplanen blev genereret. 1 vagt(er) blev tildelt, og 1 vagt(er) kunne ikke tildeles.",
      ),
    ).toBeInTheDocument();
  });

  it("reloads employee preference profiles after resetting demo data", async () => {
    let reset = false;
    stubApi([
      (method, path) => {
        if (method === "POST" && path === "/api/demo/reset") {
          reset = true;
          return jsonResponse({ wasSeeded: true, message: "Demo data was reset and seeded.", employeeCount: 10, shiftCount: 14 });
        }
        if (method === "GET" && path === "/api/employees/preference-profiles") {
          // An empty database has no employees and therefore no profiles until the reset.
          return jsonResponse(reset ? fixtures.preferenceProfiles : []);
        }
      },
    ]);
    const user = userEvent.setup();

    render(<App />);
    await user.click(await screen.findByRole("button", { name: /Medarbejdere/ }));
    expect(within(preferencesSection()).queryByText("Henrik")).not.toBeInTheDocument();

    await user.click(screen.getByRole("button", { name: "Nulstil demo-data" }));

    expect(
      await screen.findByText(/Demo-data blev nulstillet\. 10 medarbejdere og 14 vagter blev oprettet\./),
    ).toBeInTheDocument();
    expect(await within(preferencesSection()).findByText("Henrik")).toBeInTheDocument();
    expect(within(preferencesSection()).getByText("Undgår helst weekend")).toBeInTheDocument();
  });
});
