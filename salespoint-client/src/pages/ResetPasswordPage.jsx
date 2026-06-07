import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { resetPassword } from "../api/authApi";

export default function ResetPasswordPage() {
  const navigate = useNavigate();

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
      setError("Ingrese su correo.");
      return;
    }

    if (!form.resetToken.trim()) {
      setError("Ingrese el token de recuperación.");
      return;
    }

    if (!form.newPassword.trim()) {
      setError("Ingrese la nueva contraseña.");
      return;
    }

    try {
      setLoading(true);

      await resetPassword({
        email: form.email.trim(),
        resetToken: form.resetToken.trim(),
        newPassword: form.newPassword,
      });

      setSuccess("Contraseña actualizada correctamente.");
      setTimeout(() => navigate("/login", { replace: true }), 1200);
    } catch (err) {
      setError(err.response?.data?.message || "No se pudo cambiar la contraseña.");
    } finally {
      setLoading(false);
    }
  };

  return (
    <main className="login-page">
      <section className="login-card">
        <div className="login-brand">
          <div className="logo-circle">🛒</div>
          <div>
            <h1>Cambiar contraseña</h1>
            <p>Use el token generado para crear una nueva contraseña.</p>
          </div>
        </div>

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
      </section>
    </main>
  );
}