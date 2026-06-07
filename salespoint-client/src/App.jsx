import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import HomePage from "./pages/HomePage";
import CustomerManagerPage from "./pages/CustomerManagerPage";
import ProductManagerPage from "./pages/ProductManagerPage";
import SalesPointPage from "./pages/SalesPointPage";
import InvoiceHistoryPage from "./pages/InvoiceHistoryPage";
import LoginPage from "./pages/LoginPage";
import ForgotPasswordPage from "./pages/ForgotPasswordPage";
import ResetPasswordPage from "./pages/ResetPasswordPage";
import UserManagerPage from "./pages/UserManagerPage";
import RoleManagerPage from "./pages/RoleManagerPage";
import ErrorLogPage from "./pages/ErrorLogPage";
import ProtectedRoute from "./components/ProtectedRoute";

export default function App() {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LoginPage />} />
        <Route path="/forgot-password" element={<ForgotPasswordPage />} />
        <Route path="/reset-password" element={<ResetPasswordPage />} />

        <Route
          path="/"
          element={
            <ProtectedRoute>
              <HomePage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/customers"
          element={
            <ProtectedRoute allowedRoles={["ADMINISTRATOR", "SELLER"]}>
              <CustomerManagerPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/products"
          element={
            <ProtectedRoute allowedRoles={["ADMINISTRATOR"]}>
              <ProductManagerPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/sales"
          element={
            <ProtectedRoute
              allowedRoles={["SELLER"]}
              deniedMessage="No tiene permisos para registrar ventas. Esta función corresponde al rol Vendedor."
            >
              <SalesPointPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/invoices"
          element={
            <ProtectedRoute>
              <InvoiceHistoryPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/users"
          element={
            <ProtectedRoute allowedRoles={["ADMINISTRATOR"]}>
              <UserManagerPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/roles"
          element={
            <ProtectedRoute allowedRoles={["ADMINISTRATOR"]}>
              <RoleManagerPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/error-logs"
          element={
            <ProtectedRoute allowedRoles={["ADMINISTRATOR"]}>
              <ErrorLogPage />
            </ProtectedRoute>
          }
        />

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}
