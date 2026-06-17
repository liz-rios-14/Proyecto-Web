import { useState } from "react";
import { Link } from "react-router-dom";
import { forgotPassword } from "../api/authApi";
import { getApiErrorMessage } from "../api/apiError";
import { useAppAlert } from "../components/AppAlert";
import AuthLayout from "../components/AuthLayout";
import { sanitizeEmail } from "../utils/inputSanitizers";

export default function ForgotPasswordPage() {
  const { showAlert } = useAppAlert();
  const [email, setEmail] = useState("");
  const [resetToken, setResetToken] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const submit = async (event) => {
    event.preventDefault();
    setError("");
    setResetToken("");

    if (!email.trim()) {
      const message = "Ingrese su correo.";
      setError(message);
      showAlert(message, "warning");
      return;
    }

    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email.trim())) {
      const message = "Ingrese un correo válido.";
      setError(message);
      showAlert(message, "warning");
      return;
    }

    try {
      setLoading(true);
      const result = await forgotPassword({ email: email.trim() });
      setResetToken(result.resetToken || "");
      showAlert("Se generó una solicitud de recuperación.", "success");
    } catch (err) {
      const message = getApiErrorMessage(
        err,
        "No se pudo generar el token."
      );
      setError(message);
      showAlert(message, "error");
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout
      eyebrow="Recuperación de acceso"
      title="Recuperar contraseña"
      subtitle="Genere un token temporal usando el correo registrado."
    >
        <form onSubmit={submit}>
          <label htmlFor="email">Correo registrado</label>
          <input
            id="email"
            type="email"
            placeholder="admin@salespoint.local"
            value={email}
            onKeyDown={(event) => {
              if (event.key === " ") event.preventDefault();
            }}
            onChange={(event) => setEmail(sanitizeEmail(event.target.value))}
          />

          {error && <div className="form-alert">{error}</div>}

          <button className="login-button" type="submit" disabled={loading}>
            {loading ? "Generando..." : "Generar token"}
          </button>
        </form>

        {resetToken && (
          <div className="login-demo">
            <strong>Token generado</strong>
            <span className="token-text">{resetToken}</span>
            <p>Copie este token y úselo en la pantalla de cambio de contraseña.</p>
            <Link to="/reset-password" className="auth-link">
              Ir a cambiar contraseña
            </Link>
          </div>
        )}

        <Link to="/login" className="auth-link">
          Volver al login
        </Link>
    </AuthLayout>
  );
}
