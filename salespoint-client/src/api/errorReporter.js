import { getAuthToken, getAuthUser } from "../services/authStorage";

const API_BASE_URL = import.meta.env.VITE_API_URL || "https://localhost:7101/api";
const MAX_DETAIL_LENGTH = 4000;

let lastReportKey = "";
let lastReportAt = 0;

function normalizeDetail(detail) {
  const text =
    typeof detail === "string"
      ? detail
      : JSON.stringify(detail, Object.getOwnPropertyNames(detail ?? {}), 2);

  return text.slice(0, MAX_DETAIL_LENGTH);
}

export function reportFrontendError({
  source = "Frontend",
  message = "Error no controlado en el frontend.",
  detail = "",
} = {}) {
  const token = getAuthToken();

  if (!token) return;

  const cleanMessage = String(message || "Error no controlado en el frontend.").trim();
  const reportKey = `${source}|${cleanMessage}`;
  const now = Date.now();

  if (reportKey === lastReportKey && now - lastReportAt < 5000) return;

  lastReportKey = reportKey;
  lastReportAt = now;

  const user = getAuthUser();
  const payload = {
    source,
    message: cleanMessage,
    detail: normalizeDetail({
      detail,
      url: window.location.href,
      userAgent: navigator.userAgent,
      user: user
        ? {
            userId: user.userId,
            userName: user.userName,
            roleName: user.roleName,
          }
        : null,
      createdAt: new Date().toISOString(),
    }),
  };

  fetch(`${API_BASE_URL}/error-logs`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${token}`,
    },
    body: JSON.stringify(payload),
    keepalive: true,
  }).catch(() => {
    lastReportAt = Date.now();
  });
}

export function installFrontendErrorReporting() {
  window.addEventListener("error", (event) => {
    reportFrontendError({
      source: "Frontend window.error",
      message: event.message,
      detail: {
        filename: event.filename,
        lineno: event.lineno,
        colno: event.colno,
        stack: event.error?.stack,
      },
    });
  });

  window.addEventListener("unhandledrejection", (event) => {
    const reason = event.reason;

    reportFrontendError({
      source: "Frontend unhandledrejection",
      message: reason?.message || String(reason || "Promesa rechazada sin manejar."),
      detail: reason?.stack || reason,
    });
  });
}
