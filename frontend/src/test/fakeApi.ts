import { vi } from "vitest";
import employees from "./fixtures/employees.json";
import loadOverview from "./fixtures/load-overview.json";
import preferenceProfiles from "./fixtures/preference-profiles.json";
import scheduleOverview from "./fixtures/schedule-overview.json";
import shifts from "./fixtures/shifts.json";

// Fixtures are real responses captured from the API after POST /api/demo/reset.
export const fixtures = {
  employees,
  shifts,
  scheduleOverview,
  loadOverview,
  preferenceProfiles,
};

export function jsonResponse(body: unknown, status = 200): Response {
  return new Response(JSON.stringify(body), {
    status,
    headers: { "Content-Type": "application/json" },
  });
}

type Route = (
  method: string,
  path: string,
  body: unknown,
) => Response | Promise<Response> | undefined;

const defaultGetResponses: Record<string, unknown> = {
  "/api/employees": employees,
  "/api/shifts": shifts,
  "/api/schedule/overview": scheduleOverview,
  "/api/employees/load-overview": loadOverview,
  "/api/employees/preference-profiles": preferenceProfiles,
};

/**
 * Replaces global fetch with an in-memory version of the API.
 * Custom routes are tried first; unmatched GET requests fall back to the fixtures.
 */
export function stubApi(routes: Route[] = []) {
  const fetchMock = vi.fn(async (input: RequestInfo | URL, init?: RequestInit) => {
    const url = new URL(input.toString());
    const method = init?.method ?? "GET";
    const body = init?.body ? JSON.parse(init.body.toString()) : undefined;

    for (const route of routes) {
      const response = route(method, url.pathname, body);
      if (response) {
        return response;
      }
    }

    if (method === "GET" && url.pathname in defaultGetResponses) {
      return jsonResponse(defaultGetResponses[url.pathname]);
    }

    return jsonResponse({ error: `No fake route for ${method} ${url.pathname}` }, 404);
  });

  vi.stubGlobal("fetch", fetchMock);

  return fetchMock;
}

export function deferred<T>() {
  let resolve: (value: T) => void = () => {};
  const promise = new Promise<T>((res) => {
    resolve = res;
  });

  return { promise, resolve };
}
