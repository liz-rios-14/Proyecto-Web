const TOKEN_KEY = "salespoint-token";
const REFRESH_TOKEN_KEY = "salespoint-refresh-token";
const USER_KEY = "salespoint-user";

export function saveAuthSession(authData) {
  const accessToken = authData.accessToken || authData.token;
  localStorage.setItem(TOKEN_KEY, accessToken);
  localStorage.setItem(REFRESH_TOKEN_KEY, authData.refreshToken || "");
  localStorage.setItem(
    USER_KEY,
    JSON.stringify({ ...authData, token: accessToken, accessToken })
  );
}

export function getAuthToken() {
  return localStorage.getItem(TOKEN_KEY);
}

export function getRefreshToken() {
  return localStorage.getItem(REFRESH_TOKEN_KEY);
}

export function getAuthUser() {
  const rawUser = localStorage.getItem(USER_KEY);
  if (!rawUser) return null;

  try {
    return JSON.parse(rawUser);
  } catch {
    clearAuthSession();
    return null;
  }
}

export function getAuthRole() {
  const user = getAuthUser();
  const rawRole =
    user?.roleName ??
    (typeof user?.role === "string" ? user.role : user?.role?.name) ??
    "";

  return String(rawRole).trim().toUpperCase();
}

export function clearAuthSession() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(REFRESH_TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}

export function isAuthenticated() {
  return Boolean(getAuthToken() && getAuthUser());
}
