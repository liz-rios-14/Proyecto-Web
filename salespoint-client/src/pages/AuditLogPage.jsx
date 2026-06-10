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
  { key: "action", label: "Acción" },
  { key: "entityName", label: "Entidad" },
  { key: "request", label: "Solicitud" },
];

export default function AuditLogPage() {
  const { showAlert } = useAppAlert();
  const [filters, setFilters] = useState({
    userName: "",
    action: "",
    entityName: "",
    startDate: "",
    endDate: "",
  });
  const [items, setItems] = useState([]);
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [loading, setLoading] = useState(false);

  const load = async (currentPage = page) => {
    if (
      filters.startDate &&
      filters.endDate &&
      filters.endDate < filters.startDate
    ) {
      showAlert("La fecha fin no puede ser menor a la fecha inicio.", "warning");
      return;
    }

    try {
      setLoading(true);
      const response = await api.get("/audit-logs", {
        params: { ...filters, pageNumber: currentPage, pageSize },
      });
      setItems(
        (response.data.items || []).map((item) => ({
          ...item,
          formattedDate: new Intl.DateTimeFormat("es-EC", {
            dateStyle: "short",
            timeStyle: "medium",
          }).format(new Date(item.createdAt)),
          request: `${item.httpMethod} ${item.path}`,
        }))
      );
      setTotalPages(Math.max(response.data.totalPages || 1, 1));
    } catch (error) {
      setItems([]);
      showAlert(
        getApiErrorMessage(error, "No se pudo cargar la auditoría."),
        "error"
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    const timeout = window.setTimeout(() => load(page), 0);
    return () => window.clearTimeout(timeout);
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [page, pageSize]);

  const update = (field, value) =>
    setFilters((current) => ({ ...current, [field]: value }));

  return (
    <Layout>
      <section className="page-heading">
        <div>
          <span className="page-eyebrow">Seguridad</span>
          <h1>Auditoría general</h1>
          <p>Consulta las acciones relevantes realizadas por los usuarios.</p>
        </div>
      </section>

      <div className="card">
        <div className="report-filter-grid">
          <input
            placeholder="Usuario"
            value={filters.userName}
            onChange={(event) => update("userName", event.target.value)}
          />
          <input
            placeholder="Acción"
            value={filters.action}
            onChange={(event) => update("action", event.target.value)}
          />
          <input
            placeholder="Entidad"
            value={filters.entityName}
            onChange={(event) => update("entityName", event.target.value)}
          />
          <input
            type="date"
            value={filters.startDate}
            onChange={(event) => update("startDate", event.target.value)}
          />
          <input
            type="date"
            value={filters.endDate}
            onChange={(event) => update("endDate", event.target.value)}
          />
          <button type="button" disabled={loading} onClick={() => { setPage(1); load(1); }}>
            {loading ? "Buscando..." : "Buscar"}
          </button>
        </div>
      </div>

      <div className="card">
        <DataTable
          columns={columns}
          data={items}
          emptyMessage={loading ? "Cargando auditoría..." : "No existen registros"}
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
    </Layout>
  );
}
