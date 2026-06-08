import {
  BadgeCheck,
  BarChart3,
  Boxes,
  ShieldCheck,
  ShoppingCart,
} from "lucide-react";
import ThemeToggle from "./ThemeToggle";

export default function AuthLayout({
  eyebrow,
  title,
  subtitle,
  children,
}) {
  return (
    <main className="auth-page">
      <div className="auth-theme-toggle">
        <ThemeToggle />
      </div>

      <section className="auth-shell">
        <aside className="auth-showcase" aria-label="Presentación de SalesPoint">
          <div className="auth-showcase-content">
            <div className="auth-showcase-brand">
              <span className="auth-logo">
                <ShoppingCart size={28} />
              </span>
              <div>
                <strong>SalesPoint</strong>
                <span>Sistema de facturación</span>
              </div>
            </div>

            <span className="auth-showcase-badge">
              <BadgeCheck size={16} />
              Gestión segura y trazable
            </span>

            <h2>Controla ventas, inventario y auditoría desde un solo lugar.</h2>
            <p>
              Una experiencia clara para administrar el negocio y atender
              ventas con rapidez, seguridad y datos históricos confiables.
            </p>

            <div className="auth-feature-grid">
              <div>
                <ShoppingCart size={20} />
                <span>Facturación ágil</span>
              </div>
              <div>
                <Boxes size={20} />
                <span>Stock controlado</span>
              </div>
              <div>
                <BarChart3 size={20} />
                <span>Información clara</span>
              </div>
              <div>
                <ShieldCheck size={20} />
                <span>Acceso por roles</span>
              </div>
            </div>
          </div>
        </aside>

        <section className="auth-panel">
          <div className="login-card">
            <div className="login-brand">
              <span className="logo-circle" aria-hidden="true">
                <ShoppingCart size={23} />
              </span>

              <div>
                <strong>SalesPoint</strong>
                <span>Punto de venta web</span>
              </div>
            </div>

            <header className="auth-heading">
              <span className="auth-eyebrow">{eyebrow}</span>
              <h1>{title}</h1>
              <p>{subtitle}</p>
            </header>

            {children}
          </div>
        </section>
      </section>
    </main>
  );
}
