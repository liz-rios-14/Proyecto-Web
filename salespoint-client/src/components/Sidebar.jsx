import { useState } from "react";
import { NavLink, useLocation, useNavigate } from "react-router-dom";
import {
  Boxes,
  BarChart3,
  ClipboardList,
  CircleHelp,
  FileClock,
  Home,
  ReceiptText,
  Sparkles,
  ShieldCheck,
  ShoppingCart,
  TriangleAlert,
  UserCog,
  Users,
} from "lucide-react";
import KeyboardHelp from "./KeyboardHelp";
import ThemeToggle from "./ThemeToggle";
import {
  clearAuthSession,
  getAuthRole,
  getAuthUser,
  getRefreshToken,
} from "../services/authStorage";
import { logout as revokeSession } from "../api/authApi";
import { useAppAlert } from "./AppAlert";
import { getRoleLabel } from "../services/roleLabels";
import { useSafeNavigation } from "./UnsavedChangesContext";

export default function Sidebar() {
  const location = useLocation();
  const navigate = useNavigate();
  const { showAlert, showConfirm } = useAppAlert();
  const safeNavigation = useSafeNavigation();
  const [showShortcuts, setShowShortcuts] = useState(false);
  const [closingSession, setClosingSession] = useState(false);
  const authUser = getAuthUser();
  const roleName = getAuthRole();
  const isAdministrator = roleName === "ADMINISTRATOR";
  const isSeller = roleName === "SELLER";
  const roleLabel = getRoleLabel(authUser?.roleName);
  const draftKey = `salespoint-sale-draft-${authUser?.userId || authUser?.userName || "user"}`;

  const logout = async () => {
    if (closingSession) return;

    const hasDraft = Boolean(localStorage.getItem(draftKey));
    const confirmed = await showConfirm(
      safeNavigation?.isDirty
        ? "Existen cambios sin guardar. Si cierra sesión se perderán. ¿Desea continuar?"
        : hasDraft
        ? "Existe una venta en progreso. Si cierra sesión se guardará como borrador."
        : "¿Seguro que desea cerrar la sesión actual?",
      {
        title: "Cerrar sesión",
        confirmText: "Sí, cerrar sesión",
      }
    );

    if (!confirmed) return;

    try {
      setClosingSession(true);
      const refreshToken = getRefreshToken();

      if (refreshToken) {
        try {
          await revokeSession(refreshToken);
        } catch {
          // El cierre local continúa aunque la API no esté disponible.
        }
      }

      clearAuthSession();
      navigate("/login", { replace: true });
      showAlert("Sesión cerrada correctamente.", "success");
    } finally {
      setClosingSession(false);
    }
  };

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
    "/": isAdministrator
      ? [
          "Ctrl + 1 → Inicio",
          "Ctrl + 3 → Clientes",
          "Ctrl + 4 → Productos",
          "Ctrl + 5 → Historial",
        ]
      : [
          "Ctrl + 1 → Inicio",
          "Ctrl + 2 → Facturación",
          "Ctrl + 3 → Clientes",
          "Ctrl + 5 → Historial",
        ],
  };

  const currentShortcuts =
    shortcutsByPath[location.pathname] || shortcutsByPath["/"];

  const handleNavigation = async (event, path) => {
    if (!safeNavigation?.isDirty) return;
    event.preventDefault();
    await safeNavigation.requestNavigation(path);
  };

  return (
    <aside className="sidebar">
      <div>
        <div className="sidebar-logo">
          <div className="logo-circle">
            <ShoppingCart size={23} />
          </div>

          <div>
            <h2>SalesPoint</h2>
            <p>Sistema POS</p>
          </div>
        </div>

        <nav className="sidebar-nav">
          <NavLink to="/" className="sidebar-link" onClick={(event) => handleNavigation(event, "/")}>
            <Home size={19} />
            <span>Inicio</span>
          </NavLink>

          {isSeller && (
            <NavLink to="/sales" className="sidebar-link" onClick={(event) => handleNavigation(event, "/sales")}>
              <ReceiptText size={19} />
              <span>Facturación</span>
            </NavLink>
          )}

          <NavLink to="/customers" className="sidebar-link" onClick={(event) => handleNavigation(event, "/customers")}>
            <Users size={19} />
            <span>Clientes</span>
          </NavLink>

          {isAdministrator && (
            <>
              <NavLink to="/products" className="sidebar-link" onClick={(event) => handleNavigation(event, "/products")}>
                <Boxes size={19} />
                <span>Productos</span>
              </NavLink>
            </>
          )}

          <NavLink to="/invoices" className="sidebar-link" onClick={(event) => handleNavigation(event, "/invoices")}>
            <FileClock size={19} />
            <span>Historial</span>
          </NavLink>

          <NavLink to="/reports" className="sidebar-link" onClick={(event) => handleNavigation(event, "/reports")}>
            <BarChart3 size={19} />
            <span>Reportes</span>
          </NavLink>

          <NavLink to="/additional-features" className="sidebar-link" onClick={(event) => handleNavigation(event, "/additional-features")}>
            <Sparkles size={19} />
            <span>Adicionales</span>
          </NavLink>

          {isAdministrator && (
            <>
              <NavLink to="/users" className="sidebar-link" onClick={(event) => handleNavigation(event, "/users")}>
                <UserCog size={19} />
                <span>Usuarios</span>
              </NavLink>

              <NavLink to="/roles" className="sidebar-link" onClick={(event) => handleNavigation(event, "/roles")}>
                <ShieldCheck size={19} />
                <span>Roles</span>
              </NavLink>

              <NavLink to="/error-logs" className="sidebar-link" onClick={(event) => handleNavigation(event, "/error-logs")}>
                <TriangleAlert size={19} />
                <span>Registro de errores</span>
              </NavLink>

              <NavLink to="/audit-logs" className="sidebar-link" onClick={(event) => handleNavigation(event, "/audit-logs")}>
                <ClipboardList size={19} />
                <span>Auditoría</span>
              </NavLink>
            </>
          )}
        </nav>

        <div className="shortcut-help-area">
          <button
            type="button"
            className="shortcut-toggle"
            onClick={() => setShowShortcuts(!showShortcuts)}
            title="Mostrar atajos de teclado"
            aria-expanded={showShortcuts}
            aria-label={showShortcuts ? "Ocultar atajos" : "Mostrar atajos"}
          >
            <CircleHelp size={21} />
          </button>

          {showShortcuts && (
            <div className="shortcut-popover">
              <KeyboardHelp title="Atajos" shortcuts={currentShortcuts} />
            </div>
          )}
        </div>
      </div>

      <div className="sidebar-bottom">
        <div className="sidebar-user">
          <strong>{authUser?.userName || "Usuario"}</strong>
          <span>{roleLabel}</span>
        </div>

        <ThemeToggle />

        <button
          type="button"
          className="logout-button"
          disabled={closingSession}
          onClick={logout}
        >
          {closingSession ? "Cerrando..." : "Cerrar sesión"}
        </button>
      </div>
    </aside>
  );
}
