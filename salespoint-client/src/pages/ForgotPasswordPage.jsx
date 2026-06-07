import { useState } from "react";
import { Link } from "react-router-dom";
import { forgotPassword } from "../api/authApi";

export default function ForgotPasswordPage() {
  const [email, setEmail] = useState("");
  const [resetToken, setResetToken] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const submit = async (event) => {
    event.preventDefault();
    setError("");
    setResetToken("");

    if (!email.trim()) {
      setError("Ingrese su correo.");
      return;
    }

    try {
      setLoading(true);
      const result = await forgotPassword({ email: email.trim() });
      setResetToken(result.resetToken);
    } catch (err) {
      setError(err.response?.data?.message || "No se pudo generar el token.");
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
            <h1>Recuperar contraseña</h1>
            <p>Genera un token temporal de recuperación.</p>
          </div>
        </div>

        <form onSubmit={submit}>
          <label htmlFor="email">Correo registrado</label>
          <input
            id="email"
            type="email"
            placeholder="admin@salespoint.local"
            value={email}
            onChange={(event) => setEmail(event.target.value)}
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
      </section>
    </main>
  );
}