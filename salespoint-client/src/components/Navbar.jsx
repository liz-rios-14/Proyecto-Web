import { NavLink } from "react-router-dom";

export default function Navbar() {
  return (
    <nav className="navbar">
      <div className="navbar-brand">Punto de Venta</div>

      <div className="navbar-links">
        <NavLink to="/">Inicio</NavLink>
        <NavLink to="/customers">Gestor Clientes</NavLink>
        <NavLink to="/products">Gestor Productos</NavLink>
        <NavLink to="/sales">Crear Factura</NavLink>
        <NavLink to="/invoices">Historial de Facturas</NavLink>
      </div>
    </nav>
  );
}