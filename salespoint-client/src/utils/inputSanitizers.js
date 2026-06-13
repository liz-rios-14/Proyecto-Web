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
