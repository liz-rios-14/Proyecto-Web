export function sanitizePersonNames(value, maxLength = 120) {
  return value
    .replace(/[^\p{L}\s]/gu, "")
    .replace(/^\s+/, "")
    .replace(/\s{2,}/g, " ")
    .toUpperCase()
    .slice(0, maxLength);
}

export function sanitizeUserName(value, maxLength = 60) {
  return value
    .replace(/\s/g, "")
    .replace(/[^\p{L}0-9._-]/gu, "")
    .toLowerCase()
    .slice(0, maxLength);
}

export function normalizeSpaces(value) {
  return value.trim().replace(/\s+/g, " ");
}

export function sanitizeSingleSpacedText(
  value,
  maxLength = 150,
  {
    uppercase = true,
    allowedPattern = /[^\p{L}0-9\s.,#/-]/gu,
    trimEnd = false,
  } = {}
) {
  const cleanValue = value
    .replace(allowedPattern, "")
    .replace(/^\s+/, "")
    .replace(/\s{2,}/g, " ");

  const normalizedValue = trimEnd ? cleanValue.trimEnd() : cleanValue;
  const casedValue = uppercase ? normalizedValue.toUpperCase() : normalizedValue;

  return casedValue.slice(0, maxLength);
}

export function sanitizeEmail(value, maxLength = 120) {
  return value
    .replace(/\s/g, "")
    .replace(/[^a-zA-Z0-9@._+-]/g, "")
    .toLowerCase()
    .slice(0, maxLength);
}

export function sanitizeDigits(value, maxLength = 20) {
  return value.replace(/\D/g, "").slice(0, maxLength);
}

export function sanitizeMoney(value, maxLength = 12) {
  const cleanValue = value.replace(/[^0-9.]/g, "");

  if (cleanValue.split(".").length > 2) {
    return null;
  }

  if (!/^\d{0,8}(\.\d{0,2})?$/.test(cleanValue)) {
    return null;
  }

  return cleanValue.slice(0, maxLength);
}

export function readStoredState(key, fallback) {
  try {
    const rawValue = sessionStorage.getItem(key);
    return rawValue ? { ...fallback, ...JSON.parse(rawValue) } : fallback;
  } catch {
    return fallback;
  }
}

export function writeStoredState(key, value) {
  sessionStorage.setItem(key, JSON.stringify(value));
}
