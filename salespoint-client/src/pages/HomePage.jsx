import { Link, useLocation } from "react-router-dom";
import { useEffect } from "react";
import {
  AlertTriangle,
  BarChart3,
  BadgeCheck,
  Boxes,
  ClipboardList,
  Cloud,
  FileClock,
  FileSpreadsheet,
  KeyRound,
  LogIn,
  ReceiptText,
  RefreshCw,
  ShieldCheck,
  ShoppingCart,
  UserCog,
  Users,
} from "lucide-react";
import Layout from "../components/Layout";
import { getAuthRole, getAuthUser } from "../services/authStorage";
import { getRoleLabel } from "../services/roleLabels";
import { useAppAlert } from "../components/AppAlert";

export default function HomePage() {
  const location = useLocation();
  const { showAlert } = useAppAlert();
  const user = getAuthUser();
  const isAdministrator = getAuthRole() === "ADMINISTRATOR";
  const roleLabel = getRoleLabel(user?.roleName);

  useEffect(() => {
    const permissionMessage = location.state?.permissionMessage;

    if (!permissionMessage) return;

    showAlert(permissionMessage, "warning");
    window.history.replaceState({}, "", "/");
    // showAlert is provided by the global alert context.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [location.state]);

  const cards = isAdministrator
    ? [
        {
          to: "/customers",
          title: "Clientes",
          description: "Registra, consulta y administra la información de clientes.",
          action: "Gestionar clientes",
          icon: Users,
        },
        {
          to: "/products",
          title: "Productos",
          description: "Controla catálogo, precios, disponibilidad y existencias.",
          action: "Gestionar productos",
          icon: Boxes,
        },
        {
          to: "/invoices",
          title: "Facturas",
          description: "Consulta ventas y reconstruye documentos históricos.",
          action: "Ver historial",
          icon: FileClock,
        },
        {
          to: "/users",
          title: "Usuarios",
          description: "Administra accesos, estados y desbloqueo de cuentas.",
          action: "Administrar usuarios",
          icon: UserCog,
        },
        {
          to: "/roles",
          title: "Roles",
          description: "Consulta y mantiene los roles disponibles del sistema.",
          action: "Administrar roles",
          icon: ShieldCheck,
        },
        {
          to: "/error-logs",
          title: "Registro de errores",
          description: "Revisa incidencias técnicas para seguimiento y soporte.",
          action: "Consultar errores",
          icon: AlertTriangle,
        },
        {
          to: "/reports",
          title: "Reportes avanzados",
          description: "Analiza ventas, vendedores, productos, clientes y stock.",
          action: "Abrir reportes",
          icon: BarChart3,
        },
        {
          to: "/audit-logs",
          title: "Auditoría general",
          description: "Consulta accesos y operaciones relevantes realizadas por los usuarios.",
          action: "Revisar auditoría",
          icon: ClipboardList,
        },
      ]
    : [
        {
          to: "/sales",
          title: "Nueva venta",
          description: "Selecciona cliente y productos para generar una factura.",
          action: "Comenzar venta",
          icon: ReceiptText,
          featured: true,
        },
        {
          to: "/customers",
          title: "Clientes",
          description: "Consulta clientes o registra uno nuevo antes de realizar la venta.",
          action: "Consultar clientes",
          icon: Users,
        },
        {
          to: "/invoices",
          title: "Mis facturas",
          description: "Consulta y reconstruye únicamente las ventas realizadas por ti.",
          action: "Ver mis facturas",
          icon: FileClock,
        },
        {
          to: "/reports",
          title: "Mis reportes",
          description: "Consulta y exporta únicamente los resultados de tus propias ventas.",
          action: "Ver mis resultados",
          icon: BarChart3,
        },
      ];

  const bonusFeatures = [
    {
      title: "Renovación de sesión",
      description: "JWT con refresh token, rotación y cierre seguro de sesión.",
      icon: RefreshCw,
      access: "Ambos roles",
    },
    {
      title: "Recuperación de contraseña",
      description: "Token temporal, expiración y bloqueo de contraseñas anteriores.",
      icon: KeyRound,
      access: "Acceso público seguro",
    },
    {
      title: "Autenticación externa",
      description: "Inicio de sesión con Google y validación oficial del token en la API.",
      icon: LogIn,
      access: "Cuentas previamente registradas",
    },
    {
      title: "Despliegue completo",
      description: "Frontend, API y SQL Server desplegables juntos con Docker Compose.",
      icon: Cloud,
      access: "Entorno local o servidor cloud",
    },
    {
      title: "Exportación avanzada",
      description: "Facturas en PDF y reportes descargables en Excel.",
      icon: FileSpreadsheet,
      access: isAdministrator ? "Reporte global" : "Solo ventas propias",
    },
    ...(isAdministrator
      ? [
          {
            title: "Auditoría y soporte",
            description: "Trazabilidad de acciones y registro técnico de errores.",
            icon: ClipboardList,
            access: "Solo administrador",
          },
        ]
      : []),
  ];

  return (
    <Layout>
      <section className="dashboard-shell">
        <section className="dashboard-hero">
          <div className="dashboard-hero-content">
            <span className="dashboard-badge">
              Panel de {roleLabel.toLowerCase()}
            </span>

            <h1>
              Bienvenido, <span>{String(user?.userName || "USUARIO").toUpperCase()}</span>
            </h1>

            <p>
              {isAdministrator
                ? "Supervisa la operación, administra la información principal y controla la seguridad desde un solo lugar."
                : "Gestiona tus ventas de forma rápida, consulta productos disponibles y revisa tus facturas guardadas."}
            </p>

            <div className="dashboard-actions">
              <Link
                to={isAdministrator ? "/products" : "/sales"}
                className="dashboard-primary-button"
              >
                {isAdministrator ? <Boxes size={18} /> : <ShoppingCart size={18} />}
                {isAdministrator ? "Administrar productos" : "Ir al punto de venta"}
              </Link>

              <Link
                to={isAdministrator ? "/error-logs" : "/invoices"}
                className="dashboard-secondary-button"
              >
                {isAdministrator ? "Revisar soporte" : "Consultar mis facturas"}
              </Link>
            </div>
          </div>

          <div className="dashboard-session-card">
            <div className="dashboard-session-icon">
              <BadgeCheck size={28} />
            </div>
            <span>Sesión protegida</span>
            <strong>{roleLabel}</strong>
            <p>{user?.email || "Correo no registrado"}</p>
            <div className="dashboard-online">
              <i />
              Acceso activo
            </div>
          </div>
        </section>

        <div className="dashboard-section-title">
          <div>
            <span className="page-eyebrow">Accesos disponibles</span>
            <h2>¿Qué deseas hacer?</h2>
          </div>
          <p>Las opciones se muestran según los permisos de tu cuenta.</p>
        </div>

        <section className="dashboard-grid">
          {cards.map((card) => {
            const Icon = card.icon;

            return (
              <Link
                key={card.to}
                to={card.to}
                className={`dashboard-card ${
                  card.featured ? "dashboard-card-main" : ""
                }`}
              >
                <span className="dashboard-card-icon">
                  <Icon size={25} />
                </span>
                <h2>{card.title}</h2>
                <p>{card.description}</p>
                <strong>{card.action} →</strong>
              </Link>
            );
          })}
        </section>

        <section className="dashboard-summary">
          <div>
            <span className="dashboard-badge light">Operación segura</span>
            <h2>Información protegida y trazable</h2>
            <p>
              Ventas históricas, control de inventario, permisos por rol,
              mensajes claros y registro técnico de errores integrados con
              SQL Server.
            </p>
          </div>

          <div className="dashboard-stack">
            <span>{isAdministrator ? "Administración" : "Facturación"}</span>
            <span>Inventario</span>
            <span>Auditoría</span>
            <span>Seguridad</span>
            <span>Soporte</span>
          </div>
        </section>

        <section className="bonus-showcase">
          <div className="dashboard-section-title">
            <div>
              <span className="page-eyebrow">Funcionalidades bonus</span>
              <h2>Adicionales activos</h2>
            </div>
            <p>
              Integrados con la API y protegidos según el rol autenticado.
            </p>
          </div>

          <div className="bonus-feature-grid">
            {bonusFeatures.map((feature) => {
              const FeatureIcon = feature.icon;

              return (
                <article className="bonus-feature-card" key={feature.title}>
                  <span className="bonus-feature-icon">
                    <FeatureIcon size={22} />
                  </span>
                  <div>
                    <span className="bonus-status">Implementado</span>
                    <h3>{feature.title}</h3>
                    <p>{feature.description}</p>
                    <strong>{feature.access}</strong>
                  </div>
                </article>
              );
            })}
          </div>
          <Link to="/additional-features" className="dashboard-primary-button bonus-proof-link">
            Comprobar todos los adicionales
          </Link>
        </section>
      </section>
    </Layout>
  );
}
