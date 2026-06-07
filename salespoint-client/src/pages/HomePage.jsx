import { Link } from "react-router-dom";
import Layout from "../components/Layout";
import { getAuthUser } from "../services/authStorage";

export default function HomePage() {
  const user = getAuthUser();

  return (
    <Layout>
      <section className="dashboard-shell">
        <section className="dashboard-hero">
          <div className="dashboard-hero-content">
            <span className="dashboard-badge">Sistema POS Web</span>

            <h1>Panel principal de SalesPoint</h1>

            <p>
              Administra ventas, clientes, productos e historial desde una interfaz
              centralizada, conectada a la API REST y protegida con JWT.
            </p>

            <div className="dashboard-actions">
              <Link to="/sales" className="dashboard-primary-button">
                Crear venta
              </Link>

              <Link to="/products" className="dashboard-secondary-button">
                Ver productos
              </Link>
            </div>
          </div>

          <div className="dashboard-session-card">
            <div className="dashboard-session-icon">👤</div>
            <span>Sesión activa</span>
            <strong>{user?.userName || "Usuario"}</strong>
            <p>{user?.roleName || "Sin rol asignado"}</p>
          </div>
        </section>

        <section className="dashboard-grid">
          <Link to="/sales" className="dashboard-card dashboard-card-main">
            <span className="dashboard-card-icon">🧾</span>
            <h2>Facturación</h2>
            <p>Crea ventas, calcula subtotal, IVA y total.</p>
            <strong>Ir a caja →</strong>
          </Link>

          <Link to="/customers" className="dashboard-card">
            <span className="dashboard-card-icon">👥</span>
            <h2>Clientes</h2>
            <p>Registra y consulta clientes para la venta.</p>
            <strong>Gestionar →</strong>
          </Link>

          <Link to="/products" className="dashboard-card">
            <span className="dashboard-card-icon">📦</span>
            <h2>Productos</h2>
            <p>Controla precios, stock y productos activos.</p>
            <strong>Administrar →</strong>
          </Link>

          <Link to="/invoices" className="dashboard-card">
            <span className="dashboard-card-icon">📜</span>
            <h2>Historial</h2>
            <p>Reconstruye facturas y revisa ventas guardadas.</p>
            <strong>Consultar →</strong>
          </Link>
        </section>

        <section className="dashboard-summary">
          <div>
            <span className="dashboard-badge light">Arquitectura integrada</span>
            <h2>Proyecto listo para defensa</h2>
            <p>
              Integración de Domain, Application, Infrastructure, API REST,
              Oracle, React, JWT, roles, facturación e historial de ventas.
            </p>
          </div>

          <div className="dashboard-stack">
            <span>Clean Architecture</span>
            <span>SOLID</span>
            <span>Oracle</span>
            <span>React</span>
            <span>JWT</span>
            <span>REST API</span>
          </div>
        </section>
      </section>
    </Layout>
  );
}