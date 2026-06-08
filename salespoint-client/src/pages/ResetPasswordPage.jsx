import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { resetPassword } from "../api/authApi";
import { getApiErrorMessage } from "../api/apiError";
import { useAppAlert } from "../components/AppAlert";
import AuthLayout from "../components/AuthLayout";

export default function ResetPasswordPage() {
  const navigate = useNavigate();
  const { showAlert } = useAppAlert();

  const [form, setForm] = useState({
    email: "",
    resetToken: "",
    newPassword: "",
  });

  const [error, setError] = useState("");
  const [success, setSuccess] = useState("");
  const [loading, setLoading] = useState(false);

  const updateField = (field, value) => {
    setForm((current) => ({
      ...current,
      [field]: value,
    }));
  };

  const submit = async (event) => {
    event.preventDefault();
    setError("");
    setSuccess("");

    if (!form.email.trim()) {
      const message = "Ingrese su correo.";
      setError(message);
      showAlert(message, "warning");
      return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(form.email.trim())) {
      const message = "Ingrese un correo válido.";
      setError(message);
      showAlert(message, "warning");
      return;
    }

    if (!form.resetToken.trim()) {
      const message = "Ingrese el token de recuperación.";
      setError(message);
      showAlert(message, "warning");
      return;
    }

    if (!form.newPassword.trim()) {
      const message = "Ingrese la nueva contraseña.";
      setError(message);
      showAlert(message, "warning");
      return;
    }

    if (form.newPassword.length < 8 || form.newPassword.length > 10) {
      const message = "La contraseña debe tener entre 8 y 10 caracteres.";
      setError(message);
      showAlert(message, "warning");
      return;
    }

    if (!/[A-Z]/.test(form.newPassword)) {
      const message = "La contraseña debe incluir al menos una mayúscula.";
      setError(message);
      showAlert(message, "warning");
      return;
    }

    if (!/[a-z]/.test(form.newPassword)) {
      const message = "La contraseña debe incluir al menos una minúscula.";
      setError(message);
      showAlert(message, "warning");
      return;
    }

    if (!/[0-9]/.test(form.newPassword)) {
      const message = "La contraseña debe incluir al menos un número.";
      setError(message);
      showAlert(message, "warning");
      return;
    }

    if (!/[^a-zA-Z0-9]/.test(form.newPassword)) {
      const message = "La contraseña debe incluir al menos un símbolo.";
      setError(message);
      showAlert(message, "warning");
      return;
    }

    try {
      setLoading(true);

      await resetPassword({
        email: form.email.trim(),
        resetToken: form.resetToken.trim(),
        newPassword: form.newPassword,
      });

      const message = "Contraseña actualizada correctamente.";
      setSuccess(message);
      showAlert(message, "success");
      setTimeout(() => navigate("/login", { replace: true }), 1200);
    } catch (err) {
      const message = getApiErrorMessage(
        err,
        "No se pudo cambiar la contraseña."
      );
      setError(message);
      showAlert(message, "error");
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout
      eyebrow="Seguridad de la cuenta"
      title="Cambiar contraseña"
      subtitle="Use el token generado y defina una contraseña segura."
    >
        <form onSubmit={submit}>
          <label htmlFor="email">Correo</label>
          <input
            id="email"
            type="email"
            placeholder="admin@salespoint.local"
            value={form.email}
            onChange={(event) => updateField("email", event.target.value)}
          />

          <label htmlFor="resetToken">Token</label>
          <input
            id="resetToken"
            type="text"
            placeholder="Pegue aquí el token"
            value={form.resetToken}
            onChange={(event) => updateField("resetToken", event.target.value)}
          />

          <label htmlFor="newPassword">Nueva contraseña</label>
          <input
            id="newPassword"
            type="password"
            placeholder="Ejemplo: Nueva123!"
            value={form.newPassword}
            onChange={(event) => updateField("newPassword", event.target.value)}
          />

          <div className="password-rules">
            <span>Debe tener 8 a 10 caracteres.</span>
            <span>Debe incluir mayúscula, minúscula, número y símbolo.</span>
            <span>No puede ser igual a la actual ni a una anterior.</span>
          </div>

          {error && <div className="form-alert">{error}</div>}
          {success && <div className="form-success">{success}</div>}

          <button className="login-button" type="submit" disabled={loading}>
            {loading ? "Actualizando..." : "Actualizar contraseña"}
          </button>
        </form>

        <Link to="/forgot-password" className="auth-link">
          Generar otro token
        </Link>

        <Link to="/login" className="auth-link">
          Volver al login
        </Link>
    </AuthLayout>
  );
}
