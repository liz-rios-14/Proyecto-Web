import { useState } from "react";
import { NavLink, useLocation } from "react-router-dom";
import KeyboardHelp from "./KeyboardHelp";
import ThemeToggle from "./ThemeToggle";

export default function Sidebar() {
  const location = useLocation();
  const [showShortcuts, setShowShortcuts] = useState(false);

  const shortcutsByPath = {
    "/sales": [
      "Ctrl + C → Cliente",
      "Ctrl + B → Producto",
      "Ctrl + G → Facturar",
      "Ctrl + L → Limpiar",
      "Alt + C → Botón cliente",
      "Alt + B → Botón producto",
      "Alt + A → Agregar",
      "Alt + Q → Cantidad",
      "Alt + G → Facturar",
    ],
    "/customers": [
      "Ctrl + B → Buscar",
      "Ctrl + G → Guardar",
      "Ctrl + L → Limpiar",
      "Alt + B → Botón buscar",
      "Alt + G → Botón guardar",
      "Alt + L → Botón limpiar",
    ],
    "/products": [
      "Ctrl + B → Buscar",
      "Ctrl + G → Guardar",
      "Ctrl + L → Limpiar",
      "Alt + B → Botón buscar",
      "Alt + G → Botón guardar",
      "Alt + L → Botón limpiar",
    ],
    "/invoices": [
      "↑ ↓ → Navegar",
      "Enter → Ver factura",
      "Esc → Cerrar vista",
      "Ctrl + R → Recargar",
      "Alt + V → Ver factura",
    ],
    "/": [
      "Ctrl + 1 → Inicio",
      "Ctrl + 2 → Facturación",
      "Ctrl + 3 → Clientes",
      "Ctrl + 4 → Productos",
      "Ctrl + 5 → Historial",
    ],
  };

  const currentShortcuts =
    shortcutsByPath[location.pathname] || shortcutsByPath["/"];

  return (
    <aside className="sidebar">
      <div>
        <div className="sidebar-logo">
          <div className="logo-circle">🛒</div>

          <div>
            <h2>SalesPoint</h2>
            <p>Sistema POS</p>
          </div>
        </div>

        <nav className="sidebar-nav">
          <NavLink to="/" className="sidebar-link">
            <span>🏠</span>
            <span>Inicio</span>
          </NavLink>

          <NavLink to="/sales" className="sidebar-link">
            <span>🧾</span>
            <span>Facturación</span>
          </NavLink>

          <NavLink to="/customers" className="sidebar-link">
            <span>👥</span>
            <span>Clientes</span>
          </NavLink>

          <NavLink to="/products" className="sidebar-link">
            <span>📦</span>
            <span>Productos</span>
          </NavLink>

          <NavLink to="/invoices" className="sidebar-link">
            <span>📜</span>
            <span>Historial</span>
          </NavLink>
        </nav>

        <div className="shortcut-help-area">
          <button
            className="shortcut-toggle"
            onClick={() => setShowShortcuts(!showShortcuts)}
            title="Mostrar atajos de teclado"
          >
            ?
          </button>

          {showShortcuts && (
            <div className="shortcut-popover">
              <KeyboardHelp title="Atajos" shortcuts={currentShortcuts} />
            </div>
          )}
        </div>
      </div>

      <div className="sidebar-bottom">
        <ThemeToggle />
      </div>
    </aside>
  );
}