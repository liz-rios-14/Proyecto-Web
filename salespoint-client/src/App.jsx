import { BrowserRouter, Navigate, Route, Routes } from "react-router-dom";
import HomePage from "./pages/HomePage";
import CustomerManagerPage from "./pages/CustomerManagerPage";
import ProductManagerPage from "./pages/ProductManagerPage";
import SalesPointPage from "./pages/SalesPointPage";
import InvoiceHistoryPage from "./pages/InvoiceHistoryPage";
import LoginPage from "./pages/LoginPage";
import ForgotPasswordPage from "./pages/ForgotPasswordPage";
import ResetPasswordPage from "./pages/ResetPasswordPage";
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
            <ProtectedRoute>
              <CustomerManagerPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/products"
          element={
            <ProtectedRoute>
              <ProductManagerPage />
            </ProtectedRoute>
          }
        />

        <Route
          path="/sales"
          element={
            <ProtectedRoute>
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

        <Route path="*" element={<Navigate to="/" replace />} />
      </Routes>
    </BrowserRouter>
  );
}