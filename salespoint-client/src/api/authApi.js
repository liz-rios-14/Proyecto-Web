import { api } from "./apiClient";

export async function login(request) {
  const response = await api.post("/auth/login", request);
  return response.data;
}

export async function forgotPassword(request) {
  const response = await api.post("/auth/forgot-password", request);
  return response.data;
}

export async function resetPassword(request) {
  const response = await api.post("/auth/reset-password", request);
  return response.data;
}