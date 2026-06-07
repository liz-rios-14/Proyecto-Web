const TOKEN_KEY = "salespoint-token";
const USER_KEY = "salespoint-user";

export function saveAuthSession(authData) {
  localStorage.setItem(TOKEN_KEY, authData.token);
  localStorage.setItem(USER_KEY, JSON.stringify(authData));
}

export function getAuthToken() {
  return localStorage.getItem(TOKEN_KEY);
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

export function clearAuthSession() {
  localStorage.removeItem(TOKEN_KEY);
  localStorage.removeItem(USER_KEY);
}

export function isAuthenticated() {
  return Boolean(getAuthToken());
}