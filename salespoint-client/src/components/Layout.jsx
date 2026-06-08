import { useEffect } from "react";
import {
  Boxes,
  FileClock,
  ReceiptText,
  ShieldCheck,
  TriangleAlert,
  UserCog,
  Users,
} from "lucide-react";
import { useLocation, useNavigate } from "react-router-dom";
import Sidebar from "./Sidebar";

const pageMetadata = {
  "/sales": {
    eyebrow: "Punto de venta",
    title: "Crear factura",
    description: "Seleccione un cliente, agregue productos y confirme la venta.",
    icon: ReceiptText,
  },
  "/customers": {
    eyebrow: "Gestión comercial",
    title: "Clientes",
    description: "Registre, consulte y mantenga actualizada la información de sus clientes.",
    icon: Users,
  },
  "/products": {
    eyebrow: "Catálogo e inventario",
    title: "Productos",
    description: "Administre precios, existencias y disponibilidad del catálogo.",
    icon: Boxes,
  },
  "/invoices": {
    eyebrow: "Trazabilidad y auditoría",
    title: "Historial de facturas",
    description: "Consulte documentos emitidos y reconstruya sus datos históricos.",
    icon: FileClock,
  },
  "/users": {
    eyebrow: "Seguridad del sistema",
    title: "Usuarios",
    description: "Gestione cuentas, estados, roles y acceso al sistema.",
    icon: UserCog,
  },
  "/roles": {
    eyebrow: "Control de acceso",
    title: "Roles",
    description: "Configure los perfiles que determinan las funciones disponibles.",
    icon: ShieldCheck,
  },
  "/error-logs": {
    eyebrow: "Soporte técnico",
    title: "Registro de errores",
    description: "Revise incidencias técnicas y detalles útiles para diagnóstico.",
    icon: TriangleAlert,
  },
};

export default function Layout({ children }) {
  const navigate = useNavigate();
  const location = useLocation();
  const metadata = pageMetadata[location.pathname];
  const PageIcon = metadata?.icon;

  useEffect(() => {
    const handleGlobalShortcuts = (event) => {
      if (!event.ctrlKey) return;

      switch (event.key) {
        case "1":
          event.preventDefault();
          navigate("/");
          break;

        case "2":
          event.preventDefault();
          navigate("/sales");
          break;

        case "3":
          event.preventDefault();
          navigate("/customers");
          break;

        case "4":
          event.preventDefault();
          navigate("/products");
          break;

        case "5":
          event.preventDefault();
          navigate("/invoices");
          break;

        default:
          break;
      }
    };

    window.addEventListener("keydown", handleGlobalShortcuts);

    return () => {
      window.removeEventListener("keydown", handleGlobalShortcuts);
    };
  }, [navigate]);

  return (
    <div className="app-layout">
      <Sidebar />

      <main className="main-content">
        <div className="container">
          {metadata && (
            <header className="module-heading">
              <div className="module-heading-copy">
                <span className="module-eyebrow">{metadata.eyebrow}</span>
                <h1>{metadata.title}</h1>
                <p>{metadata.description}</p>
              </div>

              <span className="module-heading-icon" aria-hidden="true">
                <PageIcon size={30} />
              </span>
            </header>
          )}

          <div className={metadata ? "module-content" : ""}>{children}</div>
        </div>
      </main>
    </div>
  );
}
