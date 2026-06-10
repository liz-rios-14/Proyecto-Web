import axios from "axios";
import { reportFrontendError } from "./errorReporter";
import {
  clearAuthSession,
  getAuthToken,
  getRefreshToken,
  saveAuthSession,
} from "../services/authStorage";

const baseURL = import.meta.env.VITE_API_URL || "https://localhost:7101/api";
export const api = axios.create({ baseURL });
let refreshPromise = null;

api.interceptors.request.use((config) => {
  const token = getAuthToken();
  if (token) config.headers.Authorization = `Bearer ${token}`;
  return config;
});

const endSession = () => {
  clearAuthSession();
  if (window.location.pathname !== "/login") {
    window.location.replace("/login?reason=session-ended");
  }
};

const renewSession = async () => {
  const refreshToken = getRefreshToken();
  if (!refreshToken) throw new Error("No existe token de renovación.");

  const response = await axios.post(`${baseURL}/auth/refresh`, {
    refreshToken,
  });
  saveAuthSession(response.data);
  return response.data.accessToken || response.data.token;
};

api.interceptors.response.use(
  (response) => response,
  async (error) => {
    const originalRequest = error.config;
    const isAuthRequest = originalRequest?.url?.startsWith("/auth/");

    if (
      error.response?.status === 401 &&
      !isAuthRequest &&
      !originalRequest?._refreshAttempted
    ) {
      originalRequest._refreshAttempted = true;

      try {
        refreshPromise ??= renewSession().finally(() => {
          refreshPromise = null;
        });
        const accessToken = await refreshPromise;
        originalRequest.headers.Authorization = `Bearer ${accessToken}`;
        return api(originalRequest);
      } catch {
        endSession();
      }
    } else if (error.response?.status === 401 && !isAuthRequest) {
      endSession();
    }

    const shouldReport =
      originalRequest?.url !== "/error-logs" &&
      (!error.response || error.response.status >= 500);

    if (shouldReport) {
      reportFrontendError({
        source: "Frontend API",
        message:
          error.response?.data?.message ||
          error.message ||
          "Error al consumir la API.",
        detail: {
          method: originalRequest?.method,
          url: originalRequest?.url,
          status: error.response?.status,
          response: error.response?.data,
        },
      });
    }

    if (error.response?.status === 403 && !error.response.data?.message) {
      error.response.data = {
        message: "No tiene permisos para realizar esta operación.",
      };
    }

    return Promise.reject(error);
  }
);
