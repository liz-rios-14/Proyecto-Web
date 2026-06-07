import { Navigate } from "react-router-dom";
import { getAuthUser, isAuthenticated } from "../services/authStorage";

export default function ProtectedRoute({ children, allowedRoles = [] }) {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />;
  }

  const roleName = getAuthUser()?.roleName?.toUpperCase();

  if (
    allowedRoles.length > 0 &&
    !allowedRoles.some((role) => role.toUpperCase() === roleName)
  ) {
    return <Navigate to="/" replace />;
  }

  return children;
}
