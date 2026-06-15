import { useEffect, useState } from "react";
import Layout from "../components/Layout";
import DataTable from "../components/DataTable";
import Pagination from "../components/Pagination";
import { api } from "../api/apiClient";
import { getApiErrorMessage } from "../api/apiError";
import { useAppAlert } from "../components/AppAlert";
import { reportFrontendError } from "../api/errorReporter";
import {
  sanitizeDigits,
  sanitizeSingleSpacedText,
} from "../utils/inputSanitizers";

const searchFields = [
  { key: "", label: "Todos los campos" },
  { key: "id", label: "Identificador" },
  { key: "message", label: "Mensaje" },
  { key: "source", label: "Origen" },
  { key: "exceptionType", label: "Tipo de excepción" },
  { key: "path", label: "Ruta" },
  { key: "httpMethod", label: "Método HTTP" },
  { key: "userId", label: "Usuario" },
];

const columns = [
  { key: "id", label: "Id", type: "number" },
  { key: "formattedDate", label: "Fecha" },
  { key: "message", label: "Mensaje" },
  { key: "requestLabel", label: "Solicitud" },
  { key: "userLabel", label: "Usuario" },
];

const formatDate = (value) => {
  if (!value) return "No registrada";

  const date = new Date(value);

  if (Number.isNaN(date.getTime())) {
    return "Fecha inválida";
  }

  return new Intl.DateTimeFormat("es-EC", {
    timeZone: "America/Guayaquil",
    dateStyle: "medium",
    timeStyle: "medium",
  }).format(date);
};

export default function ErrorLogPage() {
  const { showAlert } = useAppAlert();

  const [logs, setLogs] = useState([]);
  const [users, setUsers] = useState([]);
  const [selectedLog, setSelectedLog] = useState(null);
  const [searchField, setSearchField] = useState("");
  const [searchValue, setSearchValue] = useState("");
  const [page, setPage] = useState(1);
  const [pageSize, setPageSize] = useState(10);
  const [totalPages, setTotalPages] = useState(1);
  const [totalItems, setTotalItems] = useState(0);
  const [loading, setLoading] = useState(false);

  useEffect(() => {
    api.get("/users")
      .then((response) => setUsers(response.data ?? []))
      .catch(() => setUsers([]));
  }, []);

  const getUserLabel = (userId, availableUsers = users) => {
    if (!userId) return "Sistema";

    const user = availableUsers.find((item) => Number(item.id) === Number(userId));
    return user ? `${user.fullName} (${user.userName})` : `Usuario ${userId}`;
  };

  const cleanSearchValue = (field, value) => {
    const cleanField = field?.trim().toLowerCase() ?? "";

    if (cleanField === "id" || cleanField === "userid") {
      return sanitizeDigits(value, 9);
    }

    return sanitizeSingleSpacedText(value, 180, { uppercase: false });
  };

  const buildParams = (currentPage, field, value) => {
    const params = {
      pageNumber: currentPage,
      pageSize,
    };

    const cleanField = field?.trim();
    const cleanValue = cleanSearchValue(cleanField, value ?? "").trim();

    if (cleanField && cleanValue) {
      params.field = cleanField;
      params.value = cleanValue;
    }

    return params;
  };

  const loadLogs = async (
    currentPage = page,
    field = searchField,
    value = searchValue
  ) => {
    try {
      setLoading(true);

      const response = await api.get("/error-logs", {
        params: buildParams(currentPage, field, value),
      });
      let currentUsers = users;

      if (currentUsers.length === 0) {
        try {
          const usersResponse = await api.get("/users");
          currentUsers = usersResponse.data ?? [];
          setUsers(currentUsers);
        } catch {
          currentUsers = [];
        }
      }

      const items = response.data?.items ?? [];

      setLogs(
        items.map((log) => ({
          ...log,
          formattedDate: formatDate(log.createdAt),
          requestLabel:
            [log.httpMethod, log.path].filter(Boolean).join(" ") ||
            log.source ||
            "No registrado",
          userLabel: getUserLabel(log.userId, currentUsers),
        }))
      );

      setTotalPages(Math.max(response.data?.totalPages ?? 1, 1));
      setTotalItems(response.data?.totalItems ?? 0);
    } catch (error) {
      setLogs([]);
      setTotalPages(1);
      setTotalItems(0);

      showAlert(
        getApiErrorMessage(
          error,
          "No se pudo cargar el registro de errores."
        ),
        "error"
      );
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    loadLogs(page);
  }, [page, pageSize]);

  const search = () => {
    const cleanField = searchField.trim();
    const cleanValue = cleanSearchValue(cleanField, searchValue).trim();

    if (cleanField && !cleanValue) {
      showAlert("Ingrese un valor para realizar la búsqueda.", "warning");
      return;
    }

    setPage(1);
    loadLogs(1, cleanField, cleanValue);
  };

  const clearSearch = () => {
    setSearchField("");
    setSearchValue("");
    setPage(1);
    loadLogs(1, "", "");
  };

  const createDiagnosticError = async () => {
    const registered = await reportFrontendError({
      source: "Frontend diagnóstico",
      message: "Error de prueba controlado generado desde la pantalla administrativa.",
      exceptionType: "FrontendDiagnosticError",
      httpMethod: "CLIENT",
      path: window.location.pathname,
      detail: {
        code: "FRONTEND_TEST_ERROR",
        explanation: "Prueba controlada: la aplicación debe continuar funcionando.",
      },
    });

    if (!registered) {
      showAlert("No se pudo registrar el error de prueba.", "error");
      return;
    }

    showAlert("Error de prueba registrado sin detener la aplicación.", "success");
    setPage(1);
    await loadLogs(1, "", "");
  };

  return (
    <Layout>
      <section className="page-heading">
        <div>
          <span className="page-eyebrow">Soporte técnico</span>
          <h1>Registro de errores</h1>
          <p>
            Consulta fallos técnicos relevantes registrados automáticamente
            por la API.
          </p>
        </div>

        <div className="page-stat">
          <strong>{totalItems}</strong>
          <span>errores encontrados</span>
        </div>
      </section>

      <div className="card">
        <h2>Buscar errores</h2>

        <div className="search-grid">
          <select
            value={searchField}
            onChange={(event) => setSearchField(event.target.value)}
          >
            {searchFields.map((field) => (
              <option key={field.key || "all"} value={field.key}>
                {field.label}
              </option>
            ))}
          </select>

          <input
            value={searchValue}
            placeholder="Ingrese el valor de búsqueda"
            onChange={(event) =>
              setSearchValue(cleanSearchValue(searchField, event.target.value))
            }
            onKeyDown={(event) => {
              if (event.key === "Enter") search();
            }}
          />

          <button type="button" onClick={search} disabled={loading}>
            {loading ? "Buscando..." : "Buscar"}
          </button>
        </div>

        <div className="actions-row">
          <button
            type="button"
            className="secondary-button"
            onClick={clearSearch}
            disabled={loading}
          >
            Limpiar búsqueda
          </button>

          <button
            type="button"
            className="secondary-button"
            onClick={createDiagnosticError}
            disabled={loading}
          >
            Registrar error de prueba
          </button>

          <label className="page-size-control">
            Registros por página
            <select
              value={pageSize}
              disabled={loading}
              onChange={(event) => {
                setPageSize(Number(event.target.value));
                setPage(1);
              }}
            >
              {[10, 15, 20, 30].map((size) => (
                <option key={size} value={size}>
                  {size}
                </option>
              ))}
            </select>
          </label>
        </div>
      </div>

      <div className="card">
        <h2>Historial técnico</h2>

        <DataTable
          columns={columns}
          data={logs}
          emptyMessage={
            loading
              ? "Cargando registros..."
              : "No existen errores con los filtros indicados"
          }
          actions={(log) => (
            <button
              type="button"
              className="table-action"
              onClick={() => setSelectedLog(log)}
            >
              Ver detalle
            </button>
          )}
        />

        <Pagination
          page={page}
          totalPages={totalPages}
          onPrevious={() => setPage((current) => Math.max(current - 1, 1))}
          onNext={() =>
            setPage((current) => Math.min(current + 1, totalPages))
          }
          onPageChange={setPage}
        />
      </div>

      {selectedLog && (
        <div
          className="error-detail-overlay"
          role="dialog"
          aria-modal="true"
          aria-labelledby="error-detail-title"
        >
          <article className="error-detail-card">
            <div className="error-detail-header">
              <div>
                <span className="page-eyebrow">
                  Incidencia #{selectedLog.id}
                </span>
                <h2 id="error-detail-title">Detalle del error</h2>
              </div>

              <button
                type="button"
                className="secondary-button"
                onClick={() => setSelectedLog(null)}
              >
                Cerrar
              </button>
            </div>

            <dl className="error-detail-grid">
              <div>
                <dt>Fecha</dt>
                <dd>{selectedLog.formattedDate}</dd>
              </div>

              <div>
                <dt>Usuario responsable</dt>
                <dd>{selectedLog.userLabel}</dd>
              </div>

              <div>
                <dt>Método</dt>
                <dd>{selectedLog.httpMethod || "No registrado"}</dd>
              </div>

              <div>
                <dt>Ruta</dt>
                <dd>{selectedLog.path || "No registrada"}</dd>
              </div>

              <div className="error-detail-wide">
                <dt>Tipo de excepción</dt>
                <dd>{selectedLog.exceptionType || "No registrado"}</dd>
              </div>

              <div className="error-detail-wide">
                <dt>Mensaje</dt>
                <dd>{selectedLog.message}</dd>
              </div>
            </dl>

            <div className="error-technical-detail">
              <strong>Detalle técnico</strong>
              <pre>{selectedLog.detail || "Sin detalle técnico."}</pre>
            </div>
          </article>
        </div>
      )}
    </Layout>
  );
}
