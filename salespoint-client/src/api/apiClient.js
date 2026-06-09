import axios from "axios";
import { reportFrontendError } from "./errorReporter";
import { clearAuthSession, getAuthToken } from "../services/authStorage";

export const api = axios.create({
  baseURL: import.meta.env.VITE_API_URL || "https://localhost:7101/api",
});

api.interceptors.request.use((config) => {
  const token = getAuthToken();

  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }

  return config;
});

api.interceptors.response.use(
  (response) => response,
  (error) => {
    const shouldReport =
      error.config?.url !== "/error-logs" &&
      (!error.response || error.response.status >= 500);

    if (shouldReport) {
      reportFrontendError({
        source: "Frontend API",
        message:
          error.response?.data?.message ||
          error.message ||
          "Error al consumir la API.",
        detail: {
          method: error.config?.method,
          url: error.config?.url,
          status: error.response?.status,
          response: error.response?.data,
        },
      });
    }

    if (error.response?.status === 401) {
      clearAuthSession();

      if (window.location.pathname !== "/login") {
        window.location.replace("/login?reason=session-ended");
      }
    }

    if (
      error.response?.status === 403 &&
      !error.response.data?.message
    ) {
      error.response.data = {
        message: "No tiene permisos para realizar esta operación.",
      };
    }

    return Promise.reject(error);
  }
);
