import { useEffect, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import { login } from "../api/authApi";
import { getApiErrorMessage } from "../api/apiError";
import { saveAuthSession } from "../services/authStorage";
import { useAppAlert } from "../components/AppAlert";
import AuthLayout from "../components/AuthLayout";
import PasswordInput from "../components/PasswordInput";

export default function LoginPage() {
  const navigate = useNavigate();
  const { showAlert } = useAppAlert();

  const [form, setForm] = useState({
    userNameOrEmail: "",
    password: "",
  });

  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    const parameters = new URLSearchParams(window.location.search);

    if (parameters.get("reason") !== "session-ended") return;

    const message =
      "Su sesión expiró o ya no está autorizada. Ingrese nuevamente.";

    setError(message);
    showAlert(message, "warning");
    window.history.replaceState({}, "", "/login");
  }, []);

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
      const message = "Ingrese su usuario o correo.";
      setError(message);
      showAlert(message, "warning");
      return;
    }

    if (!form.password.trim()) {
      const message = "Ingrese su contraseña.";
      setError(message);
      showAlert(message, "warning");
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
      const message = getApiErrorMessage(
        err,
        "No se pudo iniciar sesión."
      );
      setError(message);
      showAlert(message, "error");
    } finally {
      setLoading(false);
    }
  };

  return (
    <AuthLayout
      eyebrow="Acceso seguro"
      title="Bienvenido de nuevo"
      subtitle="Ingrese sus credenciales para continuar al sistema."
    >
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

          <PasswordInput
            id="password"
            name="password"
            autoComplete="current-password"
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
    </AuthLayout>
  );
}
