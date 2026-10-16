// Base URL of the ShiftPlanner API. Override with VITE_API_BASE_URL (see .env.example).
export const API_BASE_URL: string =
    import.meta.env.VITE_API_BASE_URL ?? "http://localhost:5026";
