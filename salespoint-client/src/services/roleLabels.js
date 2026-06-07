export function getRoleLabel(roleName) {
  const normalizedRole = String(roleName || "").trim().toUpperCase();

  if (normalizedRole === "ADMINISTRATOR") return "Administrador";
  if (normalizedRole === "SELLER") return "Vendedor";

  return roleName || "Sin rol";
}
