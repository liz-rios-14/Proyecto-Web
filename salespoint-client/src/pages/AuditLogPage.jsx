import { useEffect, useState } from "react";
import Layout from "../components/Layout";
import DataTable from "../components/DataTable";
import Pagination from "../components/Pagination";
import { api } from "../api/apiClient";
import { getApiErrorMessage } from "../api/apiError";
import { useAppAlert } from "../components/AppAlert";

const columns = [
  { key: "formattedDate", label: "Fecha" },
  { key: "userName", label: "Usuario" },
  { key: "actionLabel", label: "Acción" },
  { key: "entityLabel", label: "Entidad" },
  { key: "request", label: "Solicitud" },
];

const actionOptions = [
  ["", "Todas las acciones"],
  ["LOGIN_EXITOSO", "Inicio de sesión exitoso"],
  ["LOGIN_FALLIDO", "Inicio de sesión fallido"],
  ["LOGOUT", "Cierre de sesión"],
  ["CREAR_CLIENTE", "Crear cliente"],
  ["ACTUALIZAR_CLIENTE", "Actualizar cliente"],
  ["ACTIVAR_CLIENTE", "Activar cliente"],
  ["DESACTIVAR_CLIENTE", "Desactivar cliente"],
  ["CREAR_PRODUCTO", "Crear producto"],
  ["ACTUALIZAR_PRODUCTO", "Actualizar producto"],
  ["ACTIVAR_PRODUCTO", "Activar producto"],
  ["DESACTIVAR_PRODUCTO", "Desactivar producto"],
  ["CREAR_FACTURA", "Crear factura"],
  ["RECONSTRUIR_FACTURA", "Reconstruir factura"],
  ["CREAR_USUARIO", "Crear usuario"],
  ["ACTUALIZAR_USUARIO", "Actualizar usuario"],
  ["DESACTIVAR_USUARIO", "Desactivar usuario"],
  ["DESBLOQUEAR_USUARIO", "Desbloquear usuario"],
];

const entityOptions = [
  ["", "Todas las entidades"],
  ["AUTH", "Autenticación"],
  ["CLIENTE", "Cliente"],
  ["PRODUCTO", "Producto"],
  ["FACTURA", "Factura"],
  ["USUARIO", "Usuario"],
];

const humanize = (value) =>
  String(value || "No registrado")
    .toLowerCase()
    .replaceAll("_", " ")
    .replace(/^\w/, (letter) => letter.toUpperCase());

const emptyFilters = {
  userName: "",
  action: "",
  entityName: "",
  startDate: "",
  endDate: "",
};

export default function AuditLogPage() {
  const { showAlert } = useAppAlert();
  const [filters, setFilters] = useState(emptyFilters);
  const [users, setUsers] = useState([]);
  const [items, setItems] = useState([]);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const [loading, setLoading] = useState(false);
  const [selectedItem, setSelectedItem] = useState(null);

  useEffect(() => {
    api.get("/users")
      .then((response) => setUsers(response.data ?? []))
      .catch(() => setUsers([]));
  }, []);

  const load = async (
    currentPage = page,
    currentFilters = filters,
    currentPageSize = pageSize
  ) => {
    if (
      currentFilters.startDate &&
      currentFilters.endDate &&
      currentFilters.endDate < currentFilters.startDate
    ) {
      showAlert("La fecha fin no puede ser menor a la fecha inicio.", "warning");
      return;
    }

    try {
      setLoading(true);
      const params = Object.fromEntries(
        Object.entries({
          ...currentFilters,
          pageNumber: currentPage,
          pageSize: currentPageSize,
        }).filter(([, value]) => value !== "")
      );
      const response = await api.get("/audit-logs", { params });

      setItems(
        (response.data.items || []).map((item) => ({
          ...item,
          formattedDate: new Intl.DateTimeFormat("es-EC", {
            timeZone: "America/Guayaquil",
            dateStyle: "short",
            timeStyle: "medium",
          }).format(new Date(item.createdAt)),
          actionLabel: humanize(item.action),
          entityLabel: humanize(item.entityName),
          request: `${item.httpMethod} ${item.path}`,
        }))
      );
      setTotalPages(Math.max(response.data.totalPages || 1, 1));
      setTotalItems(response.data.totalItems || 0);
    } catch (error) {
      setItems([]);
      setTotalItems(0);
      showAlert(
        getApiErrorMessage(error, "No se pudo cargar la auditoría."),
        "error"
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const timeout = window.setTimeout(() => load(page, filters, pageSize), 0);
    return () => window.clearTimeout(timeout);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, pageSize]);

  const update = (field, value) =>
    setFilters((current) => ({ ...current, [field]: value }));

  const search = () => {
    setPage(1);
    load(1, filters, pageSize);
  };

  const clearFilters = () => {
    setFilters(emptyFilters);
    setPage(1);
    load(1, emptyFilters, pageSize);
  };

  return (
    <Layout>
      <section className="page-heading">
        <div>
          <span className="page-eyebrow">Seguridad</span>
          <h1>Auditoría general</h1>
          <p>Consulta las acciones relevantes realizadas por los usuarios.</p>
        </div>

        <div className="page-stat">
          <strong>{totalItems}</strong>
          <span>eventos encontrados</span>
        </div>
      </section>

      <div className="card">
        <div className="feature-scope-banner">
          <div>
            <strong>Acceso restringido</strong>
            <span>
              Esta información técnica está disponible únicamente para administradores.
            </span>
          </div>
          <span className="scope-badge">ADMINISTRADOR</span>
        </div>

        <div className="report-filter-grid">
          <label>
            Usuario
            <select
              value={filters.userName}
              onChange={(event) => update("userName", event.target.value)}
            >
              <option value="">Todos los usuarios</option>
              {users.map((user) => (
                <option key={user.id} value={user.userName}>
                  {user.fullName} ({user.userName})
                </option>
              ))}
            </select>
          </label>

          <label>
            Acción
            <select
              value={filters.action}
              onChange={(event) => update("action", event.target.value)}
            >
              {actionOptions.map(([value, label]) => (
                <option value={value} key={value || "all-actions"}>
                  {label}
                </option>
              ))}
            </select>
          </label>

          <label>
            Entidad
            <select
              value={filters.entityName}
              onChange={(event) => update("entityName", event.target.value)}
            >
              {entityOptions.map(([value, label]) => (
                <option value={value} key={value || "all-entities"}>
                  {label}
                </option>
              ))}
            </select>
          </label>

          <label>
            Fecha inicio
            <input
              type="date"
              value={filters.startDate}
              onChange={(event) => update("startDate", event.target.value)}
            />
          </label>

          <label>
            Fecha fin
            <input
              type="date"
              value={filters.endDate}
              onChange={(event) => update("endDate", event.target.value)}
            />
          </label>

          <button type="button" disabled={loading} onClick={search}>
            {loading ? "Buscando..." : "Buscar"}
          </button>
        </div>

        <button
          type="button"
          className="secondary-button"
          disabled={loading}
          onClick={clearFilters}
        >
          Limpiar filtros
        </button>
      </div>

      <div className="card">
        <DataTable
          columns={columns}
          data={items}
          emptyMessage={loading ? "Cargando auditoría..." : "No existen registros"}
          actions={(item) => (
            <button
              type="button"
              className="table-action"
              onClick={() => setSelectedItem(item)}
            >
              Ver detalle
            </button>
          )}
        />

        <div className="actions-row">
          <label className="page-size-control">
            Registros por página
            <select
              value={pageSize}
              onChange={(event) => {
                setPageSize(Number(event.target.value));
                setPage(1);
              }}
            >
              {[10, 15, 20, 30].map((size) => (
                <option key={size} value={size}>{size}</option>
              ))}
            </select>
          </label>
        </div>

        <Pagination
          page={page}
          totalPages={totalPages}
          onPrevious={() => setPage((value) => Math.max(value - 1, 1))}
          onNext={() => setPage((value) => Math.min(value + 1, totalPages))}
          onPageChange={setPage}
        />
      </div>

      {selectedItem && (
        <div className="audit-detail-overlay" role="dialog" aria-modal="true">
          <article className="audit-detail-card">
            <header className="audit-detail-header">
              <div>
                <span className="page-eyebrow">Evento #{selectedItem.id}</span>
                <h2>Detalle de auditoría</h2>
              </div>
              <button
                type="button"
                className="secondary-button"
                onClick={() => setSelectedItem(null)}
              >
                Cerrar
              </button>
            </header>

            <dl className="audit-detail-grid">
              <div><dt>Fecha</dt><dd>{selectedItem.formattedDate}</dd></div>
              <div><dt>Usuario</dt><dd>{selectedItem.userName || "Sistema"}</dd></div>
              <div><dt>Acción</dt><dd>{selectedItem.actionLabel}</dd></div>
              <div><dt>Entidad</dt><dd>{selectedItem.entityLabel}</dd></div>
              <div><dt>Identificador</dt><dd>{selectedItem.entityId || "No aplica"}</dd></div>
              <div><dt>Dirección IP</dt><dd>{selectedItem.ipAddress || "No registrada"}</dd></div>
              <div className="audit-detail-wide">
                <dt>Solicitud</dt>
                <dd>{selectedItem.request}</dd>
              </div>
            </dl>

            <div className="audit-values">
              <strong>Datos registrados</strong>
              <pre>{selectedItem.newValues || "La operación no almacenó datos adicionales."}</pre>
            </div>
          </article>
        </div>
      )}
    </Layout>
  );
}
