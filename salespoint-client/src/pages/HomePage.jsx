import { Link } from "react-router-dom";
import {
  AlertTriangle,
  BadgeCheck,
  Boxes,
  FileClock,
  ReceiptText,
  ShieldCheck,
  ShoppingCart,
  UserCog,
  Users,
} from "lucide-react";
import Layout from "../components/Layout";
import { getAuthUser } from "../services/authStorage";
import { getRoleLabel } from "../services/roleLabels";

export default function HomePage() {
  const user = getAuthUser();
  const isAdministrator =
    user?.roleName?.toUpperCase() === "ADMINISTRATOR";
  const roleLabel = getRoleLabel(user?.roleName);

  const cards = isAdministrator
    ? [
        {
          to: "/sales",
          title: "Punto de venta",
          description: "Genera facturas con control de stock y totales automáticos.",
          action: "Abrir caja",
          icon: ShoppingCart,
          featured: true,
        },
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
          to: "/invoices",
          title: "Mis facturas",
          description: "Consulta y reconstruye únicamente las ventas realizadas por ti.",
          action: "Ver mis facturas",
          icon: FileClock,
        },
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
              Bienvenido, <span>{user?.userName || "usuario"}</span>
            </h1>

            <p>
              {isAdministrator
                ? "Supervisa la operación, administra la información principal y controla la seguridad desde un solo lugar."
                : "Gestiona tus ventas de forma rápida, consulta productos disponibles y revisa tus facturas guardadas."}
            </p>

            <div className="dashboard-actions">
              <Link to="/sales" className="dashboard-primary-button">
                <ShoppingCart size={18} />
                Ir al punto de venta
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
            <span>Facturación</span>
            <span>Inventario</span>
            <span>Auditoría</span>
            <span>Seguridad</span>
            <span>Soporte</span>
          </div>
        </section>
      </section>
    </Layout>
  );
}
