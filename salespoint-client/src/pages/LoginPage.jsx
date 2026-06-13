import { useEffect, useRef, useState } from "react";
import { Link, useNavigate } from "react-router-dom";
import {
  getExternalAuthenticationStatus,
  login,
  loginWithGoogle,
} from "../api/authApi";
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
  const [googleLoading, setGoogleLoading] = useState(false);
  const [googleEnabled, setGoogleEnabled] = useState(false);
  const googleButtonRef = useRef(null);

  useEffect(() => {
    const parameters = new URLSearchParams(window.location.search);

    if (parameters.get("reason") !== "session-ended") return;

    const message =
      "Su sesión expiró o ya no está autorizada. Ingrese nuevamente.";

    setError(message);
    showAlert(message, "warning");
    window.history.replaceState({}, "", "/login");
  }, []);

  useEffect(() => {
    let cancelled = false;

    const configureGoogle = async () => {
      try {
        const status = await getExternalAuthenticationStatus();
        if (cancelled || !status.googleEnabled || !status.googleClientId) return;
        setGoogleEnabled(true);

        const renderButton = () => {
          if (cancelled || !window.google || !googleButtonRef.current) return;

          window.google.accounts.id.initialize({
            client_id: status.googleClientId,
            callback: async ({ credential }) => {
              if (!credential) return;
              try {
                setGoogleLoading(true);
                const result = await loginWithGoogle(credential);
                saveAuthSession(result);
                navigate("/", { replace: true });
              } catch (requestError) {
                showAlert(
                  getApiErrorMessage(
                    requestError,
                    "No se pudo iniciar sesión con Google."
                  ),
                  "error"
                );
              } finally {
                setGoogleLoading(false);
              }
            },
          });

          googleButtonRef.current.replaceChildren();
          window.google.accounts.id.renderButton(googleButtonRef.current, {
            theme: "outline",
            size: "large",
            width: googleButtonRef.current.clientWidth || 320,
            text: "signin_with",
            shape: "rectangular",
          });
        };

        if (window.google) {
          renderButton();
          return;
        }

        let script = document.getElementById("google-identity-script");
        if (!script) {
          script = document.createElement("script");
          script.id = "google-identity-script";
          script.src = "https://accounts.google.com/gsi/client";
          script.async = true;
          script.defer = true;
          document.head.appendChild(script);
        }
        script.addEventListener("load", renderButton, { once: true });
      } catch {
        // El acceso tradicional sigue disponible si Google no está configurado.
      }
    };

    configureGoogle();
    return () => {
      cancelled = true;
    };
  }, [navigate, showAlert]);

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

          {googleEnabled && (
            <div className="external-login">
              <span>o ingrese con</span>
              <div
                ref={googleButtonRef}
                className="google-login-button"
                aria-busy={googleLoading}
              />
              {googleLoading && <small>Validando cuenta de Google...</small>}
            </div>
          )}
        </form>

        <div className="login-links">
          <Link to="/forgot-password">
            ¿Olvidaste tu contraseña?
          </Link>
        </div>
    </AuthLayout>
  );
}
