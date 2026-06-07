import axios from "axios";
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
    if (error.response?.status === 401) {
      clearAuthSession();
    }

    return Promise.reject(error);
  }
);