import { useEffect, useState } from "react";
import { Link } from "react-router-dom";
import {
  BarChart3,
  CheckCircle2,
  ClipboardList,
  Cloud,
  FileSpreadsheet,
  KeyRound,
  LoaderCircle,
  LogIn,
  RefreshCw,
  TestTube2,
  XCircle,
} from "lucide-react";
import Layout from "../components/Layout";
import { api } from "../api/apiClient";
import { getApiErrorMessage } from "../api/apiError";
import { useAppAlert } from "../components/AppAlert";
import { getAuthRole } from "../services/authStorage";

const features = [
  {
    title: "Refresh tokens",
    description: "Rotación, expiración y revocación segura de sesiones.",
    icon: RefreshCw,
    link: "/",
    action: "Ver sesión activa",
  },
  {
    title: "Recuperación de contraseña",
    description: "Token temporal y bloqueo de contraseñas anteriores.",
    icon: KeyRound,
    link: "/forgot-password",
    action: "Abrir recuperación",
  },
  {
    title: "Autenticación externa",
    description: "Inicio con Google validado oficialmente por la API.",
    icon: LogIn,
    statusKey: "googleConfigured",
  },
  {
    title: "Auditoría general",
    description: "Registro de accesos y operaciones relevantes.",
    icon: ClipboardList,
    adminOnly: true,
    link: "/audit-logs",
    action: "Abrir auditoría",
  },
  {
    title: "Reportes avanzados",
    description: "Ventas, vendedores, productos, clientes y stock.",
    icon: BarChart3,
    link: "/reports",
    action: "Abrir reportes",
  },
  {
    title: "Exportación PDF y Excel",
    description: "Facturas PDF y libros Excel reales con varias hojas.",
    icon: FileSpreadsheet,
    link: "/reports",
    action: "Probar exportación",
  },
  {
    title: "Pruebas automatizadas",
    description: "15 pruebas para reglas críticas y autenticación externa.",
    icon: TestTube2,
  },
  {
    title: "Despliegue completo",
    description: "Frontend, API y SQL Server integrados con Docker Compose.",
    icon: Cloud,
    statusKey: "runningInContainer",
  },
];

function StatusBadge({ active, pendingText = "Requiere configuración" }) {
  return active ? (
    <span className="additional-status success">
      <CheckCircle2 size={16} />
      Activo
    </span>
  ) : (
    <span className="additional-status pending">
      <XCircle size={16} />
      {pendingText}
    </span>
  );
}

export default function AdditionalFeaturesPage() {
  const { showAlert } = useAppAlert();
  const isAdministrator = getAuthRole() === "ADMINISTRATOR";
  const [status, setStatus] = useState(null);
  const [loading, setLoading] = useState(true);

  const checkStatus = async () => {
    try {
      setLoading(true);
      const response = await api.get("/system-status");
      setStatus(response.data);
    } catch (error) {
      setStatus(null);
      showAlert(
        getApiErrorMessage(error, "No se pudo comprobar el estado del sistema."),
        "error"
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    checkStatus();
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  const visibleFeatures = features.filter(
    (feature) => !feature.adminOnly || isAdministrator
  );

  return (
    <Layout>
      <section className="page-heading additional-heading">
        <div>
          <span className="page-eyebrow">Demostración técnica</span>
          <h1>Funcionalidades adicionales</h1>
          <p>
            Comprueba desde el frontend cada bonus implementado y su conexión
            real con la API.
          </p>
        </div>

        <button type="button" onClick={checkStatus} disabled={loading}>
          {loading ? <LoaderCircle className="spin" size={18} /> : <RefreshCw size={18} />}
          {loading ? "Comprobando..." : "Comprobar ahora"}
        </button>
      </section>

      <section className="additional-live-status">
        <article>
          <span>Frontend</span>
          <StatusBadge active />
        </article>
        <article>
          <span>API REST</span>
          <StatusBadge active={Boolean(status?.apiConnected)} />
        </article>
        <article>
          <span>SQL Server</span>
          <StatusBadge active={Boolean(status?.databaseConnected)} />
        </article>
        <article>
          <span>Entorno</span>
          <strong>{status?.deploymentMode || "Sin respuesta"}</strong>
        </article>
      </section>

      <section className="additional-grid">
        {visibleFeatures.map((feature) => {
          const Icon = feature.icon;
          const hasDynamicStatus = Boolean(feature.statusKey);
          const dynamicStatus = Boolean(status?.[feature.statusKey]);

          return (
            <article className="additional-card" key={feature.title}>
              <span className="additional-card-icon">
                <Icon size={24} />
              </span>
              <div>
                {hasDynamicStatus ? (
                  <StatusBadge
                    active={dynamicStatus}
                    pendingText={
                      feature.statusKey === "googleConfigured"
                        ? "Falta Client ID"
                        : "Ejecutando fuera de Docker"
                    }
                  />
                ) : (
                  <StatusBadge active />
                )}
                <h2>{feature.title}</h2>
                <p>{feature.description}</p>
                {feature.link && (
                  <Link to={feature.link}>{feature.action} →</Link>
                )}
              </div>
            </article>
          );
        })}
      </section>

      <section className="card additional-proof">
        <div>
          <span className="page-eyebrow">Prueba de despliegue</span>
          <h2>
            {status?.runningInContainer
              ? "La aplicación está ejecutándose dentro de Docker"
              : "La aplicación actual se está ejecutando en modo local"}
          </h2>
          <p>
            Al abrir <strong>http://localhost:8080</strong>, esta sección debe
            mostrar Docker Compose activo, junto con API y SQL Server conectados.
          </p>
        </div>
        <dl>
          <div><dt>Ambiente API</dt><dd>{status?.environment || "-"}</dd></div>
          <div><dt>Modo</dt><dd>{status?.deploymentMode || "-"}</dd></div>
          <div>
            <dt>Última comprobación</dt>
            <dd>
              {status?.checkedAt
                ? new Date(status.checkedAt).toLocaleString("es-EC")
                : "-"}
            </dd>
          </div>
        </dl>
      </section>

      <section className="card additional-proof">
        <div>
          <span className="page-eyebrow">Prueba de Google</span>
          <h2>
            {status?.googleConfigured
              ? "Google está configurado y visible en el Login"
              : "Configure GOOGLE_CLIENT_ID para activar el botón"}
          </h2>
          <p>
            El acceso externo solo acepta correos ya registrados y activos en
            SalesPoint. La API verifica firma, audiencia y correo confirmado.
          </p>
          {!status?.googleConfigured && (
            <ol className="additional-steps">
              <li>Cree un cliente OAuth web en Google Cloud Console.</li>
              <li>Autorice el origen <code>http://localhost:8080</code>.</li>
              <li>Pegue el Client ID en <code>docker/.env</code>.</li>
              <li>Reconstruya los contenedores y vuelva a comprobar.</li>
            </ol>
          )}
        </div>
      </section>
    </Layout>
  );
}
