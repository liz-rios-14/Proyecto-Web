import { useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { login } from "../api/authApi";
import { saveAuthSession } from "../services/authStorage";

export default function LoginPage() {
  const navigate = useNavigate();

  const [form, setForm] = useState({
    userNameOrEmail: "",
    password: "",
  });

  const [error, setError] = useState("");
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

    if (!form.userNameOrEmail.trim()) {
      setError("Ingrese su usuario o correo.");
      return;
    }

    if (!form.password.trim()) {
      setError("Ingrese su contraseña.");
      return;
    }

    try {
      setLoading(true);

      const result = await login({
        userNameOrEmail: form.userNameOrEmail.trim(),
        password: form.password,
      });

      saveAuthSession(result);

      navigate("/", {
        replace: true,
      });
    } catch (err) {
      setError(
        err.response?.data?.message ||
        "No se pudo iniciar sesión."
      );
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
            <h1>SalesPoint</h1>
            <p>Punto de venta web</p>
          </div>
        </div>

        <form onSubmit={submit}>
          <label htmlFor="userNameOrEmail">
            Usuario o correo
          </label>

          <input
            id="userNameOrEmail"
            type="text"
            placeholder="Ingrese su usuario o correo"
            value={form.userNameOrEmail}
            onChange={(event) =>
              updateField(
                "userNameOrEmail",
                event.target.value
              )
            }
          />

          <label htmlFor="password">
            Contraseña
          </label>

          <input
            id="password"
            type="password"
            placeholder="Ingrese su contraseña"
            value={form.password}
            onChange={(event) =>
              updateField("password", event.target.value)
            }
          />

          {error && (
            <div className="form-alert">
              {error}
            </div>
          )}

          <button
            className="login-button"
            type="submit"
            disabled={loading}
          >
            {loading ? "Ingresando..." : "Ingresar"}
          </button>
        </form>

        <div className="login-links">
          <Link to="/forgot-password">
            ¿Olvidaste tu contraseña?
          </Link>
        </div>
      </section>
    </main>
  );
}