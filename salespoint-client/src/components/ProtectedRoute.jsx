import { Navigate } from "react-router-dom";
import { isAuthenticated } from "../services/authStorage";

export default function ProtectedRoute({ children }) {
  if (!isAuthenticated()) {
    return <Navigate to="/login" replace />;
  }

  return children;
}