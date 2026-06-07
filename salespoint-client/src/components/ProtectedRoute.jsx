import { Navigate } from "react-router-dom";
import { getAuthRole, isAuthenticated } from "../services/authStorage";

export default function ProtectedRoute({
  children,
  allowedRoles = [],
  deniedMessage = "No tiene permisos para acceder a esta opción.",
}) {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />;
  }

  const roleName = getAuthRole();

  if (
    allowedRoles.length > 0 &&
    !allowedRoles.some((role) => role.toUpperCase() === roleName)
  ) {
    return (
      <Navigate
        to="/"
        replace
        state={{ permissionMessage: deniedMessage }}
      />
    );
  }

  return children;
}
